using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_private.Classes
{
    public class CalculateArgs
    {
        public double Accuracy { get; set; } = 100;
        public int Combo { get; set; }
        public int Score { get; set; }
        public string[] Mods { get; set; } = Array.Empty<string>();
        public int? Time { get; set; }
    }
}
