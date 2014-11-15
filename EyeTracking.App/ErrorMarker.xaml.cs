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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EyeTracking.App
{
    /// <summary>
    /// Interaction logic for ErrorMarker.xaml
    /// </summary>
    public partial class ErrorMarker : UserControl
    {
        public double MaximumErrorSize
        {
            get { return (double)GetValue(MaximumErrorSizeProperty); }
            set { SetValue(MaximumErrorSizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaximumErrorSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaximumErrorSizeProperty =
            DependencyProperty.Register("MaximumErrorSize", typeof(double), typeof(ErrorMarker), new PropertyMetadata(50d));

        public double MeanErrorSize
        {
            get { return (double)GetValue(MeanErrorSizeProperty); }
            set { SetValue(MeanErrorSizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MeanErrorSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MeanErrorSizeProperty =
            DependencyProperty.Register("MeanErrorSize", typeof(double), typeof(ErrorMarker), new PropertyMetadata(20d));

        public ErrorMarker()
        {
            InitializeComponent();
        }
    }
}
