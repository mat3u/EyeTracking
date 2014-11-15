namespace EyeTracking.Tracker
{
    using System;
    using System.Linq;

    public class NeuralNetworkCalibrationMassCenter : NeuralNetworkCalibration
    {
        public NeuralNetworkCalibrationMassCenter()
            : base()
        {
        }

        protected override void PrepareDataSets(System.Collections.Generic.IDictionary<System.Drawing.PointF, System.Collections.Generic.IEnumerable<System.Drawing.PointF>> data, out double[][] inputs, out double[][] outputs)
        {
            var r = new Random();

            var sets = data.Select(x => new
            {
                D = new
                {
                    Output = new double[] { x.Key.X, x.Key.Y },
                    Input = new double[] { x.Value.Average(y => y.X), x.Value.Average(y => y.Y) }
                },
                R = r.Next()
            })
            .OrderBy(x => x.R)
            .ToList();

            inputs = sets.Select(x => x.D.Input).ToArray();
            outputs = sets.Select(x => x.D.Output).ToArray();
        }
    }
}
