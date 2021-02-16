using System;
using System.Diagnostics;

namespace FS2020PlanePath
{

    public interface FlightLoggingButtonModel
    {
        bool IsEnabled { get; set; }
    }

    public class FlightLogUiOrchestrator
    {

        public enum Source
        {
            StartStop,
            PauseResume
        }


        public bool IsAutomaticMode { get; set; }

        public FlightLoggingButtonModel StartButton { get; private set; }
        public FlightLoggingButtonModel StopButton { get; private set; }
        public FlightLoggingButtonModel PauseButton { get; private set; }
        public FlightLoggingButtonModel ResumeButton { get; private set; }
        public Action<Source> InitializeLoggingAction { get; private set; }
        public Action<Source> EnableLoggingAction { get; private set; }
        public Action<Source> DisableLoggingAction { get; private set; }
        public Action<Source> FlushLoggingAction { get; private set; }

        private bool userStarted;

        public FlightLogUiOrchestrator(
            FlightLoggingButtonModel startButton,
            FlightLoggingButtonModel stopButton,
            FlightLoggingButtonModel pauseButton,
            FlightLoggingButtonModel resumeButton,
            Action<Source> initializeLoggingAction,
            Action<Source> enableLoggingAction,
            Action<Source> disableLoggingAction,
            Action<Source> flushLoggingAction
        )
        {
            this.StartButton = startButton;
            this.StopButton = stopButton;
            this.PauseButton = pauseButton;
            this.ResumeButton = resumeButton;
            this.InitializeLoggingAction = initializeLoggingAction;
            this.EnableLoggingAction = enableLoggingAction;
            this.DisableLoggingAction = disableLoggingAction;
            this.FlushLoggingAction = flushLoggingAction;
        }

        public void ThresholdReached()
        {
            if (IsAutomaticMode && StartButton.IsEnabled)
            {
                StartAction(false);
            }
        }

        public void ThresholdMissed()
        {
            if (IsAutomaticMode && !userStarted && PauseButton.IsEnabled && StopButton.IsEnabled)
            {
                StopAction();
            }
        }

        public void Start()
        {
            StartAction(true);
        }

        public void Stop()
        {
            StopAction();
        }

        public void Pause()
        {
            PauseAction();
        }

        public void Resume()
        {
            ResumeAction();
        }

        private void StartAction(bool fromUi)
        {
            AssertButtonEnabled(StartButton);
            InitializeLoggingAction(Source.StartStop);
            EnableLoggingAction(Source.StartStop);
            StartButton.IsEnabled = false;
            ResumeButton.IsEnabled = false;
            StopButton.IsEnabled = true;
            PauseButton.IsEnabled = true;
            userStarted = fromUi;
        }

        private void StopAction()
        {
            AssertButtonEnabled(StopButton);
            DisableLoggingAction(Source.StartStop);
            FlushLoggingAction(Source.StartStop);
            StopButton.IsEnabled = false;
            PauseButton.IsEnabled = false;
            ResumeButton.IsEnabled = false;
            StartButton.IsEnabled = true;
        }

        private void PauseAction()
        {
            AssertButtonEnabled(StopButton);
            AssertButtonEnabled(PauseButton);
            DisableLoggingAction(Source.PauseResume);
            PauseButton.IsEnabled = false;
            ResumeButton.IsEnabled = true;
        }

        private void ResumeAction()
        {
            AssertButtonEnabled(StopButton);
            AssertButtonEnabled(ResumeButton);
            EnableLoggingAction(Source.PauseResume);
            ResumeButton.IsEnabled = false;
            PauseButton.IsEnabled = true;
        }

        private void AssertButtonEnabled(FlightLoggingButtonModel buttonModel)
        {
            Debug.Assert(buttonModel.IsEnabled, $"button({buttonModel}) is disabled");
        }

    }

}
