namespace EyeTracking.Tracker
{
    using Emgu.CV;
    using Emgu.CV.Structure;
    using System;
    using System.Drawing;
    using System.Linq;

    public class PupilDetection
    {
        private MCvFont _font;
        private Action<IImage> _setMain;
        private Action<IImage> _setSub;

        public PupilDetection(Action<IImage> setMainPreview = null, Action<IImage> setSubPreview = null)
        {
            this._font = new MCvFont(Emgu.CV.CvEnum.FONT.CV_FONT_HERSHEY_SIMPLEX, 0.5, 0.5);
            this._font.thickness = 1;
            this._font.color = new MCvScalar(255, 1, 1);

            this._setMain = setMainPreview ?? (x => { });
            this._setSub = setSubPreview ?? (x => { });
        }

        public PointF Run(Image<Bgr, byte> image, double threshold)
        {
            PointF center = default(PointF);

            var gray = image.Convert<Gray, byte>();

            var bw = gray.ThresholdBinaryInv(new Gray(threshold), new Gray(255));

            this._setSub(bw.Clone());

            bw = bw.Dilate(2)
                   .Erode(5)
                   .Dilate(7)
                   .Erode(4)
                   .SmoothGaussian(3);

            var contour = bw.FindContours();

            if (contour != null && contour.Any())
            {
                var hull = contour.GetConvexHull(Emgu.CV.CvEnum.ORIENTATION.CV_COUNTER_CLOCKWISE);
                var gcenter = hull.GetMoments().GravityCenter;

                center = new PointF((float)gcenter.x, (float)gcenter.y);

                // Pupil convex hull
                image.Draw(hull, new Bgr(255, 0, 0), 1);

                // Pupil center
                image.Draw(new CircleF(center, 1), new Bgr(0, 255, 0), 2);

            }
            else
            {
                image.Draw("BLINK", ref this._font, new Point(10, 20), new Bgr(0, 255, 0));
            }

            this._setMain(image);

            return center;
        }
    }
}
