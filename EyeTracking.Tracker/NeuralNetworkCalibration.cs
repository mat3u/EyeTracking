namespace EyeTracking.Tracker
{
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;

    using AForge.Neuro;
    using AForge.Neuro.Learning;
    using System;

    public class NeuralNetworkCalibration : ICalibration
    {
        private readonly ActivationNetwork _network;
        private readonly BackPropagationLearning _learning;

        public NeuralNetworkCalibration()
        {
            this._network = new ActivationNetwork(new SigmoidFunction(), 2, 10, 2);
            this._learning = new BackPropagationLearning(this._network) { LearningRate = 0.5 };
        }

        public Network Network
        {
            get { return this._network; }
        }

        public virtual double Calibrate(IDictionary<PointF, IEnumerable<PointF>> data)
        {
            this._network.Randomize();

            double[][] inputs, outputs;

            this.PrepareDataSets(data, out inputs, out outputs);

            var error = double.MaxValue;
            var epoch = 0;

            for (; epoch < 2000 && error > 0.1d; epoch++)
            {
                error = this._learning.RunEpoch(inputs, outputs);
            }

            this.Ready = true;

            return error;
        }

        protected virtual void PrepareDataSets(IDictionary<PointF, IEnumerable<PointF>> data, out double[][] inputs, out double[][] outputs)
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
            outputs = sets.Select(x => x.D.Output).ToArray();
        }

        public PointF GetGaze(PointF point)
        {
            var result = this._network.Compute(new double[] { point.X, point.Y });

            return new PointF((float)result[0], (float)result[1]);
        }

        public void Save(string path)
        {
            this._network.Save(path);
        }

        public bool Ready { get; set; }
    }
}
