namespace EyeTracking.SDK
{
    using System;
    using System.Windows;
    using System.Xml.Serialization;

    [Serializable]
    public struct TrackedPosition
    {
        [XmlIgnore]
        public Point Point { get; set; }

        [XmlIgnore]
        public DateTime Timestamp { get; private set; }

        public static TrackedPosition Create(Point p)
        {
            return new TrackedPosition()
            {
                Point = p,
                Timestamp = DateTime.Now
            };
        }

        #region Serialization Properties
        [XmlAttribute("x")]
        public double X
        {

            get
            {
                return this.Point.X;
            }
            set
            {
                this.Point = new Point(value, this.Point.Y);
            }
        }

        [XmlAttribute("y")]
        public double Y
        {
            get
            {
                return this.Point.Y;
            }
            set
            {
                this.Point = new Point(this.Point.X, value);
            }
        }
        #endregion

        public static TrackedPosition Create(double x, double y)
        {
            return TrackedPosition.Create(new Point(x, y));
        }
    }
}
