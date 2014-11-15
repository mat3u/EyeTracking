using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EyeTracking.Util.DP.Commands
{
    class RecalculateResultsUsingCenterCommand : RecalculateResultsCommand
    {
        public RecalculateResultsUsingCenterCommand(string[] args) 
            : base(args, new EyeTracking.Tracker.NeuralNetworkCalibrationMassCenter())
        {

        }

        public override string ToString()
        {
            return "CENTER";
        }
    }
}
