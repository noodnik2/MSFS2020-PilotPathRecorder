using System;
using System.Diagnostics;

namespace FS2020PlanePath
{

    public class FlightLoggingOrchestrator
    {

        public enum Source
        {
            StartStop,
            PauseResume
        }

        public bool IsAutomatic { get; set; }

        public IButtonStateModel<ToggleState> EnableButton { get; private set; }
        public IButtonStateModel<ToggleState> PauseButton { get; private set; }
        public Action<Source> InitializeLoggingAction { get; private set; }
        public Action<Source> EnableLoggingAction { get; private set; }
        public Action<Source> DisableLoggingAction { get; private set; }
        public Action<Source> FlushLoggingAction { get; private set; }

        private bool userStarted;

        public FlightLoggingOrchestrator(
            IButtonStateModel<ToggleState> enableButton,
            IButtonStateModel<ToggleState> pauseButton,
            Action<Source> initializeLoggingAction,
            Action<Source> enableLoggingAction,
            Action<Source> disableLoggingAction,
            Action<Source> flushLoggingAction
        )
        {
            this.EnableButton = enableButton;
            this.PauseButton = pauseButton;
            this.InitializeLoggingAction = initializeLoggingAction;
            this.EnableLoggingAction = enableLoggingAction;
            this.DisableLoggingAction = disableLoggingAction;
            this.FlushLoggingAction = flushLoggingAction;
        }

        public void ThresholdReached()
        {
            if (IsAutomatic && EnableButton.State == ToggleState.Out)
            {
                StartAction(false);
            }
        }

        public void ThresholdMissed()
        {
            if (
                IsAutomatic 
             && !userStarted 
             && EnableButton.State == ToggleState.In
             && PauseButton.State == ToggleState.Out
            )
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
            Debug.Assert(EnableButton.State == ToggleState.Out);
            InitializeLoggingAction(Source.StartStop);
            EnableLoggingAction(Source.StartStop);
            EnableButton.State = ToggleState.In;
            PauseButton.State = ToggleState.Out;
            PauseButton.IsEnabled = true;
            userStarted = fromUi;
        }

        private void StopAction()
        {
            Debug.Assert(EnableButton.State == ToggleState.In);
            DisableLoggingAction(Source.StartStop);
            FlushLoggingAction(Source.StartStop);
            EnableButton.State = ToggleState.Out;
            PauseButton.State = ToggleState.Out;
            PauseButton.IsEnabled = false;
        }

        private void PauseAction()
        {
            Debug.Assert(EnableButton.State == ToggleState.In);
            Debug.Assert(PauseButton.State == ToggleState.Out);
            DisableLoggingAction(Source.PauseResume);
            PauseButton.State = ToggleState.In;
        }

        private void ResumeAction()
        {
            Debug.Assert(EnableButton.State == ToggleState.In);
            Debug.Assert(PauseButton.State == ToggleState.In);
            EnableLoggingAction(Source.PauseResume);
            PauseButton.State = ToggleState.Out;
        }

    }

}
