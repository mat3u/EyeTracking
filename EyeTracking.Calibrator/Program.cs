namespace EyeTracking.Calibrator
{
    using EyeTracking.Tracker;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Linq;

    class Program
    {
        private const string USAGE = "cal [testSet] [validationSet] [startDelay] [endDelay] [output]";

        const long recordTime = 3600;

        static void Main(string[] args)
        {
            if (args.Count() < 5)
            {
                Console.Error.WriteLine(USAGE);
            }

            var testSetName = args[0];
            var validSetName = args[1];
            var delayStart = long.Parse(args[2]);
            var delayEnd = long.Parse(args[3]);
            var name = args[4];

            TextWriter output = null;

            if (args.Count() == 6)
            {
                output = new StreamWriter(args[5], true);
            }

            try
            {
                var testPoints = ReadData(testSetName).ToList();
                var valPoints = ReadData(validSetName).ToList();

                var calibrationSet = GroupData(testPoints, delayStart, delayEnd);
                var validationSet = GroupData(valPoints, delayStart, delayEnd);

                var ann = Calibrate(calibrationSet);
                var results = GenerateReport(validationSet, ann, name, delayStart, delayEnd);

                foreach (var @result in results)
                {
                    Console.WriteLine(@result);

                    if (output != null)
                    {
                        output.WriteLine(@result);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Error: {0}!", ex.ToString());
            }
            finally
            {
                if (output != null)
                {
                    output.Flush();
                    output.Close();
                }
                //Console.ReadKey();
            }
        }

        private static IEnumerable<string> GenerateReport(IDictionary<PointF, IEnumerable<PointF>> groups, ICalibration ann, string fileName, long startDelay, long endDelay)
        {
            /*
             * FileName StartDelay  EndDelay R2x R2y MSEx MSEy Err eDegX eDegY eDeg
             * 
             */

            var template = "{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}";

            var est = groups.SelectMany(x => x.Value.Select(y => new KeyValuePair<PointF, PointF>(x.Key, ann.GetGaze(y)))).ToList();

            /*
             * x.Key - wartość oczekiwana
             * x.Value - wartość wyliczona
             */

            var avgx = est.Average(x => x.Key.X);
            var avgy = est.Average(x => x.Key.Y);
            var cnt = est.Count();

            var SSresx = est.Sum(x => Math.Pow(x.Key.X - x.Value.X, 2));
            var SSresy = est.Sum(x => Math.Pow(x.Key.Y - x.Value.Y, 2));
            var SStotx = est.Sum(x => Math.Pow(x.Key.X - avgx, 2));
            var SStoty = est.Sum(x => Math.Pow(x.Key.Y - avgy, 2));

            var Err = est.Average(x => Math.Sqrt(Math.Pow(x.Key.X - x.Value.X, 2) + Math.Pow(x.Key.Y - x.Value.Y, 2)));

            var coefX = 40;
            var coefY = 32;

            var eDegX = est.Average(x => Math.Sqrt(Math.Pow((x.Key.X - x.Value.X) * coefX, 2)));
            var eDegY = est.Average(x => Math.Sqrt(Math.Pow((x.Key.Y - x.Value.Y) * coefY, 2)));

            var eDeg = est.Average(x => Math.Sqrt(Math.Pow((x.Key.X - x.Value.X) * coefX, 2) + Math.Pow((x.Key.Y - x.Value.Y) * coefY, 2)));

            var R2x = 1 - (SSresx / SStotx);
            var R2y = 1 - (SSresy / SStoty);
            var MSEx = SSresx / cnt;
            var MSEy = SSresy / cnt;

            yield return string.Format(template, fileName, startDelay, endDelay, R2x, R2y, MSEx, MSEy, Err, eDegX, eDegY, eDeg);
        }

        private static ICalibration Calibrate(IDictionary<PointF, IEnumerable<PointF>> groups)
        {
            var calib = new NeuralNetworkCalibration();

            calib.Calibrate(groups);

            return calib;
        }

        public static IEnumerable<TrackingPoint> ReadData(string fileName)
        {
            if (!File.Exists(fileName))
            {
                throw new Exception("File not exists!");
            }

            using (var file = File.OpenText(fileName))
            {
                while (!file.EndOfStream)
                {
                    var line = file.ReadLine();

                    if (string.IsNullOrWhiteSpace(line)) continue;

                    yield return TrackingPoint.FromLine(line);
                }
            }
        }

        public static IDictionary<PointF, IEnumerable<PointF>> GroupData(IEnumerable<TrackingPoint> points, long delayStart, long delayEnd)
        {
            var result = new Dictionary<PointF, IEnumerable<PointF>>();

            points = points.ToList();

            if (!points.Any())
            {
                return result;
            }

            // Pierwszy punkt testowy
            var ss = new TrackingPoint(TrackingPoint.PointType.SS, points.First().Time - 1, 0.5, 0.5);

            foreach (var point in points)
            {
                if (point.Type == TrackingPoint.PointType.SS)
                {
                    ss = point;
                    continue;
                }

                if (ss.Time + delayStart <= point.Time && ss.Time + recordTime - delayEnd > point.Time)
                {
                    AddResult(result, ss.Point, point.Point);
                }

            }

            return result;
        }

        private static void AddResult(Dictionary<PointF, IEnumerable<PointF>> result, PointF ss, PointF r)
        {
            if (result.ContainsKey(ss))
            {
                (result[ss] as IList<PointF>).Add(r);
            }
            else
            {
                result[ss] = new List<PointF>() { r };
            }
        }
    }

    public struct TrackingPoint
    {
        private const char SEPARATOR = '\t';

        public TrackingPoint(PointType type, long time, double x, double y)
            : this()
        {
            this.Type = type;
            this.Time = time;
            this.X = x;
            this.Y = y;
        }

        public PointType Type { get; set; }
        public long Time { get; set; }
        public double X { get; set; }
        public double Y { get; set; }

        public PointF Point
        {
            get { return new PointF((float)X, (float)Y); }
        }

        public override string ToString()
        {
            return string.Format("{0}\t{1}\t{2}\t{3}", Type.ToString(), Time, X.ToString(CultureInfo.InvariantCulture), Y.ToString(CultureInfo.InvariantCulture));
        }

        public static TrackingPoint FromLine(string line)
        {
            //R	1383154553625	0.5640227	0.5144398
            //SS	1383154553664	0.05	0.05

            var data = line.Split(SEPARATOR);

            PointType type;
            PointType.TryParse(data[0], out type);

            long time = long.Parse(data[1]);

            double x = double.Parse(data[2], CultureInfo.InvariantCulture);
            double y = double.Parse(data[3], CultureInfo.InvariantCulture);

            return new TrackingPoint(type, time, x, y);
        }

        public enum PointType
        {
            R = 0,
            SS = 1
        }
    }
}
