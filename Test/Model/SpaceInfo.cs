using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.Model
{
    public class SpaceInfo : ViewModelBase
    {       
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
        private int space;

        public int Space
        {
            get { return space; }
            set
            {
                space = value;
                RaisePropertyChanged();
            }
        }


    }
}
