using Xunit;
using Xunit.Abstractions;
using System;

namespace FS2020PlanePath.XUnitTests
{

    public class KmlLiveCamTests
    {
        private readonly ITestOutputHelper logger;
        private MultiTrackKmlGenerator multiTrackKmlGenerator = new MultiTrackKmlGenerator();

        public KmlLiveCamTests(ITestOutputHelper logger)
        {
            this.logger = logger;
        }

        [Fact]
        public void TestCamerTemplateTextRendering()
        {
            var kmlLiveCam = new KmlLiveCam("<cam({seq})", "<link({alias},{url})");
            KmlCameraParameterValues camValues = new KmlCameraParameterValues
            {
                seq = 99
            };
            KmlNetworkLinkValues linkValues = new KmlNetworkLinkValues {
                alias = "a76",
                url = "u76"
            };
            Assert.Equal("<cam(99)", kmlLiveCam.Camera.Render(camValues));
            Assert.Equal("<link(a76,u76)", kmlLiveCam.Link.Render(linkValues));
        }

        [Fact]
        public void TestCamerTemplateScriptRendering()
        {
            var kmlLiveCam = new KmlLiveCam("return $\"seq({seq})\";", "return $\"url({url})\";");
            KmlCameraParameterValues camValues = new KmlCameraParameterValues { seq = 678 };
            KmlNetworkLinkValues linkValues = new KmlNetworkLinkValues { url = "hey!" };
            Assert.Equal("seq(678)", kmlLiveCam.Camera.Render(camValues));
            Assert.Equal("url(hey!)", kmlLiveCam.Link.Render(linkValues));
        }

        [Fact]
        public void GenerateMultiTrackKmlDocTest()
        {
            Random random = new Random();
            int seq = 0;
            double lat0 = 37.7749;
            double lon0 = 122.4194;
            double alt0 = 1000;
            double hdg0 = random.Next(360);
            double pit0 = 0;
            double rol0 = 0;
            for (int i = 1; i < 5; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    double latOffset = seq * random.NextDouble() * 0.001;
                    double lonOffset = seq * random.NextDouble() * 0.002;
                    double altOffset = seq * random.NextDouble() * 10;
                    addCameraValues(
                        seq, 
                        lat0 + latOffset,
                        lon0 + lonOffset, 
                        alt0 + altOffset,
                        hdg0 + random.Next(3),
                        pit0 + random.Next(5),
                        rol0 + random.Next(10)
                    );
                    seq = seq + 1;
                }
                logger.WriteLine($"r{seq}({multiTrackKmlGenerator.GetMultiTrackKml()})");
            }
        }

        private void addCameraValues(int seq, double lat, double lon, double alt, double hdg, double pit, double rol)
        {
            multiTrackKmlGenerator.AddKmlCameraParameterValues(
                new KmlCameraParameterValues
                {
                    seq = seq,
                    latitude = lat,
                    longitude = lon,
                    altitude = alt,
                    heading = hdg,
                    tilt = pit,
                    roll = rol
                }
            );
        }

    }

}
