using System;
using System.Diagnostics;

namespace FS2020PlanePath
{

    public class FlightLoggingButton
    {
        public string name;
        public bool enabled;
        private Action pressAction;

        public FlightLoggingButton(string name, bool enabled, Action pressAction)
        {
            this.name = name;
            this.enabled = enabled;
            this.pressAction = pressAction;
        }

        public void press()
        {
            Debug.Assert(enabled, "press attempted on disabled button");
            pressAction();
        }

    }

    public class FlightLogger
    {

        public FlightLoggingButton startButton;
        public FlightLoggingButton stopButton;
        public FlightLoggingButton pauseButton;
        public FlightLoggingButton resumeButton;

        public Action initializeLogging;
        public Action enableLogging;
        public Action disableLogging;
        public Action flushLogging;

        private bool userStarted;

        public FlightLogger()
        {
            startButton = new FlightLoggingButton("start", true, () => startAction(true));
            stopButton = new FlightLoggingButton("stop", false, () => stopAction(true));
            pauseButton = new FlightLoggingButton("pause", false, () => pauseAction());
            resumeButton = new FlightLoggingButton("resume", false, () => resumeAction());
        }

        public void thresholdReached()
        {
            if (startButton.enabled)
            {
                startAction(false);
            }
        }

        public void thresholdMissed()
        {
            if (!userStarted && pauseButton.enabled && stopButton.enabled)
            {
                stopAction(false);
            }
        }

        void startAction(bool fromUi)
        {
            assertButtonEnabled(startButton);
            initializeLogging();
            enableLogging();
            stopButton.enabled = true;
            startButton.enabled = false;
            userStarted = fromUi;
            pauseButton.enabled = true;
            resumeButton.enabled = true;
        }

        void stopAction(bool isFromUser)
        {
            assertButtonEnabled(stopButton);
            disableLogging();
            flushLogging();
            startButton.enabled = true;
            stopButton.enabled = false;
            pauseButton.enabled = false;
            resumeButton.enabled = false;
        }

        void pauseAction()
        {
            assertButtonEnabled(pauseButton);
            disableLogging();
            resumeButton.enabled = true;
            pauseButton.enabled = false;
        }

        void resumeAction()
        {
            assertButtonEnabled(resumeButton);
            enableLogging();
            pauseButton.enabled = true;
            resumeButton.enabled = false;
        }

        private void assertButtonEnabled(FlightLoggingButton button)
        {
            Debug.Assert(button.enabled, $"button({button.name}) is disabled");
        }

    }

}
