using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.Entity
{
    public class LoadCombinationInfo
    {
        public LoadCombinationInfo()
        {
            this.Tag = null;
            this.Dead = 0;
            this.Live = 0;
            this.WX0_0 = 0;
            this.WX0_1 = 0;
            this.WX1_0 = 0;
            this.WX1_1 = 0;
            this.WY0_0 = 0;
            this.WY0_1 = 0;
            this.WY1_0 = 0;
            this.WY1_1 = 0;
            this.EX = 0;
            this.EY = 0;

        }
        public string Tag { get; set;}
        public double Dead { get; set; }
        public double Live { get; set; }
        public double WX0_0 { get; set; }
        public double WX0_1 { get; set; }
        public double WX1_0 { get; set; }
        public double WX1_1 { get; set; }
        public double WY0_0 { get; set; }
        public double WY0_1 { get; set; }
        public double WY1_0 { get; set; }
        public double WY1_1 { get; set; }
        public double EX { get; set; }
        public double EY { get; set; }
    }
}
