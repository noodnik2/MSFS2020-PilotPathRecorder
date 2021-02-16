using System.Collections.Generic;
using Xunit;

namespace FS2020PlanePath.XUnitTests
{

    public class TestFlightLoggingButtonModel : FlightLoggingButtonModel
    {

        public TestFlightLoggingButtonModel(bool initialEnabledState)
        {
            IsEnabled = initialEnabledState;
        }

        public bool IsEnabled { get; set; }

    }

    public class FlightLoggingUiOrchestrationTests
    {

        public FlightLoggingUiOrchestrationTests()
        {
            actionNames = new List<string>();
            actionSources = new List<int>();
            orchestrator = new FlightLogUiOrchestrator(
                new TestFlightLoggingButtonModel(true),
                new TestFlightLoggingButtonModel(false),
                new TestFlightLoggingButtonModel(false),
                new TestFlightLoggingButtonModel(false),
                s => registerAction(s, "i"),
                s => registerAction(s, "e"),
                s => registerAction(s, "d"),
                s => registerAction(s, "f")
            );
        }

        private void registerAction(FlightLogUiOrchestrator.Source actionSource, string actionName)
        {
            actionNames.Add(actionName);
            actionSources.Add((int) actionSource);
        }

        private List<string> actionNames;
        private List<int> actionSources;
        private FlightLogUiOrchestrator orchestrator;

        /// <summary>
        /// Tests expected initial configuration.
        /// </summary>
        [Fact]
        public void StartupState()
        {
            Assert.False(orchestrator.IsAutomaticMode);
            AssertInitialButtonState();
            AssertActions("");
        }

        /// <summary>
        /// Arrival at either threshold ignored while not in automatic mode.
        /// Also tests start, stop, pause & resume actions.
        /// </summary>
        [Fact]
        public void NonAutomaticFullNormalCase()
        {
            orchestrator.IsAutomaticMode = false;
            AssertActions("");
            orchestrator.ThresholdReached();
            orchestrator.ThresholdMissed();
            AssertActions("");
            Assert.False(orchestrator.IsAutomaticMode);
            AssertInitialButtonState();
            orchestrator.Start();
            AssertActions("i,e");
            orchestrator.Pause();
            AssertButtonState(false, true, false, true);
            AssertActions("i,e,d");
            orchestrator.Resume();
            AssertButtonState(false, true, true, false);
            AssertActions("i,e,d,e");
            orchestrator.Stop();
            AssertActions("i,e,d,e,d,f");
            AssertSources("0,0,1,1,0,0");
            AssertInitialButtonState();
        }

        /// <summary>
        /// Switching off automatic mode while in flight disables close & flush of log on threshold missed.
        /// </summary>
        [Fact]
        public void AutomaticToNonAutomaticSimpleCase()
        {
            orchestrator.IsAutomaticMode = true;
            AssertInitialButtonState();
            orchestrator.ThresholdReached();
            AssertActions("i,e");
            orchestrator.IsAutomaticMode = false;
            orchestrator.ThresholdMissed();
            AssertActions("i,e");
            AssertSources("0,0");
            AssertButtonState(false, true, true, false);
        }

        /// <summary>
        /// "If you manually start logging then you must manually stop it."
        /// </summary>
        [Fact]
        public void AutomaticUserStartedThresholdReachedThenMissedThenUserStopped()
        {
            orchestrator.IsAutomaticMode = true;
            Assert.True(orchestrator.IsAutomaticMode);
            orchestrator.Start();
            AssertActions("i,e");
            orchestrator.ThresholdReached();
            AssertActions("i,e");
            orchestrator.ThresholdMissed();
            AssertActions("i,e");
            AssertButtonState(false, true, true, false);
            orchestrator.Stop();
            AssertInitialButtonState();
            AssertSources("0,0,0,0");
            AssertActions("i,e,d,f");
        }

        /// <summary>
        /// "If you manually start logging then you must manually stop it."
        /// </summary>
        [Fact]
        public void NonAutomaticUserStartedToAutomaticThresholdMissed()
        {
            orchestrator.IsAutomaticMode = false;
            Assert.False(orchestrator.IsAutomaticMode);
            AssertInitialButtonState();
            orchestrator.Start();
            AssertActions("i,e");
            orchestrator.ThresholdReached();
            AssertActions("i,e");
            AssertButtonState(false, true, true, false);
            orchestrator.IsAutomaticMode = true;
            orchestrator.ThresholdMissed();
            AssertActions("i,e");
            AssertSources("0,0");
            AssertButtonState(false, true, true, false);
        }

        /// <summary>
        /// "If you manually stop an automatically started log then once you go below the threshold speed
        /// and then above that speed again it will start an automatic log as a new flight."
        /// </summary>
        [Fact]
        public void AutomaticThresholdReachedUserStoppedThresholdMissedThenReached()
        {
            orchestrator.IsAutomaticMode = true;
            Assert.True(orchestrator.IsAutomaticMode);
            orchestrator.ThresholdReached();
            AssertActions("i,e");
            orchestrator.Stop();
            AssertActions("i,e,d,f");
            orchestrator.ThresholdMissed();
            AssertActions("i,e,d,f");
            orchestrator.ThresholdReached();
            AssertSources("0,0,0,0,0,0");
            AssertActions("i,e,d,f,i,e");
        }

        /// <summary>
        /// "Above that value, logging starts and below that value, it stops and the flight log is saved."
        /// </summary>
        [Fact]
        public void AutomaticThresholdReachedThenMissed()
        {
            orchestrator.IsAutomaticMode = true;
            Assert.True(orchestrator.IsAutomaticMode);
            AssertActions("");
            orchestrator.ThresholdReached();
            AssertActions("i,e");
            orchestrator.ThresholdMissed();
            AssertSources("0,0,0,0");
            AssertActions("i,e,d,f");
        }

        /// <summary>
        /// "If you pause a log and then go below the threshold speed you must manually stop the log."
        /// </summary>
        [Fact]
        public void AutomaticThresholdReachedUserPausedThresholdMissed()
        {
            orchestrator.IsAutomaticMode = true;
            Assert.True(orchestrator.IsAutomaticMode);
            orchestrator.ThresholdReached();
            AssertActions("i,e");
            AssertButtonState(false, true, true, false);
            orchestrator.Pause();
            AssertButtonState(false, true, false, true);
            AssertActions("i,e,d");
            orchestrator.ThresholdMissed();
            AssertActions("i,e,d");
            AssertButtonState(false, true, false, true);
            orchestrator.Stop();
            AssertInitialButtonState();
            AssertActions("i,e,d,d,f");
            AssertSources("0,0,1,0,0");
        }

        /// <summary>
        /// "If you pause a log and then go below the threshold speed you must manually stop the log."
        /// (... but, after you resume then go below threshold again, it will close & flush...)
        /// </summary>
        [Fact]
        public void AutomaticThresholdReachedUserPausedThresholdMissedUserResumed()
        {
            orchestrator.IsAutomaticMode = true;
            orchestrator.ThresholdReached();
            orchestrator.Pause();
            orchestrator.ThresholdMissed();
            orchestrator.Resume();
            AssertActions("i,e,d,e");
            orchestrator.ThresholdMissed();
            AssertActions("i,e,d,e,d,f");
            AssertInitialButtonState();
            AssertSources("0,0,1,1,0,0");
        }

        /// <summary>
        /// After user starts while in automatic mode, automatic operation is restored in the next cycle.
        /// </summary>
        [Fact]
        public void AutomaticOperationRestoredAfterUserStop()
        {
            orchestrator.IsAutomaticMode = true;
            orchestrator.Start();
            AssertActions("i,e");
            orchestrator.ThresholdReached();
            AssertActions("i,e");
            orchestrator.ThresholdMissed();
            AssertActions("i,e");
            orchestrator.ThresholdReached();
            AssertActions("i,e");
            orchestrator.Stop();
            AssertActions("i,e,d,f");
            AssertInitialButtonState();
            orchestrator.ThresholdMissed();
            AssertActions("i,e,d,f");
            AssertInitialButtonState();
            orchestrator.ThresholdReached();
            AssertButtonState(false, true, true, false);
            AssertActions("i,e,d,f,i,e");
            orchestrator.ThresholdMissed();
            AssertActions("i,e,d,f,i,e,d,f");
            AssertInitialButtonState();
            AssertSources("0,0,0,0,0,0,0,0");
        }

        private void AssertActions(string actions)
        {
            Assert.Equal(actions, string.Join(",", actionNames));
        }

        private void AssertSources(string sources)
        {
            Assert.Equal(sources, string.Join(",", actionSources));
        }

        private void AssertInitialButtonState()
        {
            AssertButtonState(true, false, false, false);
        }

        private void AssertButtonState(bool start, bool stop, bool pause, bool resume)
        {
            Assert.Equal(start, orchestrator.StartButton.IsEnabled);
            Assert.Equal(stop, orchestrator.StopButton.IsEnabled);
            Assert.Equal(pause, orchestrator.PauseButton.IsEnabled);
            Assert.Equal(resume, orchestrator.ResumeButton.IsEnabled);
        }

    }

}
