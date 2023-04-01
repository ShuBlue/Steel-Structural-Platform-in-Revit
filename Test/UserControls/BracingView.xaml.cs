using Autodesk.Revit.DB;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Test.ViewModel;

namespace Test.UserControls
{
    /// <summary>
    /// BracingView.xaml 的交互逻辑
    /// </summary>
    public partial class BracingView : UserControl
    {
        private static BracingView _instance;
        public MainViewModel VM { get; set; }
        public int bentNumber { get; set; }
        private BracingView()
        {
            InitializeComponent();

            
        }
        public void Initial(Document doc)
        {

            BracingNumber.Text = "0";
            //将梁类型添加至下拉列表
            FilteredElementCollector col2 = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_StructuralFraming);
            IList<Element> beamTypeList = col2.ToElements();
            foreach (var item in beamTypeList)
            {
                ColBracingSelection.Items.Add(item.Name);
                RofBracingSelection.Items.Add(item.Name);
            }
            //将结构材料添加至下拉列表
            FilteredElementCollector col3 = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Materials);
            IList<Element> materialList = col3.ToElements();
            IList<Element> structuralmaterialList = new List<Element>();
            foreach (var item in materialList)
            {
                if ( item.Name.Contains("Q"))
                {
                    structuralmaterialList.Add(item);

                }
            }
            foreach (var item in structuralmaterialList)
            {
                ColBracingMaterial.Items.Add(item.Name);
                RofBracingMaterial.Items.Add(item.Name);
            }
            Messenger.Default.Register<int>(this, "bentNumber", t =>
            {
                this.bentNumber = t;
                VM = new MainViewModel(bentNumber);
                DataContext = VM;
            });
            VM=new MainViewModel(6);
            DataContext = VM;
        }
        public static BracingView GetInstance()
        {
            if (_instance == null)
            {
                _instance = new BracingView();
            }
            return _instance;
        }

    }
}
