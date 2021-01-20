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

    /// <summary>
    /// Generalized template renderer which will delegate rendering of the passed template to the
    /// appropriate renderer, based upon hints as to the template's type (e.g., KML or Script).
    /// </summary>
    /// <typeparam name="T">object whose properties can be referenced by the template</typeparam>
    public class GenericTemplateRenderer<T> : IStringTemplateRenderer<T>
    {

        private IStringTemplateRenderer<T> renderer;

        public string Render(T propertyValues)
        {
            return renderer.Render(propertyValues);
        }

        public string Template { get => renderer.Template; }

        public string[] Diagnostics => renderer.Diagnostics;

        public GenericTemplateRenderer(string template)
        {
            if (template.TrimStart().StartsWith("<"))
            {
                // template is KML text with placeholders
                renderer = new KmlTemplateRenderer<T>(template);
            }
            else
            {
                // template is a script
                renderer = new ScriptTemplateRenderer<T>(template);
            }
        }

    }

    /// <summary>
    /// C# Script whose string return value will become the rendered result
    /// </summary>
    /// <typeparam name="V">type of object whose property values will be available to the script during its execution</typeparam>
    public class ScriptTemplateRenderer<V> : IStringTemplateRenderer<V>
    {

        private Script<string> _script;

        public ScriptTemplateRenderer(string scriptTemplate)
        {
            _script = CSharpScript.Create<string>(
                code: scriptTemplate,
                globalsType: typeof(V)
            );
        }

        public string Render(V propertyValues)
        {
            Task<ScriptState<string>> scriptStateTask = _script.RunAsync(propertyValues);
            if (!scriptStateTask.Wait(2000))
            {
                Console.WriteLine("ERROR: script execution timed out");
                return "";
            }
            return scriptStateTask.Result.ReturnValue;
        }

        public string Template { get => _script.Code; }

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

    }

    /// <summary>
    /// KML Document text renderer
    /// </summary>
    /// <typeparam name="V">type of object whose properties supply subsitution values into the template upon rendering</typeparam>
    public class KmlTemplateRenderer<V> : IStringTemplateRenderer<V>
    {

        private string kmlTemplate;

        public KmlTemplateRenderer(string kmlTemplate)
        {
            this.kmlTemplate = kmlTemplate;
        }

        public string Render(V propertyValues)
        {
            return TextRenderer.Render<V>(Template, propertyValues);
        }

        public string Template { get => kmlTemplate; }

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

    public static class TextRenderer
    {

        /// <typeparam name="V">type of the 'propertyValues' object</typeparam>
        /// <param name="textTemplate">textual template optionally containing property references for substitution</param>
        /// <param name="propertyValues">object whose properties can be referenced within, and substituted into the text template</param>
        /// <returns>a 'rendering' of 'textTemplate' with all property referencess substituted using values from 'propertyValues'</returns>
        public static string Render<V>(string textTemplate, V propertyValues)
        {
            string textResult = textTemplate;
            foreach (PropertyInfo property in typeof(V).GetProperties())
            {
                string propertyName = $"{{{property.Name}}}";
                object propertyValue = property.GetValue(propertyValues);
                string substitutionValue = (propertyValue == null) ? "" : propertyValue.ToString();
                while (true)
                {
                    string newResult = textResult.Replace(propertyName, substitutionValue);
                    if (newResult == textResult)
                    {
                        break;
                    }
                    textResult = newResult;
                }
            }
            return textResult;
        }

        /// <param name="propertyValueType">type of object whose properties can be referenced within a template</param>
        /// <returns>array of property references that can be used within a template</returns>
        public static string[] Placeholders(Type propertyValueType)
        {
            List<string> placeholders = new List<string>();
            foreach (PropertyInfo info in propertyValueType.GetProperties())
            {
                placeholders.Add($"{{{info.Name}}}");
            }
            return placeholders.ToArray();
        }

    }

    /// <summary>
    /// A "String Template Renderer" can render a string template using a current set of property values
    /// </summary>
    /// <typeparam name="V">type of object whose properties supply subsitution values into the template upon rendering</typeparam>
    public interface IStringTemplateRenderer<V>
    {

        string Render(V propertyValues);
        string Template { get; }
        string[] Diagnostics { get; }
    }

}
