using Demo.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public MainViewModel(int n)
        {
            spaceInfos = new List<SpaceInfo>();
            colBracingInfos = new List<ColBracingInfo>();
            roofBracingInfos = new List<RofBracingInfo>();
            for (int i = 1; i <= n - 1; i++)
            {

                SpaceInfos.Add(new SpaceInfo() { SpaceName ="第" + i.ToString() + "跨", Space = 0 });
                ColBracingInfo colBracingInfo = new ColBracingInfo()
                {
                    SpaceName ="第" + i.ToString() + "跨",
                    SelectedBracingTypeL = "无",
                    SelectedBracingTypeR = "无" 
                };
                RofBracingInfo rofBracingInfo = new RofBracingInfo()
                {
                    SpaceName ="第" + i.ToString() + "跨",
                    SelectedBracingTypeL = "无",
                    SelectedBracingTypeR = "无"
                };
                colBracingInfos.Add(colBracingInfo);
                roofBracingInfos.Add(rofBracingInfo);
            }
        }

        private List<SpaceInfo> spaceInfos;
        public List<SpaceInfo> SpaceInfos

        {
            get { return spaceInfos; }
            set
            {
                spaceInfos = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SpaceInfos"));
            }
        }
        private List<ColBracingInfo> colBracingInfos;

        public List<ColBracingInfo> ColBracingInfos
        {
            get { return colBracingInfos; }
            set 
            {
                colBracingInfos = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ColBracingInfos"));
            }
        }
        private List<RofBracingInfo> roofBracingInfos;

        public List<RofBracingInfo> RoofBracingInfos
        {
            get { return roofBracingInfos; }
            set
            {
                roofBracingInfos = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("RoofBracingInfos"));
            }
        }

    }
}
