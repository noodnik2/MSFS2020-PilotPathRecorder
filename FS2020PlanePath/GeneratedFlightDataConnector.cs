using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace FS2020PlanePath
{

    public interface IFlightDataGenerator
    {
        string Name { get; }
        IEnumerator<FlightPathData> NextFlightPathSegment();
        void Reset();
    }

    public class GeneratedFlightDataConnector : IFlightDataConnector
    {

        public GeneratedFlightDataConnector(
            Control parentControl,
            Action<FlightDataStructure> simPlaneDataHandler,
            Action<EnvironmentDataStructure> simPlaneEnvironmentChangeHandler,
            Action<Exception> exceptionHandler,
            IFlightDataGenerator flightDataGenerator
        )
        {
            this.parentControl = parentControl;
            this.simPlaneDataHandler = simPlaneDataHandler;
            this.simPlaneEnvironmentChangeHandler = simPlaneEnvironmentChangeHandler;
            this.exceptionHandler = exceptionHandler;
            this.flightDataGenerator = flightDataGenerator;
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
                    title = flightDataGenerator.Name
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
                //Console.WriteLine("ignoring connect call; already connected");
                return false;
            }

            try
            {
                flightDataGenerator.Reset();
            }
            catch (Exception e)
            {
                this.exceptionHandler(e);
                return false;
            }

            timer.Enabled = true;
            return true;
        }

        public void CloseConnection()
        {
            if (!IsSimConnected())
            {
                //Console.WriteLine("ignoring close call; not connected");
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
                //Console.WriteLine("ignoring initialize call; not connected");
                return;
            }
            if (!IsSimInitialized())
            {
                //Console.WriteLine("ignoring initialize call; already initialized");
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
                        //Console.WriteLine("HandleWindowMessage");
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
            IEnumerator<FlightPathData> flightPathDataEnumerator = flightDataGenerator.NextFlightPathSegment();
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

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        private const int WM_USER_SIMCONNECT = 0x0402;

        private Timer timer;
        private Control parentControl;
        private Action<FlightDataStructure> simPlaneDataHandler;
        private Action<EnvironmentDataStructure> simPlaneEnvironmentChangeHandler;
        private Action<Exception> exceptionHandler;
        private IFlightDataGenerator flightDataGenerator;

    }

}
