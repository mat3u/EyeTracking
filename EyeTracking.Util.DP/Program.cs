using System.Collections;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;

namespace EyeTracking.Util.DP
{
    using System;
    using System.Drawing;

    class Program
    {
        private static void Main(string[] args)
        {
            if (args.Count() < 1)
            {
                Console.WriteLine("Usage:\n\tdp <Command> [<param1>[, <param2>[...]]]\n\nMateusz Stasch (c) 2013 | matt.stasch@gmail.com"); ;
                return;
            }

            var arg = args[0];
            var cmdArgs = args.Skip(1).ToArray();

            switch (arg)
            {
                case "R2G":
                    (new Commands.RecordedToGazeCommand(cmdArgs)).Execute();
                    break;
                case "ERROR":
                    (new Commands.CalculateErrorsCommand(cmdArgs)).Execute();
                    break;
                case "CENTER":
                    (new Commands.RecalculateResultsUsingCenterCommand(cmdArgs)).Execute();
                    break;
                case "PROP":
                    (new Commands.RecalculateResultsUsingProportional(cmdArgs)).Execute();
                    break;
                case "NAIVE":
                    (new Commands.RecalculateResultsUsingNaiveCommand(cmdArgs)).Execute();
                    break;
                case "SEP":
                    (new Commands.RecalculateResultsUsingSeparated(cmdArgs)).Execute();
                    break;
                default:
                    Console.Error.WriteLine("Unknown command!");
                    break;
            }
        }
    }
}
