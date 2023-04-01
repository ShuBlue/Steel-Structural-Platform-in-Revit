using Autodesk.Revit.DB;
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

namespace Test.UserControls
{
    /// <summary>
    /// Framing.xaml 的交互逻辑
    /// </summary>
    public partial class FramingView : UserControl
    {
        private static FramingView _instance;
        private FramingView()
        {
            InitializeComponent();
            ColHeight.Text = "6000";
            WindResistantColSpace.Text = "6000;6000";
            RfHeight.Text = "900";
            IsSetWindResistantCol.IsChecked = true;
        }
        public void Initial(Document doc)
        {
            //将柱类型添加至下拉列表
            FilteredElementCollector cols = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_StructuralColumns);
            IList<Element> colTypeList = cols.ToElements();
            foreach (var item in colTypeList)
            {
                ColSelection.Items.Add(item.Name);
                WindResistantColSelection.Items.Add(item.Name);
            }
            //将梁类型添加至下拉列表
            FilteredElementCollector col2 = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_StructuralFraming);
            IList<Element> beamTypeList = col2.ToElements();
            foreach (var item in beamTypeList)
            {
                BeamSelection.Items.Add(item.Name);
                XiGanSelection.Items.Add(item.Name);
            }
            //将结构材料添加至下拉列表
            FilteredElementCollector col3 = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Materials);
            IList<Element> materialList = col3.ToElements();
            IList<Element> structuralmaterialList = new List<Element>();
            foreach (var item in materialList)
            {
                if (item.Name.Contains("Q"))
                {
                    structuralmaterialList.Add(item);

                }
            }
            foreach (var item in structuralmaterialList)
            {
                ColMaterial.Items.Add(item.Name);
                BeamMaterial.Items.Add(item.Name);
                XiGanMaterial.Items.Add(item.Name);
                WindResistantColMaterial.Items.Add(item.Name);
            }
            //将边界条件添加至下拉列表           
            Connection.Items.Add("固定");
            Connection.Items.Add("铰接");

        }
        public static FramingView GetInstance()
        {
            if (_instance == null)
            {
                _instance=new FramingView();
            }
            return _instance;
        }

        private void IsSetWindResistantCol_Checked(object sender, RoutedEventArgs e)
        {
            WindResistantColSelectionTip.IsEnabled = true;
            WindResistantColSelection.IsEnabled = true;
            WindResistantColMaterialTip.IsEnabled = true;
            WindResistantColMaterial.IsEnabled = true;
            WindResistantColNumberTip.IsEnabled = true;
            WindResistantColSpace.IsEnabled = true;
        }

        private void IsSetWindResistantCol_Unchecked(object sender, RoutedEventArgs e)
        {
            WindResistantColSelectionTip.IsEnabled = false;
            WindResistantColSelection.IsEnabled = false;
            WindResistantColMaterialTip.IsEnabled = false;
            WindResistantColMaterial.IsEnabled = false;
            WindResistantColNumberTip.IsEnabled = false;
            WindResistantColSpace.IsEnabled = false;
        }

        private void IsRoofBeam_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
