using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EyeTracking.App
{
    /// <summary>
    /// Interaction logic for CalibrationPoint.xaml
    /// </summary>
    public partial class CalibrationPoint : UserControl
    {
        public CalibrationPoint()
        {
            InitializeComponent();
        }



        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set
            {
                SetValue(TextProperty, value);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    this.point.Fill = Brushes.Blue;
                    this.point.Stroke = Brushes.Blue;
                }
            }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(CalibrationPoint), new PropertyMetadata(""));
    }
}
