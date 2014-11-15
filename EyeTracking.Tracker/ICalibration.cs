namespace EyeTracking.Tracker
{
    using System.Collections.Generic;
    using System.Drawing;

    public interface ICalibration
    {
        double Calibrate(IDictionary<PointF, IEnumerable<PointF>> data);
        PointF GetGaze(PointF point);
        void Save(string path);
        bool Ready { get; }
    }
}