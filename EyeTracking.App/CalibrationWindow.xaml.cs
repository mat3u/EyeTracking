namespace EyeTracking.App
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Shapes;
    using System.Xml.Serialization;

#pragma warning disable 4014

    public enum CalibrationWindowState
    {
        Default = 0,
        Calibration = 1,
        Validation = 2
    }

    public delegate void CalibrationDoneEventHandler(IDictionary<DateTime, PointF> checkPoints);

    /// <summary>
    /// Interaction logic for CalibrationWindow.xaml
    /// </summary>
    public partial class CalibrationWindow : Window
    {
        private CalibrationWindowState _state;

        #region Constructors
        public CalibrationWindow()
        {
            this.Configuration = Configuration.GetConfig();

            InitializeComponent();

            this.Message = Resource.WelcomeMessage;

            this.OnStateChanged += CalibrationWindow_OnStateChanged;
        }

        void CalibrationWindow_OnStateChanged(object sender, EventArgs e)
        {
            this.message.Visibility = Visibility.Hidden;
            this.marker.IsMarkerVisible = true;
        }
        #endregion

        #region Properties
        public Configuration Configuration { get; set; }

        public string Message
        {
            get
            {
                return this.message.Text;
            }
            set
            {
                this.message.Text = value;
            }
        }

        public CalibrationWindowState State
        {
            get { return _state; }
            private set
            {
                if (this._state != value)
                {
                    _state = value;
                    if (this.OnStateChanged != null)
                    {
                        this.OnStateChanged(this, null);
                    }
                }
            }
        }

        public IEnumerable<ErrorMarker> ErrorMarkers
        {
            get
            {
                return this.grid.Children.OfType<ErrorMarker>();
            }
        }

        public IEnumerable<CalibrationPoint> Points
        {
            get
            {
                return this.grid.Children.OfType<CalibrationPoint>();
            }
        }

        public bool Tracking { get; set; }
        #endregion

        #region Events
        public event CalibrationDoneEventHandler OnCalibrationDone;

        public event EventHandler OnCalibrationStart;

        public event EventHandler OnCalibrationAborted;

        public event CalibrationDoneEventHandler OnValidationDone;

        public event EventHandler OnValidationStart;

        public event EventHandler OnValidationAborted;

        public new event EventHandler OnStateChanged;
        #endregion

        #region Event Handlers
        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    switch(this.State)
                    {
                        case CalibrationWindowState.Calibration:
                            this.AbortCalibration();
                            break;
                        case CalibrationWindowState.Validation:
                            this.AbortValidation();
                            break;
                        case CalibrationWindowState.Default:
                            this.Hide();
                            break;
                    }
                    this.Hide();
                    break;
                case Key.Space:
                    if (this.State == CalibrationWindowState.Default)
                    {
                        this.StartCalibration();
                    }
                    break;
                case Key.V:
                    if (this.State == CalibrationWindowState.Default)
                    {
                        this.StartValidation();
                    }
                    break;
                case Key.D0:
                    this.marker.MoveMarker(new PointF(0, 0));
                    break;
                case Key.D9:
                    this.marker.MoveMarker(new PointF(1, 1));
                    break;
                case Key.D8:
                    var r = new Random();
                    this.marker.MoveMarker(new PointF((float)r.NextDouble(), (float)r.NextDouble()));
                    break;
                case Key.E:
                    this.ToggleVisibility(this.ErrorMarkers);
                    break;
                case Key.R:
                    this.ToggleVisibility(this.Points);
                    break;
                case Key.P:
                    this.ToggleVisibility(this.grid.Children.OfType<Marker>());
                    break;
                case Key.T:
                    this.grid.Background = this.grid.Background == System.Windows.Media.Brushes.Transparent
                        ? System.Windows.Media.Brushes.Gray : System.Windows.Media.Brushes.Transparent;
                    break;
                case Key.Z:
                    this.Tracking = !this.Tracking;
                    break;
                default:
                    // NOP
                    break;
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (this.Configuration.PreviewMarkers)
            {
                foreach (var point in this.Configuration.TestPoints)
                {
                    this.AddMarkerPreview(point);
                }
            }
        }
        #endregion

        #region Private Methods
        private void AbortCalibration()
        {
            if (this.State != CalibrationWindowState.Calibration)
            {
                return;
            }

            if (this.OnCalibrationAborted != null)
            {
                this.OnCalibrationAborted(this, null);
            }

            this.State = CalibrationWindowState.Default;
            this.ResetMarkerPosition();
        }

        private void AbortValidation()
        {
            if (this.State != CalibrationWindowState.Validation)
            {
                return;
            }

            if (this.OnValidationAborted != null)
            {
                this.OnValidationAborted(this, null);
            }

            this.State = CalibrationWindowState.Default;
            this.ResetMarkerPosition();
        }

        private async void StartCalibration()
        {
            if (this.State != CalibrationWindowState.Default) return;

            this.State = CalibrationWindowState.Calibration;

            if (this.OnCalibrationStart != null)
            {
                this.OnCalibrationStart(this, null);
            }

            var results = new Dictionary<DateTime, PointF>();
            var checkPoints = this.Configuration.TestPoints.ToList();

            IEnumerable<PointF> testPoints = null;

            if (this.Configuration.RandomOrder)
            {
                var random = new Random();

                testPoints = checkPoints.Select(x => new { P = (PointF)x, o = random.Next() })
                                   .OrderBy(x => x.o)
                                   .Select(x => x.P);
            }
            else
            {
                testPoints = checkPoints.Select(x => (PointF)x);
            }

            this.ResetMarkerPosition();

            // Wait before start
            await Task.Delay(this.Configuration.WaitTime);

            foreach (var p in testPoints)
            {
                if (this.State != CalibrationWindowState.Calibration) return;

                var timestamp = await NextPoint(p);

                results.Add(timestamp, p);
            }

            // Return to default state after calibration
            if (this.OnCalibrationDone != null)
            {
                this.OnCalibrationDone(results);
            }

            this.State = CalibrationWindowState.Default;
        }

        private async void StartValidation()
        {
            if (this.State != CalibrationWindowState.Default)
            {
                return;
            }

            this.State = CalibrationWindowState.Validation;

            if (this.OnValidationStart != null)
            {
                this.OnValidationStart(this, null);
            }

            var results = new Dictionary<DateTime, PointF>();
            var checkPoints = this.Configuration.ValidationPoints.ToList();

            IEnumerable<PointF> testPoints = null;

            if (this.Configuration.RandomOrder)
            {
                var random = new Random();

                testPoints = checkPoints.Select(x => new { P = (PointF)x, o = random.Next() })
                                   .OrderBy(x => x.o)
                                   .Select(x => x.P);
            }
            else
            {
                testPoints = checkPoints.Select(x => (PointF)x);
            }

            this.ResetMarkerPosition();

            // Wait before start
            await Task.Delay(this.Configuration.WaitTime);

            foreach (var p in testPoints)
            {
                if (this.State != CalibrationWindowState.Validation) return;

                var timestamp = await NextPoint(p);

                results.Add(timestamp, p);
            }

            // Return to default state after validation
            if (this.OnValidationDone != null)
            {
                this.OnValidationDone(results);
            }

            this.State = CalibrationWindowState.Default;
        }

        private async Task<DateTime> NextPoint(PointF p)
        {
            await this.marker.MoveMarker(p);

            var timestamp = DateTime.Now;

            await Task.Delay(this.Configuration.SkipBefore + this.Configuration.WaitTime + this.Configuration.SkipAfter);

            return timestamp;
        }

        private void AddMarkerPreview(PointF p)
        {
            var element = new Marker()
            {
                Margin = new Thickness(p.X * 2 * this.ActualWidth - this.ActualWidth, p.Y * 2 * this.ActualHeight - this.ActualHeight, 0, 0),
                Visibility = System.Windows.Visibility.Visible
            };

            this.grid.Children.Add(element);
        }

        private void ToggleVisibility(IEnumerable<Control> controls)
        {
            foreach (var p in controls)
            {
                p.Visibility = p.Visibility == System.Windows.Visibility.Visible ? System.Windows.Visibility.Hidden : System.Windows.Visibility.Visible;
            }
        }
        #endregion

        #region Methods
        public void ResetMarkerPosition()
        {
            this.MoveMarker(new PointF(0.5f, 0.5f));
        }

        public void MoveMarker(PointF p)
        {
            this.marker.MoveMarker(p, TimeSpan.FromMilliseconds(1));
        }

        public void AddErrorMarker(PointF p, double max, double mean)
        {
            var error = new ErrorMarker()
            {
                MaximumErrorSize = max * (this.Width + this.Height),
                MeanErrorSize = mean * (this.Width + this.Height),
                Margin = new Thickness(p.X * 2 * this.ActualWidth - this.ActualWidth, p.Y * 2 * this.ActualHeight - this.ActualHeight, 0, 0),
                Visibility = System.Windows.Visibility.Visible
            };

            this.grid.Children.Add(error);
        }

        public void RemoveErrorMarkers()
        {
            foreach (var e in this.ErrorMarkers.ToList())
            {
                this.grid.Children.Remove(e);
            }
        }

        public void AddPoints(PointF p, PointF? c)
        {
            var text = c.HasValue ? string.Format("({0}; {1})", c.Value.X, c.Value.Y) : string.Empty;

            var point = new CalibrationPoint()
            {
                Margin = new Thickness(p.X * 2 * this.ActualWidth - this.ActualWidth, p.Y * 2 * this.ActualHeight - this.ActualHeight, 0, 0),
                Visibility = System.Windows.Visibility.Hidden,
                Text = text
            };

            this.grid.Children.Add(point);
        }

        public void RemovePoints()
        {
            foreach (var p in this.Points.ToList())
            {
                this.grid.Children.Remove(p);
            }
        }

        #endregion
    }
}
