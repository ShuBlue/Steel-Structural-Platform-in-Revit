using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.Entity
{
    public class EarthquakeInfo
    {
        public string SeismicIntensity { get; set; }
        public double AlphaMax { get; set; }
        public string SiteCategory { get; set; }
        public string ClassificationOfDesignEarthquake { get; set; }
        public double CharacteristicPeriod { get; set; }
        public double PeriodTimeReductionFactor { get; set; }
        public double DampingRatio { get; set; }
        public string ModesCombination { get; set; }
    }
}
