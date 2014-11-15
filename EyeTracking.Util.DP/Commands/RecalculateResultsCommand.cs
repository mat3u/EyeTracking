namespace EyeTracking.Util.DP.Commands
{
    using EyeTracking.Tracker;
    using System;
    using System.Drawing;
    using System.IO;
    using System.Linq;

    internal abstract class RecalculateResultsCommand : ICommand
    {
        private string[] _recorded;
        private string[] _validation;
        private ICalibration _calib;
        
        protected RecalculateResultsCommand(string[] args, ICalibration calib)
        {

            this._recorded = File.ReadAllLines(args[0]);
            this._validation = File.ReadAllLines(args[1]);
            this._calib = calib;
        }

        public void Execute()
        {
            for (int i = 0; i < 3; i++)
            {
                var rp = this._recorded.Select(l =>
                {
                    var s = l.Split('\t');
                    return new
                    {
                        o = new PointF((float)double.Parse(s[0]), (float)double.Parse(s[1])),
                        r = new PointF((float)double.Parse(s[2]), (float)double.Parse(s[3])),
                    };
                }).GroupBy(x => x.o).ToDictionary(x => x.Key, x => x.Select(y => y.r));

                this._calib.Calibrate(rp);

                var vp = this._validation.Select(l =>
                {
                    var s = l.Split('\t');
                    return new
                    {
                        o = new PointF((float)double.Parse(s[0]), (float)double.Parse(s[1])),
                        r = new PointF((float)double.Parse(s[2]), (float)double.Parse(s[3])),
                    };
                }).Select(x => new
                {
                    o = x.o,
                    c = x.r,
                    g = this._calib.GetGaze(x.r)
                }).Select(x => new
                {
                    o = x.o,
                    c = x.c,
                    g = x.g,
                    eX = Math.Abs(x.o.X - x.g.X),
                    eY = Math.Abs(x.o.Y - x.g.Y)
                }).ToList();

                var emx = vp.Average(x => x.eX) * 46.86;
                var emy = vp.Average(x => x.eY) * 30.53;

                Console.Write("{0}\t{1}\t{2}\t{3}", this.ToString(), emx, emy, Math.Sqrt(emx * emx + emy * emy));
            }
        }
    }
}
