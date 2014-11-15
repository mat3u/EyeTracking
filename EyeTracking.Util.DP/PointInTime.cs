namespace EyeTracking.Util.DP
{
    using System;

    public struct PointInTime
    {
        public PointInTime(string line)
            : this()
        {
            var s = line.Split(new[] { ';' });

            this.T = new DateTime(1970, 1, 1).AddMilliseconds(double.Parse(s[0]));
            this.X = Double.Parse(s[1]);
            this.Y = Double.Parse(s[2]);
        }

        public PointInTime(double x, double y)
            : this()
        {
            this.X = x;
            this.Y = y;
        }

        public double X { get; set; }

        public double Y { get; set; }

        public DateTime T { get; set; }
    }
}
