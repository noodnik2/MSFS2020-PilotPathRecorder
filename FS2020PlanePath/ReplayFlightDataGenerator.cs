using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace FS2020PlanePath
{
    public class ReplayFlightDataGenerator : IFlightDataGenerator
    {

        public ReplayFlightDataGenerator(
            Control parentControl,
            Action<FlightDataStructure> simPlaneDataHandler,
            Action<EnvironmentDataStructure> simPlaneEnvironmentChangeHandler,
            Action<Exception> exceptionHandler
        )
        {
            this.parentControl = parentControl;
            this.simPlaneDataHandler = simPlaneDataHandler;
            this.simPlaneEnvironmentChangeHandler = simPlaneEnvironmentChangeHandler;
            this.exceptionHandler = exceptionHandler;
            timer = new Timer();
            timer.Enabled = false;
            timer.Interval = 5000;  // TODO parameterize
            timer.Tick += TimerTickHandler;
        }

        public void GetSimEnvInfo()
        {
            simPlaneEnvironmentChangeHandler.Invoke(
                new EnvironmentDataStructure
                {
                    title = "Replay"
                }
            );
        }

        public bool IsSimConnected()
        {
            return timer.Enabled;
        }

        public bool Connect()
        {
            if (IsSimConnected())
            {
                Console.WriteLine("ignoring connect call; already connected");
                return false;
            }

            timer.Enabled = true;
            return true;
        }

        public void CloseConnection()
        {
            if (!IsSimConnected())
            {
                Console.WriteLine("ignoring close call; not connected");
                return;
            }
            timer.Stop();
            timer.Enabled = false;
        }

        public bool IsSimInitialized()
        {
            return timer.Enabled;
        }

        public void Initialize()
        {
            if (!IsSimConnected())
            {
                Console.WriteLine("ignoring initialize call; not connected");
                return;
            }
            if (!IsSimInitialized())
            {
                Console.WriteLine("ignoring initialize call; already initialized");
                return;
            }
            timer.Start();
        }

        public bool HandleWindowMessage(ref Message m)
        {
            if (m.Msg == WM_USER_SIMCONNECT)
            {
                if (IsSimInitialized())
                {
                    try
                    {
                        Console.WriteLine("randomWalk HandleWindowMessage");
                        pumpFlightDataUpdates();
                    }
                    catch (Exception ex)
                    {
                        exceptionHandler.Invoke(ex);
                    }
                }
                return true;
            }
            return false;
        }

        private void TimerTickHandler(object sender, EventArgs e)
        {
            PostMessage(parentControl.Handle, WM_USER_SIMCONNECT, 0, 0);
        }

        private void pumpFlightDataUpdates()
        {
            IEnumerator<FlightPathData> flightPathDataEnumerator = GetLiveCamTrackSinceDateTimestamp(0, 0);
            while (flightPathDataEnumerator.MoveNext())
            {
                FlightPathData flightPathData = flightPathDataEnumerator.Current;
                simPlaneDataHandler.Invoke(
                    new FlightDataStructure
                    {
                        latitude = flightPathData.latitude,
                        longitude = flightPathData.longitude,
                        altitude = flightPathData.altitude,
                        altitude_above_ground = flightPathData.altitudeaboveground,
                        engine1rpm = flightPathData.Eng1Rpm,
                        engine2rpm = flightPathData.Eng2Rpm,
                        engine3rpm = flightPathData.Eng3Rpm,
                        engine4rpm = flightPathData.Eng4Rpm,
                        lightsmask = flightPathData.LightsMask,
                        ground_velocity = flightPathData.ground_velocity,
                        plane_pitch = flightPathData.plane_pitch,
                        plane_bank = flightPathData.plane_bank,
                        plane_heading_true = flightPathData.plane_heading_true,
                        plane_heading_magnetic = flightPathData.plane_heading_magnetic,
                        plane_airspeed_indicated = flightPathData.plane_airspeed_indicated,
                        airspeed_true = flightPathData.airspeed_true,
                        vertical_speed = flightPathData.vertical_speed,
                        heading_indicator = flightPathData.heading_indicator,
                        flaps_handle_position = flightPathData.flaps_handle_position,
                        spoilers_handle_position = flightPathData.spoilers_handle_position,
                        gear_handle_position = flightPathData.gear_handle_position,
                        ambient_wind_velocity = flightPathData.ambient_wind_velocity,
                        ambient_wind_direction = flightPathData.ambient_wind_direction,
                        ambient_temperature = flightPathData.ambient_temperature,
                        stall_warning = flightPathData.stall_warning,
                        overspeed_warning = flightPathData.overspeed_warning,
                        is_gear_retractable = flightPathData.is_gear_retractable,
                        spoiler_available = flightPathData.spoiler_available,
                        //gps_wp_prev_latitude = flightPathData.gps_wp_prev_latitude,
                        //gps_wp_prev_longitude = flightPathData.gps_wp_prev_longitude,
                        //gps_wp_prev_altitude = flightPathData.gps_wp_prev_altitude,
                        //gps_wp_prev_id = flightPathData.gps_wp_prev_id,
                        //gps_wp_next_latitude = flightPathData.gps_wp_next_latitude,
                        //gps_wp_next_longitude = flightPathData.gps_wp_next_longitude,
                        //gps_wp_next_altitude = flightPathData.gps_wp_next_altitude,
                        //gps_wp_next_id = flightPathData.gps_wp_next_id,
                        //gps_flight_plan_wp_index = flightPathData.gps_flight_plan_wp_index,
                        //gps_flight_plan_wp_count = flightPathData.gps_flight_plan_wp_count,
                        sim_on_ground = flightPathData.sim_on_ground
                    }
                );
            }
        }

        private IEnumerator<FlightPathData> GetLiveCamTrackSinceDateTimestamp(int pk, long earliestTimestamp)
        {
            long ticksPerSample = System.TimeSpan.TicksPerSecond / 2;
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
                yield return (
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

        }

        // TODO parameterize random walk seed
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

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        private const int WM_USER_SIMCONNECT = 0x0402;

        private Timer timer;
        private Control parentControl;
        private Action<FlightDataStructure> simPlaneDataHandler;
        private Action<EnvironmentDataStructure> simPlaneEnvironmentChangeHandler;
        private Action<Exception> exceptionHandler;
    }

}
