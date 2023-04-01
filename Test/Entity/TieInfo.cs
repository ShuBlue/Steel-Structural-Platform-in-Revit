using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.Entity
{
    public class TieInfo
    {
        public int Id { get; set; }
        public string StructuralType { get; set; }
        public XYZ Start { get; set; }
        public XYZ End { get; set; }
        public string SectionShape { get; set; }
        public string SectionDimension { get; set; }
        public string Material { get; set; }
    }
}
