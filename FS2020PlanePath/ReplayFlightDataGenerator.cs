using System;
using System.Linq;
using System.Collections.Generic;

namespace FS2020PlanePath
{

    public class ReplayFlightDataGenerator : AbstractFlightDataGenerator
    {

        public class Context
        {
            internal int flightNo;
            internal int segmentDurationSecs;
        }

        internal ReplayFlightDataGenerator(
            FS2020_SQLLiteDB dbAccessor,
            Func<Context> contextSupplier
        )
        {
            this.dbAccessor = dbAccessor;
            this.contextSupplier = contextSupplier;
        }

        public override string Name => "Replay";

        public override void Reset()
        {
            context = contextSupplier.Invoke();
            Console.WriteLine($"replaying flightNo({context.flightNo})");
            base.Reset();
            flightPath = dbAccessor.GetFlightPathSinceTimestamp(context.flightNo, 0);
        }

        internal override List<FlightPathData> GetFlightPathSince(long startingTimestamp)
        {
            if (flightPath.Count == 0)
            {
                return new List<FlightPathData>();
            }
            long initialTimestamp = Math.Max(nextStartingTimestamp, Math.Max(startingTimestamp, flightPath[0].timestamp));
            nextStartingTimestamp = initialTimestamp + context.segmentDurationSecs * TICKS_PER_SECOND;
            return flightPath.FindAll(fp => fp.timestamp >= initialTimestamp && fp.timestamp < nextStartingTimestamp).ToList();
        }

        private const int TICKS_PER_SECOND = 10000000;

        private FS2020_SQLLiteDB dbAccessor;
        private Func<Context> contextSupplier;
        private Context context;
        private List<FlightPathData> flightPath;
        private long nextStartingTimestamp;

    }

}
