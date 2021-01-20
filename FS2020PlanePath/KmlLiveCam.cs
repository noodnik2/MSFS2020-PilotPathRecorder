using System;
using System.Reflection;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using SharpKml.Base;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting;

namespace FS2020PlanePath
{

    public class KmlLiveCam : ILiveCam<KmlCameraParameterValues, KmlNetworkLinkValues>
    {

        public KmlLiveCam(string cameraTemplate, string linkTemplate)
        {
            _camera = NewRenderer<KmlCameraParameterValues>(cameraTemplate);
            _link = NewRenderer<KmlNetworkLinkValues>(linkTemplate);
        }

        public IStringTemplateRenderer<KmlCameraParameterValues> Camera { get { return _camera; } }

        public IStringTemplateRenderer<KmlNetworkLinkValues> Link { get { return _link; } }

        public string[] Diagnostics
        {
            get
            {
                return Camera.Diagnostics.Concat(Link.Diagnostics).ToArray();
            }
        }


        private IStringTemplateRenderer<KmlCameraParameterValues> _camera;
        private IStringTemplateRenderer<KmlNetworkLinkValues> _link;

        /// <typeparam name="T">type of the parameter structure referenced in the template</typeparam>
        /// <param name="template">text of the template to use</param>
        /// <returns>new renderer instance, using the specified template</returns>
        private IStringTemplateRenderer<T> NewRenderer<T>(string template)
        {
            IStringTemplateRenderer<T> renderer;
            if (template.TrimStart().StartsWith("<"))
            {
                // use the template renderer for XML text
                renderer = new KmlTemplateRenderer<T>();
            }
            else
            {
                // else compile it as a script
                renderer = new ScriptTemplateRenderer<T>();
            }
            renderer.Template = template;
            return renderer;
        }

    }

    // see: https://developers.google.com/kml/documentation/kmlreference#camera
    public class KmlCameraParameterValues
    {
        public double longitude { get; set; }
        public double latitude { get; set; }
        public double altitude { get; set; }    // meters
        public double heading { get; set; }
        public double tilt { get; set; }
        public double roll { get; set; }
        public int seq { get; set; }    // update sequence
    }

    // see: https://developers.google.com/kml/documentation/kmlreference#networklink
    public class KmlNetworkLinkValues
    {
        public string url { get; set; }
        public string alias { get; set; }

    }

    public class ScriptTemplateRenderer<V> : IStringTemplateRenderer<V>
    {

        public string Render(V values)
        {
            Task<ScriptState<string>> scriptStateTask = _script.RunAsync(values);
            if (!scriptStateTask.Wait(2000))
            {
                Console.WriteLine("ERROR: script execution timed out");
                return "";
            }
            return scriptStateTask.Result.ReturnValue;
        }

        public string Template { 
            get { return _template; } 
            set
            {
                _template = value;
                _script = CSharpScript.Create<string>(
                    code: _template,
                    globalsType: typeof(V)
                );
            }
        }

        public string[] Diagnostics
        {
            get
            {
                List<string> problems = (
                    _script.Compile()
                    .Where(d => d.Severity >= DiagnosticSeverity.Warning)
                    .Select(d => d.ToString())
                    .Take(10)
                    .ToList()
                );
                if (problems.Count == 0)
                {
                    return new string[0];
                }

                return (
                    new List<string> { "C# Script:" }
                    .Concat(problems)
                    .ToArray()
                );
            }
        }

        private Script<string> _script;

        private string _template;

    }

    public class KmlTemplateRenderer<V> : IStringTemplateRenderer<V>
    {

        public string Render(V values)
        {
            return TemplateRenderer.Render<V>(Template, values);
        }

        public string Template { get; set; }

        public string[] Diagnostics
        {
            get
            {
                try
                {
                    new Parser().ParseString(Template, true);
                    return new string[0];
                }
                catch (Exception pe)
                {
                    return new string[] { 
                        "KML Parser:",
                        pe.Message 
                    };
                }
            }
        }
    }

    public static class TemplateRenderer
    {

        public static string Render<V>(string template, V values)
        {
            string result = template;
            foreach (PropertyInfo info in typeof(V).GetProperties())
            {
                string substitutionTokenName = $"{{{info.Name}}}";
                string substitutionTokenValue = info.GetValue(values).ToString();
                while (true)
                {
                    string newResult = result.Replace(substitutionTokenName, substitutionTokenValue);
                    if (newResult == result)
                    {
                        break;
                    }
                    result = newResult;
                }
            }
            return result;
        }

        public static string[] Placeholders(Type valueType)
        {
            List<string> placeholders = new List<string>();
            foreach (PropertyInfo info in valueType.GetProperties())
            {
                placeholders.Add($"{{{info.Name}}}");
            }
            return placeholders.ToArray();
        }

    }

    /** A "Live Cam" has a "Camera", which can be "called back" repeatedly through a "Link" */
    public interface ILiveCam<CV, LV>
    {

        /** A "Camera" renders a snapshot using current values */
        IStringTemplateRenderer<CV> Camera { get; }

        /** A "Link" renders a callback to the "Camera" */
        IStringTemplateRenderer<LV> Link { get; }

        string[] Diagnostics { get; }

    }

    /** A "String Template Renderer" can render a string template using a current set of values */
    public interface IStringTemplateRenderer<V>
    {

        string Render(V values);
        string Template { get; set; }
        string[] Diagnostics { get; }
    }

}
