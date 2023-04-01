using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.Model
{
    public class RofBracingInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public RofBracingInfo()
        {
            bracingTypeList = new List<BracingType>
            {
                new BracingType{ BracingTypeName="无"},
                new BracingType{ BracingTypeName="十字形支撑"},
            };
        }
        private string spaceName;

        public string SpaceName
        {
            get { return spaceName; }
            set
            {
                spaceName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SpaceName"));
            }
        }
        private string selectedBracingTypeL;
        public string SelectedBracingTypeL
        {
            get { return selectedBracingTypeL; }
            set
            {
                selectedBracingTypeL = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedBracingTypeL"));
            }
        }
        private string selectedBracingTypeR;
        public string SelectedBracingTypeR
        {
            get { return selectedBracingTypeR; }
            set
            {
                selectedBracingTypeR = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedBracingTypeR"));
            }
        }

        private List<BracingType> bracingTypeList;

        public List<BracingType> BracingTypeList
        {
            get { return bracingTypeList; }
            set
            {
                bracingTypeList = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("BracingTypeList"));
            }
        }
    }
}
