using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EyeTracking.App
{
    /// <summary>
    /// Interaction logic for MarkerBoard.xaml
    /// </summary>
    public partial class MarkerBoard : UserControl
    {
        public PointF MarkerPosition { get; protected set; }

        public bool IsMarkerVisible
        {
            get
            {
                return this.marker.Visibility == System.Windows.Visibility.Visible;
            }
            set
            {
                if (value)
                    this.marker.Visibility = System.Windows.Visibility.Visible;
                else
                    this.marker.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        public double MarkerSize
        {
            get { return (double)GetValue(MarkerSizeProperty); }
            set { SetValue(MarkerSizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MarkerSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MarkerSizeProperty =
            DependencyProperty.Register("MarkerSize", typeof(double), typeof(MarkerBoard), new PropertyMetadata((double)48));

        

        public double MarkerMoveTime
        {
            get { return (double)GetValue(MarkerMoveTimeProperty); }
            set { SetValue(MarkerMoveTimeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MarkerMoveTime.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MarkerMoveTimeProperty =
            DependencyProperty.Register("MarkerMoveTime", typeof(double), typeof(MarkerBoard), new PropertyMetadata((double)700));

        public MarkerBoard()
        {
            InitializeComponent();

            this.MarkerPosition = new PointF(0.5f, 0.5f);

            var sb = (Storyboard)this.FindResource("PulseStoryboard");

            sb.Begin();
        }

        public async Task MoveMarker(PointF p)
        {
            TranslateTransform transform = new TranslateTransform();
            this.marker.RenderTransform = transform;

            var moveTime = this.MarkerMoveTime;

            await this.MoveMarker(p, TimeSpan.FromMilliseconds(moveTime));
        }

        public async Task MoveMarker(PointF p, TimeSpan t)
        {
            TranslateTransform transform = new TranslateTransform();
            this.marker.RenderTransform = transform;

            double dx, dy, sx, sy;

            PointToScreen(p, out dx, out dy);
            PointToScreen(this.MarkerPosition, out sx, out sy);

            var marginAnimationX = new DoubleAnimation(sx, dx, t);
            var marginAnimationY = new DoubleAnimation(sy, dy, t);

            transform.BeginAnimation(TranslateTransform.XProperty, marginAnimationX);
            transform.BeginAnimation(TranslateTransform.YProperty, marginAnimationY);

            this.MarkerPosition = p;

            await Task.Delay((int)t.TotalMilliseconds);
        }

        private void PointToScreen(PointF p, out double x, out double y)
        {
            x = p.X * this.Width - this.MarkerSize / 2;
            y = p.Y * this.Height - this.MarkerSize / 2;
        }
    }
}
