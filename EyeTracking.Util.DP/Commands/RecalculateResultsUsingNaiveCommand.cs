namespace EyeTracking.Util.DP.Commands
{
    class RecalculateResultsUsingNaiveCommand : RecalculateResultsCommand
    {
        public RecalculateResultsUsingNaiveCommand(string[] args) 
            : base(args, new EyeTracking.Tracker.NeuralNetworkCalibration())
        {

        }

        public override string ToString()
        {
            return "NAIVE";
        }
    }
}
