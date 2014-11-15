namespace EyeTracking.Aggregate
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using System.Linq;

    class Arguments
    {
        public Arguments()
        {
            this.Column = -1;
        }

        [Required]
        public string Input { get; set; }

        public string Delimiter { get; set; }

        public string Function { get; set; }

        public int Column { get; set; }

        public string GetDelimiter()
        {
            switch (this.Delimiter)
            {
                case "\\s":
                    return " ";
                case "":
                case null:
                    return "\t";
                default:
                    return this.Delimiter;
            }
        }

        public Func<IEnumerable<float>, float> GetFunction()
        {
            switch (this.Function)
            {
                case "MAX":
                    return _ => _.Max();
                case "MIN":
                    return _ => _.Min();
                case "COUNT":
                    return _ => _.Count();
                case "SUM":
                    return _ => _.Sum();
                default:
                    return _ => _.Average();
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var config = Args.Configuration.Configure<Arguments>().CreateAndBind(args);

            if (!File.Exists(config.Input))
            {
                Console.Error.WriteLine("Input file not found!");
                return;
            }

            var delimiter = config.GetDelimiter();
            var function = config.GetFunction();

            var lines = File.ReadAllLines(config.Input);
            var all = lines.Select(x => x.Split(new[] { delimiter }, StringSplitOptions.None)).ToList();

            var s = all.FirstOrDefault();

            if (s == null)
            {
                return;
            }

            var columns = s.Count();

            if (config.Column != -1 && config.Column >= 0 && columns > config.Column)
            {
                var groups = all.GroupBy(x => x[config.Column]);

                foreach (var @group in groups)
                {
                    var r = Group(@group.AsEnumerable(), columns, config.Column, config.GetFunction());
                    Console.WriteLine(string.Join("\t", r));
                }
            }
            else
            {
                var r = Group(all, columns, config.Column, config.GetFunction());
                Console.WriteLine(string.Join("\t", r));
            }
        }

        public static IEnumerable<string> Group(IEnumerable<string[]> data, int n, int k, Func<IEnumerable<float>, float> agg)
        {
            var p = data.ToList();

            for (int i = 0; i < n; i++)
            {
                if (i == k)
                {
                    yield return p.First()[i];
                }
                else
                {

                    float vfloat = 0.0f;

                    var t = p.Select(x => x[i]).ToList();
                    var f = t.First();

                    if (float.TryParse(f, out vfloat))
                    {
                        yield return agg(t.Select(float.Parse)).ToString();
                    }
                    else
                    {
                        yield return "X";
                    }
                }
            }
        }
    }
}
