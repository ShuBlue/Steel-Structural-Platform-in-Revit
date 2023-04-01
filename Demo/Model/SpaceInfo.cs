using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.Model
{
    public class SpaceInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
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
        private int space;

        public int Space
        {
            get { return space; }
            set
            {
                space = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Space"));
            }
        }


    }
}
