using System;
using System.IO;
using System.ComponentModel;
using System.Windows.Forms;

namespace FS2020PlanePath
{
    public partial class KmlLiveCamEditorForm : Form
    {
        private const string SavedFileFilter = (
            "Keyhole Markup Language files (*.kml)|*.kml" +
            "|C# Script files (*.csx)|*.csx" +
            "|All files (*.*)|*.*"
        );

        private KmlLiveCam _kmlLiveCam;

        public KmlLiveCam KmlLiveCam { get { return _kmlLiveCam; } }

        public KmlLiveCamEditorForm(string alias, KmlLiveCam kmlLiveCam) {
            InitializeComponent();
            _kmlLiveCam = kmlLiveCam;
            Text = $"Live Camera KML Editor: {alias}";
            cameraEditorTB.Text = kmlLiveCam.Camera.Template;
            linkEditorTB.Text = kmlLiveCam.Link.Template;
        }

        private void Handle_KmlTextValidation_Event(object sender, CancelEventArgs e)
        {
            KmlLiveCam updatedKmlLiveCam = new KmlLiveCam(cameraEditorTB.Text, linkEditorTB.Text);

            string[] diagnostics = updatedKmlLiveCam.Diagnostics;
            if (diagnostics.Length > 0)
            {
                displayError("Validation Error(s)", string.Join("\n", diagnostics));
                e.Cancel = true;
                return;
            }

            _kmlLiveCam = updatedKmlLiveCam;
        }

        private void Handle_FileOpenMenuItemSelected_Event(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = SavedFileFilter;
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                string textFromFile;
                try
                {
                    textFromFile = File.ReadAllText(openFileDialog.FileName);
                }
                catch (Exception ex)
                {
                    displayError("Exception Encountered", ex.Message);
                    return;
                }

                switch (editorPaneTC.SelectedTab.Name)
                {
                    case "cameraEditorTP":
                        cameraEditorTB.Text = textFromFile;
                        break;
                    case "linkEditorTP":
                        linkEditorTB.Text = textFromFile;
                        break;
                    default:
                        displayUnknownTabError(editorPaneTC.SelectedTab.Name);
                        return;
                }

            }

        }

        private void Handle_FileSaveMenuItemSelected_Event(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = SavedFileFilter;
                saveFileDialog.FilterIndex = 1;
                saveFileDialog.RestoreDirectory = true;

                if (saveFileDialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                string textToWrite;
                switch (editorPaneTC.SelectedTab.Name)
                {
                    case "cameraEditorTP":
                        textToWrite = cameraEditorTB.Text;
                        break;
                    case "linkEditorTP":
                        textToWrite = linkEditorTB.Text;
                        break;
                    default:
                        displayUnknownTabError(editorPaneTC.SelectedTab.Name);
                        return;
                }

                try
                {
                    File.WriteAllText(saveFileDialog.FileName, textToWrite);
                } catch(Exception ex)
                {
                    displayError("Exception Encountered", ex.Message);
                }
            }

        }

        private void Handle_HelpRequested_Event(object sender, HelpEventArgs hlpevent)
        {
            showHelp();
        }

        private void Handle_HelpMenuItemSelected_Event(object sender, EventArgs e)
        {
            showHelp();
        }

        private void displayError(string caption, string details)
        {
            MessageBox.Show($"Details:\n{details}", caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void displayUnknownTabError(string tabName)
        {
            displayError("Unknown Tab", $"Tab Name: {tabName}");
        }

        private void showHelp() {
            Help.ShowPopup(this,
                $@"Permitted substitution values:

Camera Template:

{"\t" + string.Join("\n\t", TextRenderer.Placeholders(typeof(KmlCameraParameterValues)))}

Link Template:

{"\t" + string.Join("\n\t", TextRenderer.Placeholders(typeof(KmlNetworkLinkValues)))}

See: https://developers.google.com/kml/documentation/kmlreference",
                this.Location
            );
        }
    }

}
