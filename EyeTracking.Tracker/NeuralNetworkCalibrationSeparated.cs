using System.Linq;
using AForge.Neuro;
using AForge.Neuro.Learning;

namespace EyeTracking.Tracker
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;

    public class NeuralNetworkCalibrationSeparated : ICalibration
    {
        private readonly ActivationNetwork _networkX;
        private readonly ActivationNetwork _networkY;
        private readonly BackPropagationLearning _learningX;
        private readonly BackPropagationLearning _learningY;

        public NeuralNetworkCalibrationSeparated()
        {
            this._networkX = new ActivationNetwork(new SigmoidFunction(), 2, 8, 1);
            this._networkY = new ActivationNetwork(new SigmoidFunction(), 2, 8, 1);

            this._learningX = new BackPropagationLearning(this._networkX) { LearningRate = 0.35 };
            this._learningY = new BackPropagationLearning(this._networkY) { LearningRate = 0.35 };
        }

        public double Calibrate(IDictionary<PointF, IEnumerable<PointF>> data)
        {
            double[][] i, x, y;

            this.PrepareDataSets(data, out i, out x, out y);

            var errorX = double.MaxValue;
            var errorY = double.MaxValue;
            var epoch = 0;

            for (; epoch < 2000 && errorX > 0.1d; epoch++)
            {
                errorX = this._learningX.RunEpoch(i, x);
            }

            for (epoch = 0; epoch < 2000 && errorY > 0.1d; epoch++)
            {
                errorY = this._learningY.RunEpoch(i, y);
            }

            this.Ready = true;

            return (errorX + errorY)*0.5;
        }

        public bool Ready { get; set; }

        public PointF GetGaze(PointF point)
        {
            var x = this._networkX.Compute(new double[] { point.X, point.Y });
            var y = this._networkY.Compute(new double[] { point.X, point.Y });

            return new PointF((float)x[0], (float)y[0]);
        }

        public void Save(string path)
        {
        }

        protected virtual void PrepareDataSets(IDictionary<PointF, IEnumerable<PointF>> data, out double[][] inputs, out double[][] outputsX, out double[][] outputsY)
        {
            var r = new Random();

            var sets = data.Select(x => x.Value.Select(y =>
                new
                {
                    Output = new double[] { x.Key.X, x.Key.Y },
                    Input = new double[] { y.X, y.Y }
                })
                )
                .SelectMany(x => x)
                .Select(x => new { D = x, R = r.Next() })
                .OrderBy(x => x.R)
                .ToList();

            inputs = sets.Select(x => x.D.Input).ToArray();
            outputsX = sets.Select(x => new[] { x.D.Output[0] }).ToArray();
            outputsY = sets.Select(x => new[] { x.D.Output[1] }).ToArray();
        }
    }
}
