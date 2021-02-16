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
                flightPath.Add(CopyOf(currentSample));

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

        private static FlightPathData CopyOf(FlightPathData source)
        {
            return new FlightPathData
            {
                timestamp = source.timestamp,
                longitude = source.longitude,
                latitude = source.latitude,
                altitude = source.altitude,
                plane_pitch = source.plane_pitch,
                plane_bank = source.plane_bank,
                plane_heading_true = source.plane_heading_true,
                ground_velocity = source.ground_velocity,

                vertical_speed = source.vertical_speed,
                airspeed_true = source.airspeed_true,
                heading_indicator = source.heading_indicator,
                plane_airspeed_indicated = source.plane_airspeed_indicated,
                plane_heading_magnetic = source.plane_heading_magnetic,
                altitudeaboveground = source.altitudeaboveground,
                Eng1Rpm = source.Eng1Rpm,
                Eng2Rpm = source.Eng2Rpm,
                Eng3Rpm = source.Eng3Rpm,
                Eng4Rpm = source.Eng4Rpm,
                LightsMask = source.LightsMask,
                flaps_handle_position = source.flaps_handle_position,
                spoilers_handle_position = source.spoilers_handle_position,
                gear_handle_position = source.gear_handle_position,
                ambient_wind_velocity = source.ambient_wind_velocity,
                ambient_wind_direction = source.ambient_wind_direction,
                ambient_temperature = source.ambient_temperature,
                stall_warning = source.stall_warning,
                overspeed_warning = source.overspeed_warning,
                is_gear_retractable = source.is_gear_retractable,
                spoiler_available = source.spoiler_available,
                sim_on_ground = source.sim_on_ground
            };
        }

    }

}
