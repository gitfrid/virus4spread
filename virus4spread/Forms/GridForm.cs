using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Diagnostics;
using VirusSpreadLibrary.SpreadModel;
using VirusSpreadLibrary.AppProperties;
using System.Drawing.Drawing2D;

namespace virus4spread.Forms
{
    public partial class GridForm : Form
    {

        public readonly Simulation simulation;
        readonly Stopwatch watch = Stopwatch.StartNew();
        private readonly int maxX;
        private readonly int maxY;
        private bool noTrackMovement =!AppSettings.Config.TrackMovment;

        private int x, y;
        private float scale;


        public GridForm(Simulation ModelSimulation, int MaxX, int MaxY)
        {
            // fastbitmap settings
            //Application.Idle += (s, e) => this.OnIdle();

            InitializeComponent();

            simulation = ModelSimulation;
            maxX = MaxX;
            maxY = MaxY;
            this.Width = maxX;
            this.Height = maxY;

            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.DoubleBuffer, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.UserPaint, true);

            if (AppSettings.Config.GridFormTimer < 1)
            {
                Timer1.Interval = 1;
            }
            else
            {
                Timer1.Interval = AppSettings.Config.GridFormTimer;
            }

            Timer1.Enabled = true;
        }

        public bool NoTrackMovement 
        { 
            set => noTrackMovement = value; 
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            if (noTrackMovement) 
            {
                simulation.CreateFastBitmap(maxX, maxY);
            }            
            simulation.NextIteration();
            OnIdle();
        }        
        private void UpdateBenchmarkMessage()
        {
            Text = $"virus4spread Iteration: {simulation.Iteration} [{Width}x{Height}] " +
                $"in {Math.Truncate(watch.Elapsed.TotalMilliseconds)} ms " +
                $"({1 / watch.Elapsed.TotalSeconds:N1} Hz)";
            watch.Restart();
        }
        public void OnIdle()
        {
            Invalidate();
        }
        protected override void OnPaint(PaintEventArgs e)
        {

            // optional - set interpolation mode.
            e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            if (simulation.FastBitmap != null)
            {
                //simulation.FastBitmap.Draw(e.Graphics);                
                e.Graphics.DrawImage(simulation.FastBitmap.Image, new Rectangle(0, 0, x, y));
                BackgroundImage = simulation.FastBitmap.Image;
                //e.Graphics.DrawImage(BackgroundImage, new Rectangle(0, 0, x, y));
                //this.InvokePaintBackground(this, new PaintEventArgs(this.CreateGraphics(), this.ClientRectangle));
            }
            UpdateBenchmarkMessage();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // calling base will cause flickering
            // base.OnPaintBackground(e);
        }

        protected override void OnBackgroundImageChanged(EventArgs e)
        {
            if (BackgroundImage != null) scale = (float)BackgroundImage.Width / BackgroundImage.Height;
            base.OnBackgroundImageChanged(e);
        }


        private void GridForm_SizeChanged(object sender, EventArgs e)
        {
            if (scale > (float)Width / Height)
            {
                x = Width;
                y = (int)(Width / scale);
            }
            else
            {
                y = Height;
                x = (int)(Height * scale);
            }
            // set state to check in Main form
            simulation.IsMinimizedGridForm = (WindowState == FormWindowState.Minimized);
        }


        protected override void OnClosed(EventArgs e)
        {
            Timer1.Stop();
            // disposes fastbitmap - workaround needed because of remaining artefacts when scaling points
            // slows down drawing because before each frame the bittmap is disposed
            simulation.Dispose();
        }

        private void GridForm_Load(object sender, EventArgs e)
        {
            RestoreWindowPosition();
            this.BackgroundImageLayout = ImageLayout.Stretch;
        }
        private static bool IsVisiblePosition(Point location, Size size)
        {
            Rectangle myArea = new(location, size);
            bool intersect = false;
            foreach (Screen screen in Screen.AllScreens)
            {
                intersect |= myArea.IntersectsWith(screen.WorkingArea);
            }
            return intersect;
        }
        private void RestoreWindowPosition()
        {
            // set window position
            if (IsVisiblePosition(AppSettings.Config.GridForm_WindowLocation, AppSettings.Config.GridForm_WindowSize))
            {
                this.StartPosition = FormStartPosition.Manual;
                this.Location = AppSettings.Config.GridForm_WindowLocation;
                this.Size = AppSettings.Config.GridForm_WindowSize;
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
                AppSettings.Config.GridForm_WindowSize = this.Size;
                AppSettings.Config.GridForm_WindowLocation = this.Location;
            }
            else
            {
                AppSettings.Config.GridForm_WindowSize = this.RestoreBounds.Size;
                AppSettings.Config.GridForm_WindowLocation = this.RestoreBounds.Location;
            }
        }

        private void GridForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveWindowsPosition();
            AppSettings.Config.Setting.Save();
        }

        private async void GridForm_Activated(object sender, EventArgs e)
        {
            // bugfix to fire OnPaint event after form is shown
            await Task.Delay(500);
            this.Width = Width + 1;
            this.Width = Width - 1;
        }
    
    }
}
