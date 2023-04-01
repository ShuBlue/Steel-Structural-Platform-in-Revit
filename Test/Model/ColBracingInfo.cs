using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.Model
{
    public class ColBracingInfo :ViewModelBase
    {
        public ColBracingInfo()
        {
            bracingTypeList = new List<BracingType>
            {
                new BracingType{ BracingTypeName="无"},
                new BracingType{ BracingTypeName="十字形支撑"},
                new BracingType{ BracingTypeName="人字形支撑"}
            };
        }
        private string spaceName;

        public string SpaceName
        {
            get { return spaceName; }
            set 
            {
                spaceName = value;
                RaisePropertyChanged();
            }
        }
        private string selectedBracingTypeL;
        public string SelectedBracingTypeL
        {
            get { return selectedBracingTypeL; }
            set 
            {
                selectedBracingTypeL = value;
                RaisePropertyChanged();
            }
        }
        private string selectedBracingTypeR;
        public string SelectedBracingTypeR
        {
            get { return selectedBracingTypeR; }
            set
            {
                selectedBracingTypeR = value;
                RaisePropertyChanged();
            }
        }

        private List<BracingType> bracingTypeList;

        public List<BracingType> BracingTypeList
        {
            get { return bracingTypeList; }
            set 
            {
                bracingTypeList = value;
                RaisePropertyChanged();
            }
        }


    }

}
