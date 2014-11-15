namespace EyeTracking.Util.DP.Commands
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    class CalculateErrorsCommand : ICommand
    {
        private string[] _data;
        private AForge.Neuro.Network _neural;

        public CalculateErrorsCommand(string[] args)
        {
            this._data = File.ReadAllLines(args[0]);
            this._neural = AForge.Neuro.ActivationNetwork.Load(args[1]);
        }

        public void Execute()
        {
            var rp = this._data.Select(l =>
            {
                var s = l.Split(';');
                return new
                {
                    oX = double.Parse(s[0]),
                    oY = double.Parse(s[1]),
                    rX = double.Parse(s[2]),
                    rY = double.Parse(s[3])
                };
            });

            var all = (from p in rp
                       let g = this._neural.Compute(new[] { p.rX, p.rY })
                       select new
                       {
                           eX = Math.Abs(g[0] - p.oX),
                           eY = Math.Abs(g[1] - p.oY),
                           oX = p.oX,
                           oY = p.oY,
                           rX = p.rX,
                           rY = p.rY,
                           gX = g[0],
                           gY = g[1]
                       }).ToList();

            Console.WriteLine("oX; oY; rX; rY; gX; gY; eX; eY");

            foreach (var al in all)
            {
                Console.WriteLine("{0}; {1}; {2}; {3}; {4}; {5}; {6}; {7}", al.oX, al.oY, al.rX, al.rY, al.gX, al.gY, al.eX, al.eY);
            }

            Console.Error.WriteLine("{0}; {1}", all.Average(x => x.eX), all.Average(x => x.eY));
        }
    }
}
