using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EyeTracking.Util.DP.Commands
{
    class RecalculateResultsUsingProportional : RecalculateResultsCommand
    {
        public RecalculateResultsUsingProportional(string[] args) 
            : base(args, new EyeTracking.Tracker.NeuralNetworkCalibrationProportional())
        {

        }

        public override string ToString()
        {
            return "PROP";
        }
    }
}
