using System.Globalization;

namespace EyeTracking.SplitData
{
    using System;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using EyeTracking.Calibrator;

    class Program
    {
        private const string USAGE = "split [dataset] [pointspec] [pointset]";

        static void Main(string[] args)
        {
            if (args.Count() < 3)
            {
                Console.Write(USAGE);
            }

            var dataset = File.ReadAllLines(args[0]);
            var pointspec = File.ReadAllLines(args[1]);
            var pointset = File.ReadAllLines(args[2]);

            var allowedPoints = pointset.Where(x => !string.IsNullOrWhiteSpace(x)).Select(int.Parse).ToList();
            var points = pointspec.Select(x =>
            {
                var line = x.Split('\t');
                return new
                {
                    ID = int.Parse(line[0]), 
                    X = float.Parse(line[1], CultureInfo.InvariantCulture), 
                    Y = float.Parse(line[2], CultureInfo.InvariantCulture)
                };
            })
                .Where(x => allowedPoints.Contains(x.ID))
                .Select(x => new PointF(x.X, x.Y))
                .ToList();

            var use = false;
            var zero = new PointF(0, 0);

            foreach (var point in dataset.Select(TrackingPoint.FromLine))
            {
                if (point.Type == TrackingPoint.PointType.SS)
                {
                    use = points.Contains(point.Point);
                }

                if (use && point.Point != zero)
                {
                    Console.WriteLine(point.ToString());
                }
            }
        }
    }
}
