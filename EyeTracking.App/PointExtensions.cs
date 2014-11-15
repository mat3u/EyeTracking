namespace EyeTracking.App
{
    using System;
    using System.Drawing;

    public static class PointExtensions
    {
        public static PointF Normalize(this PointF @this, int width, int height)
        {
            return new PointF(@this.X / width, @this.Y / height);
        }

        public static PointF Denormalize(this PointF @this, int width, int height)
        {
            return new PointF(@this.X * width, @this.Y * height);
        }

        public static double DistanceFromZero(this PointF @this)
        {
            return Math.Sqrt(@this.X * @this.X + @this.Y * @this.Y);
        }

        public static PointF AbsSub(this PointF @this, PointF t)
        {
            return new PointF(Math.Abs(@this.X - t.X), Math.Abs(@this.Y - t.Y));
        }

        public static double Distance(this PointF @this, PointF t)
        {
            return @this.AbsSub(t).DistanceFromZero();
        }
    }
}
