using System;
using System.Collections.Generic;
using Xunit;

namespace FS2020PlanePath.XUnitTests
{
    public class FlightLoggingLogicTests
    {

        public FlightLoggingLogicTests()
        {
            actionsList = new List<string>();
            logger = new FlightLogger()
            {
                initializeLogging = () => actionsList.Add("i"),
                enableLogging = () => actionsList.Add("e"),
                disableLogging = () => actionsList.Add("d"),
                flushLogging = () => actionsList.Add("f")
            };
        }

        private List<string> actionsList;
        private FlightLogger logger;

        /// <summary>
        /// If you manually start logging then you must manually stop it. 
        /// </summary>
        [Fact]
        public void UserStartedThresholdReachedThenMissedThenUserStopped()
        {
            logger.startButton.press();
            AssertActions("i,e");
            logger.thresholdReached();
            AssertActions("i,e");
            logger.thresholdMissed();
            AssertActions("i,e");
            logger.stopButton.press();
            AssertActions("i,e,d,f");
        }

        /// <summary>
        /// If you manually stop an automatically started log then once you go below the threshold speed
        /// and then above that speed again it will start an automatic log as a new flight.
        /// </summary>
        [Fact]
        public void ThresholdReachedUserStoppedThresholdMissedThenReached()
        {
            logger.thresholdReached();
            AssertActions("i,e");
            logger.stopButton.press();
            AssertActions("i,e,d,f");
            logger.thresholdMissed();
            AssertActions("i,e,d,f");
            logger.thresholdReached();
            AssertActions("i,e,d,f,i,e");
        }

        [Fact]
        public void ThresholdReachedThenMissed()
        {
            logger.thresholdReached();
            AssertActions("i,e");
            logger.thresholdMissed();
            AssertActions("i,e,d,f");
        }

        /// <summary>
        /// If you pause a log and then go below the threshold speed you must manually stop the log.
        /// </summary>
        [Fact]
        public void ThresholdReachedUserPausedThresholdMissed()
        {
            logger.thresholdReached();
            AssertActions("i,e");
            logger.pauseButton.press();
            AssertActions("i,e,d");
            logger.thresholdMissed();
            AssertActions("i,e,d");
            logger.stopButton.press();
            AssertActions("i,e,d,d,f");
        }

        private void AssertActions(string actions)
        {
            Assert.Equal(actions, string.Join(",", actionsList));
        }

    }

}
