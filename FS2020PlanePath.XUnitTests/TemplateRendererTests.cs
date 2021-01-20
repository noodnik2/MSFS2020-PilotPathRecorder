using Xunit;
using Xunit.Abstractions;

namespace FS2020PlanePath.XUnitTests
{

    public class RenderValues
    {
        public int iv1 { get; set; }
        public int iv2 { get; set; }
        public string sv1 { get; set; } = null;
    }

    public class TemplateRendererTests
    {

        private readonly ITestOutputHelper logger;

        public TemplateRendererTests(ITestOutputHelper logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Demonstrate basic and comparable functioning of template renderers
        /// supported by the "Generic" template renderer.
        /// </summary>
        [Fact]
        public void BasicGenericTemplateRendererTest()
        {
            string[] templateVersions =
            {
                "<kml iv1='{iv1}' sv1='{sv1}' />",          // KML template
                "@$\"<kml iv1='{iv1}' sv1='{sv1}' />\""     // Script template
            };
            foreach (string templateVersion in templateVersions)
            {
                IStringTemplateRenderer<RenderValues> testRenderer = new GenericTemplateRenderer<RenderValues>(templateVersion);
                Assert.Equal("<kml iv1='9965' sv1='' />", testRenderer.Render(new RenderValues { iv1 = 9965 }));
            }
        }

    }
}
