using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FS2020PlanePath
{
    public class GeoCalcUtils
    {

        public const double PI = 3.14159265359;
        public const double Rnm = 3_440.1;   // ~ earth's radius in nautical miles

        /// <summary>
        /// Calculates coordinates resulting from travel from initial coordinate along bearing for distance.
        /// </summary>
        /// <param name="latitude">initial latitude</param>
        /// <param name="longitude">initial longitude</param>
        /// <param name="bearing">direction of travel from the initial position</param>
        /// <param name="distance">distance (nautical miles)</param>
        // <returns>new (latitude, longitude) coordinates after travel</returns>
        public static (double, double) calcLatLonOffset(double latitude, double longitude, double bearing, double distance)
        {

            double lat1Rad = degreesToRadians(latitude);
            double lon1Rad = degreesToRadians(longitude);
            double brngRad = degreesToRadians(bearing);

            double distRad = distance / Rnm;
            double lat2Rad = Math.Asin(Math.Sin(lat1Rad) * Math.Cos(distRad) + Math.Cos(lat1Rad) * Math.Sin(distRad) * Math.Cos(brngRad));
            double lon2Rad = lon1Rad + Math.Atan2(Math.Sin(brngRad) * Math.Sin(distRad) * Math.Cos(lat1Rad), Math.Cos(distRad) - Math.Sin(lat1Rad) * Math.Sin(lat2Rad));

            return (radiansToDegrees(lat2Rad), radiansToDegrees(lon2Rad));
        }

        public static double normalizeIso6709Coordinate(double iso6709Coordinate)
        {
            return normalizeGeoCoordinate(iso6709Coordinate, 180);
        }

        public static double normalizeCompassCoordinate(double compassCoordinate)
        {
            return normalizeGeoCoordinate(compassCoordinate, 360);
        }

        public static string pcoord((double lat, double lon) coord)
        {
            return $"{coord.lat},{coord.lon}";
        }

        private static double degreesToRadians(double degrees)
        {
            return degrees / 180 * PI; 
        }

        private static double radiansToDegrees(double radians)
        {
            return radians / PI * 180;
        }

        private static double normalizeGeoCoordinate(double coordinate, double limit)
        {
            double normalizedCoordinate = coordinate;
            while (normalizedCoordinate < -Math.Abs(limit))
            {
                normalizedCoordinate += 360;
            }
            while (normalizedCoordinate > Math.Abs(limit))
            {
                normalizedCoordinate -= 360;
            }
            return normalizedCoordinate;
        }

    }
}
