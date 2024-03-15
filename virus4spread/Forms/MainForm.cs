using System;
using System.Drawing;
using System.Threading;
using System.ComponentModel;
using VirusSpreadLibrary.AppProperties;
using VirusSpreadLibrary.AppProperties.PropertyGridExt;
using VirusSpreadLibrary.SpreadModel;
using virus4spread.Forms;
using System.Windows.Forms;


namespace virus4spread;

public partial class MainForm : Form
{
    private Simulation? modelSimulation;
    private GridForm? gridForm;
    public MainForm()
    {
        InitializeComponent();
        //  make property grid listen to collection properties changes
        //  using a custom editor extension in CollectionEditorExt.cs
        CollectionEditorExt.EditorFormClosed += new CollectionEditorExt.
        EditorFormClosedEventHandler(ConfigurationPropertyGrid_CollectionFormClosed);
    }
    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        AppSettings.Config.PersonMoveRate.PropertyChanged += SamplePropertyChangedHandler!;
        AppSettings.Config.VirusMoveRate.PropertyChanged += SamplePropertyChangedHandler!;
        // do somthing on change here ..
        ConfigurationPropertyGrid.SelectedObject = ConfigurationPropertyGrid.SelectedObject = AppSettings.Config;
    }

    private void SamplePropertyChangedHandler(object sender, PropertyChangedEventArgs e)
    {
        //eventsListBox.AddEvent(null!, nameof(DoubleSeriesClass.PropertyChanged), e);
    }
    private void MainForm_Load(object sender, EventArgs e)
    {
        AppSettings.Config.Setting.Load();
        PropertyGridSelectConfig();
        RestoreWindowPosition();
        ShowSimulationCheckBox.Checked = AppSettings.Config.ShowSimulationGridWindow;
    }
    private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        SaveWindowsPosition();
        AppSettings.Config.Setting.Save();
    }
    private void Timer_Tick(object sender, EventArgs e)
    {
        if (modelSimulation != null)
        {
            // run simulation from main form if GridForm not shown
            modelSimulation?.NextIteration();
        }
        this.Text = $"virus4spread Main Form Iteration: {modelSimulation?.Iteration}";
    }

    private void StartHoldSimulationButton_Click(object sender, EventArgs e)
    {
        if (AppSettings.Config.GridFormTimer < 1)
        {
            Timer.Interval = 1;
        }
        else
        {
            Timer.Interval = AppSettings.Config.GridFormTimer;
        }

        bool iterationRunnig = false;
        if (modelSimulation is not null)
        {
            iterationRunnig = modelSimulation.IterationRunning;
        }

        // stop-hold simulation
        if (Timer.Enabled || iterationRunnig) // <-hold
        {
            Timer.Enabled = false;
            if (iterationRunnig)
            {
                modelSimulation?.StopIteration();
            }
            StartHoldSimulationButton.BackColor = SystemColors.Control;
        }
        else // <- start
        {
            this.Text = $"virus4spread Main Form";
            modelSimulation ??= new();

            if (ShowSimulationCheckBox.Checked)
            {
                // run simulation iterations from GridForm, if GridForm is shown
                Timer.Enabled = false;
                Form? grdForm = Application.OpenForms["GridForm"];
                if (grdForm == null)
                {
                    grdForm?.Close();
                    try
                    {
                        gridForm = new(modelSimulation, AppSettings.Config.GridMaxX, AppSettings.Config.GridMaxY);
                        gridForm.Show();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
                modelSimulation.StartIteration();                
            }
            else
            {
                // run simulation iterations from main form, if GridForm not shown
                modelSimulation.StartIteration();
                Timer.Enabled = true;
            }
            StartHoldSimulationButton.BackColor = SystemColors.ControlLightLight;
        }
    }

    private void ResetSimulationButton_Click(object sender, EventArgs e)
    {
        Form? grdForm = Application.OpenForms["GridForm"];
        grdForm?.Close();
        Form? plotForm = Application.OpenForms["PlotForm"];
        plotForm?.Close();
        Form? phasChartForm = Application.OpenForms["PhaseChartForm"];
        phasChartForm?.Close();
        Timer.Enabled = false;
        StartHoldSimulationButton.BackColor = SystemColors.Control;
        modelSimulation = null;
    }
    private void ShowChart_button_Click(object sender, EventArgs e)
    {
        Form? pltForm = Application.OpenForms["PlotForm"];
        pltForm?.Close();
        if (modelSimulation is not null)
        {
            PlotForm plotForm = new(modelSimulation.PlotData);
            plotForm.Show();
        }
        this.Focus();
    }
    private void ShowPhaseChart_button_Click(object sender, EventArgs e)
    {
        Form? phaChartForm = Application.OpenForms["PhaseChartForm"];
        phaChartForm?.Close();
        if (modelSimulation is not null)
        {
            PhaseChartForm phaseChartForm = new(modelSimulation.PlotData);
            phaseChartForm.Show();
        }
        this.Focus();
    }
    private void PropertyGridSelectConfig()
    {
        ConfigurationPropertyGrid.SelectedObject = AppSettings.Config;
        //unhides properties with attribute [Browsable(false)]
        if (AppSettings.Config.ShowHelperSettings)
        {
            ConfigurationPropertyGrid.BrowsableAttributes = new AttributeCollection();
        }
        else
        {
            ConfigurationPropertyGrid.BrowsableAttributes = null;
        };
        ConfigurationPropertyGrid.Refresh();
    }
    private void ConfigurationPropertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
    {
        Form? grdForm = Application.OpenForms["GridForm"];
        if (grdForm != null) 
        {
            gridForm.NoTrackMovement = !AppSettings.Config.TrackMovment;
        }            
        AppSettings.Config.Setting.Save();
    }
    private void ConfigurationPropertyGrid_CollectionFormClosed(object s, FormClosedEventArgs e)
    {
        // code to run if extended custom collection editor form is closed
        // MessageBox.Show("was closed");
        ConfigurationPropertyGrid.SelectedObject = ConfigurationPropertyGrid.SelectedObject;
        AppSettings.Config.Setting.Save();
    }
    private void LoadConfig_button2_Click(object sender, EventArgs e)
    {
        AppSettings.Config.Setting.Load(true);
        PropertyGridSelectConfig();
    }
    private void SaveConfig_button3_Click(object sender, EventArgs e)
    {
        AppSettings.Config.Setting.Save(true);
        ConfigurationPropertyGrid.Refresh();
    }
    private static bool IsVisiblePosition(Point location, Size size)
    {
        Rectangle myArea = new(location, size);
        bool intersect = false;
        foreach (System.Windows.Forms.Screen screen in System.Windows.Forms.Screen.AllScreens)
        {
            intersect |= myArea.IntersectsWith(screen.WorkingArea);
        }
        return intersect;
    }
    private void RestoreWindowPosition()
    {
        // set window position
        if (IsVisiblePosition(AppSettings.Config.Form_Config_WindowLocation, AppSettings.Config.Form_Config_WindowSize))
        {
            this.StartPosition = FormStartPosition.Manual;
            this.Location = AppSettings.Config.Form_Config_WindowLocation;
            this.Size = AppSettings.Config.Form_Config_WindowSize;
            WindowState = FormWindowState.Normal;
        }
        else
        {
            this.WindowState = FormWindowState.Normal;
        }
    }
    private void SaveWindowsPosition()
    {
        // write window size to app config vars
        if (this.WindowState == FormWindowState.Normal)
        {
            AppSettings.Config.Form_Config_WindowSize = this.Size;
            AppSettings.Config.Form_Config_WindowLocation = this.Location;
        }
        else
        {
            AppSettings.Config.Form_Config_WindowSize = this.RestoreBounds.Size;
            AppSettings.Config.Form_Config_WindowLocation = this.RestoreBounds.Location;
        }
    }
    private void ShowSimulationCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        AppSettings.Config.ShowSimulationGridWindow = ShowSimulationCheckBox.Checked;
    }   

}
