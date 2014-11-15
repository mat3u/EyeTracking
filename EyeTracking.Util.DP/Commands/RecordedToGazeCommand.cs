namespace EyeTracking.Util.DP.Commands
{
    using System;
    using System.IO;
    using System.Linq;

    class RecordedToGazeCommand : ICommand
    {
        private string[] _data;
        private AForge.Neuro.Network _neural;

        public RecordedToGazeCommand(string[] args)
        {
            this._data = File.ReadAllLines(args[0]);
            this._neural = AForge.Neuro.ActivationNetwork.Load(args[1]);
        }

        public void Execute()
        {
            var recordedPoints = from l in this._data select new PointInTime(l);

            var screenPoints = recordedPoints.Select(p =>
            {
                var k = this._neural.Compute(new double[] { p.X, p.Y });

                return new PointInTime { T = p.T, X = k[0], Y = k[1] };
            });

            foreach (var screenPoint in screenPoints)
            {
                Console.WriteLine("{0}; {1}", screenPoint.X, screenPoint.Y);
            }
        }
    }
}
