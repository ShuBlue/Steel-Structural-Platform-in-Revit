using Test.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using Autodesk.Revit.DB;
using System.Collections.ObjectModel;

namespace Test.ViewModel
{
    public class MainViewModel : ViewModelBase
    {

        public MainViewModel(int n)
        {
            spaceInfos = new ObservableCollection<SpaceInfo>();
            colBracingInfos = new ObservableCollection<ColBracingInfo>();
            roofBracingInfos = new ObservableCollection<RofBracingInfo>();

            for (int i = 1; i <= n - 1; i++)
            {

                SpaceInfos.Add(new SpaceInfo() { SpaceName = "第" + i.ToString() + "跨", Space = 0 });
                ColBracingInfo colBracingInfo = new ColBracingInfo()
                {
                    SpaceName = "第" + i.ToString() + "跨",
                    SelectedBracingTypeL = "无",
                    SelectedBracingTypeR = "无"
                };
                RofBracingInfo rofBracingInfo = new RofBracingInfo()
                {
                    SpaceName = "第" + i.ToString() + "跨",
                    SelectedBracingTypeL = "无",
                    SelectedBracingTypeR = "无"
                };
                colBracingInfos.Add(colBracingInfo);
                roofBracingInfos.Add(rofBracingInfo);
            }
        }

        private ObservableCollection<SpaceInfo> spaceInfos;
        public ObservableCollection<SpaceInfo> SpaceInfos

        {
            get { return spaceInfos; }
            set
            {
                spaceInfos = value;
                RaisePropertyChanged();
            }
        }
        private ObservableCollection<ColBracingInfo> colBracingInfos;

        public ObservableCollection<ColBracingInfo> ColBracingInfos
        {
            get { return colBracingInfos; }
            set 
            {
                colBracingInfos = value;
                RaisePropertyChanged();
            }
        }
        private ObservableCollection<RofBracingInfo> roofBracingInfos;

        public ObservableCollection<RofBracingInfo> RoofBracingInfos
        {
            get { return roofBracingInfos; }
            set
            {
                roofBracingInfos = value;
                RaisePropertyChanged();
            }
        }


    }
}
