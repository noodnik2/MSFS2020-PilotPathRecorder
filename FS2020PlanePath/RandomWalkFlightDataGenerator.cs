using System;
using System.Collections.Generic;

namespace FS2020PlanePath
{

    public class RandomWalkFlightDataGenerator : AbstractFlightDataGenerator
    {

        public override string Name => "RandomWalk";

        internal override List<FlightPathData> GetFlightPathSince(long earliestTimestamp)
        {
            List<FlightPathData> flightPath = new List<FlightPathData>();
            long ticksPerSample = TimeSpan.TicksPerSecond / 2;
            Random random = new Random();

            for (
                currentSample.timestamp = Math.Max(currentSample.timestamp, earliestTimestamp) + ticksPerSample;
                currentSample.timestamp <= DateTime.Now.Ticks;
                currentSample.timestamp += ticksPerSample
            )
            {

                double distancePerHr = currentSample.ground_velocity += random.NextDouble() - 0.5;
                double distancePerTick = distancePerHr / (3600 * System.TimeSpan.TicksPerSecond);
                double distancePerSample = distancePerTick * ticksPerSample;

                double bearing = GeoCalcUtils.rationalizedCompassDirection(
                    currentSample.plane_heading_true + 5 * (random.NextDouble() - 0.5)
                );

                (double lat, double lon) to = GeoCalcUtils.calcLatLonOffset(
                    GeoCalcUtils.normalizedIso6709GeoDirection(currentSample.latitude),
                    GeoCalcUtils.normalizedIso6709GeoDirection(currentSample.longitude),
                    bearing,
                    distancePerSample
                );

                //Console.WriteLine($"brg({bearing}),v({distancePerHr}),d({distancePerSample}),p({GeoCalcUtils.pcoord(to)}");

                currentSample.ground_velocity = distancePerHr;
                currentSample.plane_heading_true = bearing;
                currentSample.latitude = to.lat;
                currentSample.longitude = to.lon;
                flightPath.Add(
                    new FlightPathData
                    {
                        timestamp = currentSample.timestamp,
                        longitude = currentSample.longitude,
                        latitude = currentSample.latitude,
                        altitude = currentSample.altitude,
                        plane_pitch = currentSample.plane_pitch,
                        plane_bank = currentSample.plane_bank,
                        plane_heading_true = currentSample.plane_heading_true,
                        ground_velocity = currentSample.ground_velocity
                    }
                );

            }

            return flightPath;
        }

        private FlightPathData currentSample = new FlightPathData
        {
            timestamp = DateTime.Now.Ticks,
            longitude = -121.6601805,
            latitude = 38.0282797,
            altitude = 2000,
            plane_pitch = 0,
            plane_bank = 0,
            plane_heading_true = 200,
            ground_velocity = 40           // nm/hr
        };

    }

}
