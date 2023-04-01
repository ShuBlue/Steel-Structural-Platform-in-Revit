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
    /// LintelView.xaml 的交互逻辑
    /// </summary>
    public partial class PurlinView : UserControl
    {
        public static PurlinView _instance;
        private PurlinView()
        {
            InitializeComponent();

            PurlinNumberL.Text = "4";
            PurlinNumberR.Text = "4";
            PurlinSpaceL.Text = "1533";
            PurlinSpaceR.Text = "1533";
            PurlinRL.Text = "200";
            PurlinRR.Text = "200";
            PurlinCL.Text = "200";
            PurlinCR.Text = "200";
            IsNumberDefine.IsChecked = true;
        }
        public void Initial(Document doc)
        {
            //将梁类型添加至下拉列表
            FilteredElementCollector col2 = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_StructuralFraming);
            IList<Element> beamTypeList = col2.ToElements();
            foreach (var item in beamTypeList)
            {
                PurlinSelection.Items.Add(item.Name);
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
                PurlinMaterial.Items.Add(item.Name);
            }

        }
        public static PurlinView GetInstance()
        {
            if (_instance == null)
            {
                _instance = new PurlinView();   
            }
            return _instance;
        }

        private void IsNumberDefine_Checked(object sender, RoutedEventArgs e)
        {
            IsSpaceDefine.IsChecked = false;
            PurlinSpaceL.IsEnabled = false;
            PurlinSpaceLTip.IsEnabled = false;
            PurlinSpaceR.IsEnabled = false;
            PurlinSpaceRTip.IsEnabled = false;

        }

        private void IsNumberDefine_Unchecked(object sender, RoutedEventArgs e)
        {
            IsSpaceDefine.IsChecked = true;
            PurlinSpaceLTip.IsEnabled = true;
            PurlinSpaceL.IsEnabled = true;
            PurlinSpaceR.IsEnabled = true;
            PurlinSpaceRTip.IsEnabled = true;

        }

        private void IsSpaceDefine_Checked(object sender, RoutedEventArgs e)
        {
            IsNumberDefine.IsChecked = false;
            PurlinNumberLTip.IsEnabled = false;
            PurlinNumberL.IsEnabled = false;
            PurlinNumberRTip.IsEnabled = false;
            PurlinNumberR.IsEnabled = false;

        }

        private void IsSpaceDefine_Unchecked(object sender, RoutedEventArgs e)
        {
            IsNumberDefine.IsChecked = true;
            PurlinNumberLTip.IsEnabled = true;
            PurlinNumberL.IsEnabled = true;
            PurlinNumberRTip.IsEnabled = true;
            PurlinNumberR.IsEnabled = true;

        }
    }

}
