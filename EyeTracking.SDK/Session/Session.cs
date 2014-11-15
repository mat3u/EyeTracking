namespace EyeTracking.SDK.Session
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;

    [Serializable]
    public class Session
    {
        public Session()
        {
            Points = new List<TestPoint>();
        }

        [XmlAttribute("screenWidth")]
        public int ScreenWidth { get; set; }

        [XmlAttribute("screenHeight")]
        public int ScreenHeight { get; set; }

        [XmlElement("testPoint")]
        public List<TestPoint> Points { get; set; }

        [Serializable]
        public class TestPoint
        {
            [XmlAttribute("x")]
            public double X { get; set; }

            [XmlAttribute("y")]
            public double Y { get; set; }

            [XmlElement("tracked")]
            public List<TrackedPosition> Tracked { get; set; }
        }
    }
}
