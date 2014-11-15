using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EyeTracking.Util.DP.Commands
{
    class RecalculateResultsUsingSeparated : RecalculateResultsCommand
    {
        public RecalculateResultsUsingSeparated(string[] args) 
            : base(args, new EyeTracking.Tracker.NeuralNetworkCalibrationSeparated())
        {

        }

        public override string ToString()
        {
            return "SEP";
        }
    }
}
