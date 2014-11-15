namespace EyeTracking.Tracker
{
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;

    public class NeuralNetworkCalibrationProportional : NeuralNetworkCalibrationMassCenter
    {
        private readonly NeuralNetworkCalibration _naive;

        public NeuralNetworkCalibrationProportional()
        {
            this._naive = new NeuralNetworkCalibration();
        }

        protected override void PrepareDataSets(IDictionary<PointF, IEnumerable<PointF>> data, out double[][] inputs, out double[][] outputs)
        {
            double[][] cInputs, cOutputs;

            base.PrepareDataSets(data, out cInputs, out cOutputs);

            this._naive.Calibrate(data);

            var sets = data.SelectMany(x => x.Value).AsParallel().Select(x =>
            {
                var tmp = this._naive.GetGaze(x);

                return new { Inputs = new double[] {(double) x.X, (double) x.Y}, Outputs = new double[] {(double) tmp.X, (double) tmp.Y} };
            }).ToList();

            inputs = sets.Select(x => x.Inputs).ToArray();
            outputs = sets.Select(x => x.Outputs).ToArray();

            this.Ready = true;
        }
    }
}
