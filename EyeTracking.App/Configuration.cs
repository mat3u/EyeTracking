namespace EyeTracking.App
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Drawing;

    public class Configuration : ConfigurationSection
    {
        public static Configuration GetConfig()
        {
            return (Configuration)System.Configuration.ConfigurationManager.GetSection("Calibration") ?? new Configuration();
        }

        /// <summary>
        /// Time that marker waits in single point
        /// </summary>
        [ConfigurationProperty("waitTime", DefaultValue = 400, IsRequired = true)]
        [IntegerValidator(ExcludeRange = false, MinValue = 100, MaxValue = 10000)]
        public int WaitTime
        {
            get
            {
                return (int)this["waitTime"];
            }
            set
            {
                this["waitTime"] = value;
            }
        }

        /// <summary>
        /// Time that marker waits in single point
        /// </summary>
        [ConfigurationProperty("moveTime", DefaultValue = 700, IsRequired = true)]
        [IntegerValidator(ExcludeRange = false, MinValue = 1, MaxValue = 5000)]
        public int MoveTime
        {
            get
            {
                return (int)this["moveTime"];
            }
            set
            {
                this["moveTime"] = value;
            }
        }

        /// <summary>
        /// Time that marker waits in single point
        /// </summary>
        [ConfigurationProperty("skipBefore", DefaultValue = 100, IsRequired = true)]
        [IntegerValidator(ExcludeRange = false, MinValue = 10, MaxValue = 5000)]
        public int SkipBefore
        {
            get
            {
                return (int)this["skipBefore"];
            }
            set
            {
                this["skipBefore"] = value;
            }
        }

        /// <summary>
        /// Time that marker waits in single point
        /// </summary>
        [ConfigurationProperty("skipAfter", DefaultValue = 100, IsRequired = true)]
        [IntegerValidator(ExcludeRange = false, MinValue = 10, MaxValue = 5000)]
        public int SkipAfter
        {
            get
            {
                return (int)this["skipAfter"];
            }
            set
            {
                this["skipAfter"] = value;
            }
        }

        [ConfigurationProperty("calibrationMethod", DefaultValue = CalibrationMethod.Naive, IsRequired = true)]
        public CalibrationMethod CalibrationMethod
        {
            get
            {
                return (CalibrationMethod)this["calibrationMethod"];
            }
            set
            {
                this["calibrationMethod"] = value.ToString();
            }
        }

        [ConfigurationProperty("markerSize", DefaultValue = 48, IsRequired = false)]
        [IntegerValidator(ExcludeRange = false, MinValue = 16, MaxValue = 256)]
        public int MarkerSize
        {
            get
            {
                return (int)this["markerSize"];
            }
            set
            {
                this["markerSize"] = value;
            }
        }

        [ConfigurationProperty("cameraId", DefaultValue = 0, IsRequired = true)]
        [IntegerValidator(ExcludeRange = false, MinValue = 0)]
        public int CameraId
        {
            get
            {
                return (int)this["cameraId"];
            }
            set
            {
                this["cameraId"] = value;
            }
        }

        [ConfigurationProperty("cameraW", DefaultValue = 352, IsRequired = false)]
        [IntegerValidator(ExcludeRange = false, MinValue = 0)]
        public int CameraW
        {
            get
            {
                return (int)this["cameraW"];
            }
            set
            {
                this["cameraW"] = value;
            }
        }

        [ConfigurationProperty("cameraH", DefaultValue = 288, IsRequired = false)]
        [IntegerValidator(ExcludeRange = false, MinValue = 0)]
        public int CameraH
        {
            get
            {
                return (int)this["cameraH"];
            }
            set
            {
                this["cameraH"] = value;
            }
        }

        [ConfigurationProperty("randomOrder", DefaultValue = false, IsRequired = false)]
        public bool RandomOrder
        {
            get
            {
                return (bool)this["randomOrder"];
            }
            set
            {
                this["randomOrder"] = value;
            }
        }

        [ConfigurationProperty("saveResults", DefaultValue = false, IsRequired = false)]
        public bool SaveResults
        {
            get
            {
                return (bool)this["saveResults"];
            }
            set
            {
                this["saveResults"] = value;
            }
        }

        [ConfigurationProperty("previewMarkers", DefaultValue = true, IsRequired = false)]
        public bool PreviewMarkers
        {
            get
            {
                return (bool)this["previewMarkers"];
            }
            set
            {
                this["previewMarkers"] = value;
            }
        }

        [ConfigurationProperty("testPoints", IsRequired = true, IsDefaultCollection = true)]
        public TestPointsCollection TestPoints
        {
            get
            {
                return (TestPointsCollection)this["testPoints"];
            }
            set
            {
                this["testPoints"] = value;
            }
        }

        [ConfigurationProperty("validationPoints", IsRequired = true, IsDefaultCollection = true)]
        public TestPointsCollection ValidationPoints
        {
            get
            {
                return (TestPointsCollection)this["validationPoints"];
            }
            set
            {
                this["validationPoints"] = value;
            }
        }
    }


    public class TestPoint : ConfigurationElement
    {
        [ConfigurationProperty("id", IsKey = true, IsRequired = true)]
        public string Key
        {
            get
            {
                return (string)this["id"];
            }
        }

        [ConfigurationProperty("x", IsRequired = true)]
        public double X
        {
            get
            {
                return (double)this["x"];
            }
        }

        [ConfigurationProperty("y", IsRequired = true)]
        public double Y
        {
            get
            {
                return (double)this["y"];
            }
        }

        public static implicit operator PointF(TestPoint t)
        {
            return new PointF((float)t.X, (float)t.Y);
        }
    }

    [ConfigurationCollection(typeof(TestPoint), AddItemName = "point", CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class TestPointsCollection : ConfigurationElementCollection, IEnumerable<TestPoint>
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new TestPoint();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((TestPoint)element).Key;
        }



        public new IEnumerator<TestPoint> GetEnumerator()
        {
            int count = base.Count;
            for (int i = 0; i < count; i++)
            {
                yield return base.BaseGet(i) as TestPoint;
            }
        }
    }

    public enum CalibrationMethod
    {
        Naive = 0, 
        Center = 1,
        Proportional = 2,
        //Approx = 3,

        Separated = 4,
    }
}
