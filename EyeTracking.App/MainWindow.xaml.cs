namespace EyeTracking.App
{
    using Emgu.CV;
    using Emgu.CV.Structure;
    using EyeTracking.Tracker;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Timers;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media.Imaging;
    using WPFImage = System.Windows.Controls.Image;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Fields
        private Capture _capture;

        private IImage _main = new Image<Bgr, byte>(352, 288);
        private IImage _sub = new Image<Bgr, byte>(352, 288);

        private bool _online;
        private Stopwatch _watch;

        private PupilDetection _pupil;
        private MCvFont _font;

        private bool _recording;

        private Dictionary<DateTime, System.Drawing.PointF> _recordedPoints;

        private CalibrationWindow _calibrationWindow;
        private readonly ICalibration _calibration;

        private DateTime _session;

        private Timer _timer;

        #endregion

        #region Constructors
        public MainWindow()
        {
            InitializeComponent();

            this.Configuration = Configuration.GetConfig();

            this._timer = new Timer();
            this._watch = new Stopwatch();
            this._font = new MCvFont(Emgu.CV.CvEnum.FONT.CV_FONT_HERSHEY_SIMPLEX, 0.5, 0.5)
            {
                thickness = 1,
                color = new MCvScalar(255, 1, 1)
            };

            this._session = DateTime.Now;

            this._pupil = new PupilDetection(x => this.Main = x, x => this.Sub = x);
            this._calibration = this.GetCalibrationAlgorithm();

            this._recordedPoints = new Dictionary<DateTime, PointF>();

            this.output = File.CreateText("output.csv");

            InitializeCalibrationWindow();

            this.Online = true;
        }

        #endregion

        #region Properties
        public IImage Main
        {
            set
            {
                this.Dispatcher.Invoke(() =>
                {
                    this._main = value;
                    this.mainPreview.GetBindingExpression(WPFImage.SourceProperty).UpdateTarget();
                });
            }
        }

        public IImage Sub
        {
            set
            {
                this.Dispatcher.Invoke(() =>
                {
                    this._sub = value;
                    this.subPreview.GetBindingExpression(WPFImage.SourceProperty).UpdateTarget();
                });
            }
        }

        public BitmapSource MainPreview
        {
            get
            {
                return this._main.ToBitmapSource();
            }
        }

        public BitmapSource SubPreview
        {
            get
            {
                return this._sub.ToBitmapSource();
            }
        }

        public bool Online
        {
            get
            {
                return this._online;
            }
            set
            {
                if (this._online == value)
                {
                    return;
                }
                else if (value)
                {
                    this._capture = new Capture(1);

                    this._capture.FlipHorizontal = true;
                    this._capture.FlipVertical = false;

                    this._capture.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_WIDTH, this.Configuration.CameraW);
                    this._capture.SetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_HEIGHT, this.Configuration.CameraH);

                    this._timer.Interval = 1;
                    this._timer.Elapsed += OnImageGrabbed;
                    this._timer.Start();
                }
                else if (this._capture != null)
                {
                    this._timer.Stop();
                    this._capture.Dispose();

                    this._capture = null;
                }

                this._online = value;
            }
        }

        public double Threshold
        {
            get
            {
                return this.Dispatcher.Invoke<double>(() =>
                {
                    return this.threshold.Value;
                });
            }
        }

        public PointF Position
        {
            set
            {
                this.Dispatcher.Invoke(() =>
                {
                    this.PositionX = (int)value.X;
                    this.PositionY = (int)value.Y;

                    if (this._recording)
                    {
                        var p = value.Normalize(this.Configuration.CameraW, this.Configuration.CameraH);

                        this._recordedPoints.Add(DateTime.Now, p);
                    }
                });
            }
        }

        public Configuration Configuration { get; set; }
        #endregion

        #region Dependency Properties
        public int PositionX
        {
            get { return (int)GetValue(PositionXProperty); }
            set { SetValue(PositionXProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PositionX.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PositionXProperty =
            DependencyProperty.Register("PositionX", typeof(int), typeof(MainWindow), new PropertyMetadata(0));

        public int PositionY
        {
            get { return (int)GetValue(PositionYProperty); }
            set { SetValue(PositionYProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PositionY.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PositionYProperty =
            DependencyProperty.Register("PositionY", typeof(int), typeof(MainWindow), new PropertyMetadata(0));



        public long CalculationTime
        {
            get { return (long)GetValue(CalculationTimeProperty); }
            set
            {
                this.Dispatcher.Invoke(() => SetValue(CalculationTimeProperty, value));
            }
        }

        // Using a DependencyProperty as the backing store for CalculationTime.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CalculationTimeProperty =
            DependencyProperty.Register("CalculationTime", typeof(long), typeof(MainWindow), new PropertyMetadata(0L));



        public double CalibrationError
        {
            get { return (double)GetValue(CalibrationErrorProperty); }
            set { SetValue(CalibrationErrorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CalibrationError.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CalibrationErrorProperty =
            DependencyProperty.Register("CalibrationError", typeof(double), typeof(MainWindow), new PropertyMetadata(0d));
        private StreamWriter output;
        #endregion

        #region Methods
        private PointF CalculateGaze(PointF p)
        {
            return this._calibration.GetGaze(p);
        }

        private void Calibrate(IDictionary<DateTime, PointF> checkPoints, Dictionary<DateTime, PointF> recordedPoints)
        {
            var screen = checkPoints.ToList();
            var pupil = recordedPoints.ToList();

            var data = FilterAndGroupData(pupil, screen);

            this.Save(data, "Calib_Combined");

            // Calibration
            var error = this._calibration.Calibrate(data);

            // Error calculating
            this._calibrationWindow.RemoveErrorMarkers();
            this._calibrationWindow.RemovePoints();

            foreach (var d in data)
            {
                var distances = (from p in d.Value
                                 select this._calibration.GetGaze(p).Distance(d.Key)).ToList();

                var mean = distances.Average();
                var max = distances.Max();

                this._calibrationWindow.AddErrorMarker(d.Key, max, mean);

                foreach (var p in d.Value)
                {
                    this._calibrationWindow.AddPoints(this.CalculateGaze(p), d.Key);
                }
            }

            var unused = pupil.Select(x => x.Value).Except(data.SelectMany(x => x.Value)).Select(this.CalculateGaze);

            foreach (var u in unused)
            {
                this._calibrationWindow.AddPoints(u, null);
            }

            this.CalibrationError = error;
        }

        private Dictionary<PointF, IEnumerable<PointF>> FilterAndGroupData(List<KeyValuePair<DateTime, PointF>> pupil, IEnumerable<KeyValuePair<DateTime, PointF>> screen)
        {
            // Blink filtering
            var noblinks = from p in pupil
                           let b = from k in pupil where k.Value.IsEmpty select k.Key
                           where !(from bl in b where p.Key.InRange(bl.AddMilliseconds(-100), bl.AddMilliseconds(100)) select b).Any()
                           select p;

            // Prepare data
            var data = (from s in screen
                        let p =
                            noblinks.Where(
                                x =>
                                    x.Key.InRange(s.Key.AddMilliseconds(this.Configuration.SkipBefore),
                                        s.Key.AddMilliseconds(this.Configuration.SkipBefore + this.Configuration.WaitTime)))
                                .Select(x => x.Value)
                                .ToList() as IEnumerable<PointF>
                        select new { Key = s.Value, Value = p }).GroupBy(x => x.Key).ToDictionary(x => x.Key, x => x.SelectMany(y => y.Value));
            return data;
        }

        private ICalibration GetCalibrationAlgorithm()
        {
            switch (this.Configuration.CalibrationMethod)
            {
                case CalibrationMethod.Center:
                    return new NeuralNetworkCalibrationMassCenter();
                case CalibrationMethod.Proportional:
                    return new NeuralNetworkCalibrationProportional();
                case CalibrationMethod.Separated:
                    return new NeuralNetworkCalibrationSeparated();
                case CalibrationMethod.Naive:
                default:
                    return new NeuralNetworkCalibration();
            }
        }

        private void InitializeCalibrationWindow()
        {
            this._calibrationWindow = new CalibrationWindow();

            this._calibrationWindow.OnCalibrationStart += OnStart;
            this._calibrationWindow.OnValidationStart += OnStart;

            this._calibrationWindow.OnCalibrationDone += OnCalibrationDone;
            this._calibrationWindow.OnValidationDone += OnValidationDone;

            this._calibrationWindow.OnCalibrationAborted += OnAborted;
            this._calibrationWindow.OnValidationAborted += OnAborted;
        }

        #endregion

        #region Event Handlers
        private void OnImageGrabbed(object sender, EventArgs e)
        {
            this._timer.Stop();

            try
            {
                this._watch.Restart();

                // ------------------------------------------------

                using (var image = this._capture.QueryFrame())
                {
                    // Algorithm goes here

                    var p = this._pupil.Run(image, this.Threshold);

                    this.Position = p;

                    if (this._calibration.Ready && this._calibrationWindow.State == CalibrationWindowState.Default && p != default(PointF))
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            var np = this.CalculateGaze(p.Normalize(this.Configuration.CameraW, this.Configuration.CameraH));

                            if (this._calibrationWindow.Tracking)
                            {
                                this.output.WriteLine(string.Format("{0}; {1}", np.X, np.Y));
                            }

                            this._calibrationWindow.MoveMarker(np);
                        });
                    }

                    this._watch.Stop();

                    this.CalculationTime = this._watch.ElapsedMilliseconds;
                    // ------------------------------------------------
                }
            }
            finally
            {
                this._timer.Start();
            }
        }

        private void OnCalibrateClick(object sender, RoutedEventArgs e)
        {
            this._calibrationWindow.Show();
        }

        private void OnCalibrationDone(IDictionary<DateTime, PointF> checkPoints)
        {
            this._recording = false;

            this.Calibrate(checkPoints, this._recordedPoints);

            this.Save(this._recordedPoints, "Calib_Recorded");
            this.Save(checkPoints, "Calib_Check");
            this._calibration.Save(string.Format("{0}.ann", this._session.ToString("yyyyMMddHHmmss")));
        }

        private void Save(IDictionary<DateTime, PointF> points, string p)
        {
            using (var tx = File.CreateText(string.Format("{0}_{1}.dat", this._session.ToString("yyyyMMddHHmmss"), p)))
            {
                foreach (var point in points)
                {
                    tx.WriteLine("{0}\t{1}\t{2}", point.Key.TotalMilliseconds(), point.Value.X, point.Value.Y);
                }
            }
        }

        private void OnValidationDone(IDictionary<DateTime, PointF> checkPoints)
        {
            this._recording = false;

            var screen = checkPoints.ToList();
            var pupil = this._recordedPoints.ToList();

            this.Save(this._recordedPoints, "Valid_Recorded");
            this.Save(checkPoints, "Valid_Check");

            var data = FilterAndGroupData(pupil, screen);

            this.Save(data, "Valid_Combined");

            this.CalibrationError = data.Average(x => x.Value.Average(y => x.Key.Distance(y)));
        }

        private void Save(Dictionary<PointF, IEnumerable<PointF>> data, string p)
        {
            using (var tx = File.CreateText(string.Format("{0}_{1}.dat", this._session.ToString("yyyyMMddHHmmss"), p)))
            {
                foreach (var points in data)
                {
                    foreach (var point in points.Value)
                    {
                        tx.WriteLine("{0}\t{1}\t{2}\t{3}", points.Key.X, points.Key.Y, point.X, point.Y);
                    }
                }
            }
        }

        private void OnStart(object sender, EventArgs e)
        {
            this._recordedPoints = new Dictionary<DateTime, PointF>();
            this._recording = true;
        }

        private void OnAborted(object sender, EventArgs e)
        {
            this._recording = false;
            this._recordedPoints.Clear();
        }

        private void CloseBtnClick(object sender, RoutedEventArgs e)
        {
            this.Online = false;

            this.Close();
        }

        private void DragArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this._calibrationWindow.Close();

            this.output.Close();
            this.output.Dispose();

            this.Online = false;
        }
        #endregion
    }
}
