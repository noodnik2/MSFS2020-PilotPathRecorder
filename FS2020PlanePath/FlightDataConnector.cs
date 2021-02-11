using System;
using System.Windows.Forms;

namespace FS2020PlanePath
{

    public interface IFlightDataConnector
    {
        void GetSimEnvInfo();
        bool IsSimConnected();
        bool Connect();
        void CloseConnection();
        bool IsSimInitialized();
        void Initialize();
        bool HandleWindowMessage(ref Message m);
    }

    public class FlightDataConnector : IFlightDataConnector
    {

        internal FlightDataConnector(
            OperationalMode operationalMode,
            Control parentControl,
            Action<FlightDataStructure> interfacePlaneDataHandler,
            Action<EnvironmentDataStructure> interfaceEnvironmentDataHandler,
            Action<Exception> exceptionHandler,
            FS2020_SQLLiteDB dbAccessor,
            Func<ReplayFlightDataGenerator.Context> replayContextSupplier
        )
        {
            simConnectConnector = new SimConnectFlightDataConnector(
                parentControl,
                simConnectData => interfacePlaneDataHandler.Invoke(
                    FlightData(simConnectData)
                ),
                simConnectEnvironmentData => interfaceEnvironmentDataHandler.Invoke(
                    EnvironmentData(simConnectEnvironmentData)
                ),
                exceptionHandler
            );
            randomWalkConnector = new GeneratedFlightDataConnector(
                parentControl,
                interfacePlaneDataHandler,
                interfaceEnvironmentDataHandler,
                exceptionHandler,
                new RandomWalkFlightDataGenerator()
            );
            replayConnector = new GeneratedFlightDataConnector(
                parentControl,
                interfacePlaneDataHandler,
                interfaceEnvironmentDataHandler,
                exceptionHandler,
                new ReplayFlightDataGenerator(dbAccessor, replayContextSupplier)
            );
            this.operationalMode = operationalMode;
        }

        public OperationalMode Mode
        {
            get => operationalMode;
            set
            {
                if (value == operationalMode)
                {
                    //Console.WriteLine($"call ignored; operational mode already({value})");
                    return;
                }
                if (IsSimConnected())
                {
                    CloseConnection();
                }
                //Console.WriteLine($"setting operationalMode({value})");
                operationalMode = value;
            }
        }

        public bool Connect()
        {
            //Console.WriteLine($"connecting({operationalMode})");
            return DelegateConnector.Connect();
        }

        public void CloseConnection()
        {
            //Console.WriteLine($"disconnecting({operationalMode})");
            DelegateConnector.CloseConnection();
        }

        public void Initialize()
        {
            //Console.WriteLine($"initializing({operationalMode})");
            DelegateConnector.Initialize();
        }

        public bool IsSimConnected()
        {
            return DelegateConnector.IsSimConnected();
        }

        public bool IsSimInitialized()
        {
            return DelegateConnector.IsSimInitialized();
        }

        public void GetSimEnvInfo()
        {
            DelegateConnector.GetSimEnvInfo();
        }

        public bool HandleWindowMessage(ref Message m)
        {
            return DelegateConnector.HandleWindowMessage(ref m);
        }

        public enum OperationalMode
        {
            SimConnect,
            RandomWalk,
            Replay
        }

        private IFlightDataConnector DelegateConnector {
            get
            {
                switch (operationalMode)
                {

                    case OperationalMode.SimConnect:
                        return simConnectConnector;

                    case OperationalMode.Replay:
                        return replayConnector;

                    case OperationalMode.RandomWalk:
                        return randomWalkConnector;

                    default:
                        return default(IFlightDataConnector);
                }
            }
        }

        private static EnvironmentDataStructure EnvironmentData(
            SimConnectFlightDataConnector.SimEnvironmentDataStructure newSimConnectEnvironmentStructure
        )
        {
            return new EnvironmentDataStructure
            {
                title = newSimConnectEnvironmentStructure.title
            };
        }

        private static FlightDataStructure FlightData(
            SimConnectFlightDataConnector.SimPlaneDataStructure newSimConnectData
        )
        {
            return new FlightDataStructure
            {
                latitude = newSimConnectData.latitude,
                longitude = newSimConnectData.longitude,
                altitude = newSimConnectData.altitude,
                altitude_above_ground = newSimConnectData.altitude_above_ground,
                engine1rpm = newSimConnectData.engine1rpm,
                engine2rpm = newSimConnectData.engine2rpm,
                engine3rpm = newSimConnectData.engine3rpm,
                engine4rpm = newSimConnectData.engine4rpm,
                lightsmask = newSimConnectData.lightsmask,
                ground_velocity = newSimConnectData.ground_velocity,
                plane_pitch = newSimConnectData.plane_pitch,
                plane_bank = newSimConnectData.plane_bank,
                plane_heading_true = newSimConnectData.plane_heading_true,
                plane_heading_magnetic = newSimConnectData.plane_heading_magnetic,
                plane_airspeed_indicated = newSimConnectData.plane_airspeed_indicated,
                airspeed_true = newSimConnectData.airspeed_true,
                vertical_speed = newSimConnectData.vertical_speed,
                heading_indicator = newSimConnectData.heading_indicator,
                flaps_handle_position = newSimConnectData.flaps_handle_position,
                spoilers_handle_position = newSimConnectData.spoilers_handle_position,
                gear_handle_position = newSimConnectData.gear_handle_position,
                ambient_wind_velocity = newSimConnectData.ambient_wind_velocity,
                ambient_wind_direction = newSimConnectData.ambient_wind_direction,
                ambient_temperature = newSimConnectData.ambient_temperature,
                stall_warning = newSimConnectData.stall_warning,
                overspeed_warning = newSimConnectData.overspeed_warning,
                is_gear_retractable = newSimConnectData.is_gear_retractable,
                spoiler_available = newSimConnectData.spoiler_available,
                gps_wp_prev_latitude = newSimConnectData.gps_wp_prev_latitude,
                gps_wp_prev_longitude = newSimConnectData.gps_wp_prev_longitude,
                gps_wp_prev_altitude = newSimConnectData.gps_wp_prev_altitude,
                gps_wp_prev_id = newSimConnectData.gps_wp_prev_id,
                gps_wp_next_latitude = newSimConnectData.gps_wp_next_latitude,
                gps_wp_next_longitude = newSimConnectData.gps_wp_next_longitude,
                gps_wp_next_altitude = newSimConnectData.gps_wp_next_altitude,
                gps_wp_next_id = newSimConnectData.gps_wp_next_id,
                gps_flight_plan_wp_index = newSimConnectData.gps_flight_plan_wp_index,
                gps_flight_plan_wp_count = newSimConnectData.gps_flight_plan_wp_count,
                sim_on_ground = newSimConnectData.sim_on_ground
            };
        }

        private IFlightDataConnector simConnectConnector;
        private IFlightDataConnector randomWalkConnector;
        private IFlightDataConnector replayConnector;
        private OperationalMode operationalMode;

    }

    public struct FlightDataStructure
    {
        public double latitude;
        public double longitude;
        public Int32 altitude;
        public Int32 altitude_above_ground;
        public Int32 engine1rpm;
        public Int32 engine2rpm;
        public Int32 engine3rpm;
        public Int32 engine4rpm;
        public Int32 lightsmask;
        public double ground_velocity;
        public double plane_pitch;
        public double plane_bank;
        public double plane_heading_true;
        public double plane_heading_magnetic;
        public double plane_airspeed_indicated;
        public double airspeed_true;
        public double vertical_speed;
        public double heading_indicator;
        public Int32 flaps_handle_position;
        public Int32 spoilers_handle_position;
        public Int32 gear_handle_position;
        public double ambient_wind_velocity;
        public double ambient_wind_direction;
        public double ambient_temperature;
        public Int32 stall_warning;
        public Int32 overspeed_warning;
        public Int32 is_gear_retractable;
        public Int32 spoiler_available;
        public double gps_wp_prev_latitude;
        public double gps_wp_prev_longitude;
        public Int32 gps_wp_prev_altitude;
        public string gps_wp_prev_id;
        public double gps_wp_next_latitude;
        public double gps_wp_next_longitude;
        public Int32 gps_wp_next_altitude;
        public string gps_wp_next_id;
        public Int32 gps_flight_plan_wp_index;
        public Int32 gps_flight_plan_wp_count;
        public Int32 sim_on_ground;
    }

    public struct EnvironmentDataStructure
    {
        public string title;
    }

}
