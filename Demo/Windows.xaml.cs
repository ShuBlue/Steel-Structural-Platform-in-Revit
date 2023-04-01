using Autodesk.Revit.DB;
using Demo.ViewModel;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Demo
{
    /// <summary>
    /// Window.xaml 的交互逻辑
    /// </summary>
    public partial class Windows : Window
    {

        public MainViewModel VM { get; set; }
        public Windows(Document doc)
        {
            InitializeComponent();
            //默认参数
            BentNumber.Text = "5";
            NetDepth.Text = "10000";
            Space.Text = "3000";
            ColHeight.Text = "6000";
            WindResistantColNumber.Text = "2";
            RfHeight.Text = "2000";
            //RfAngle.Text = "21.8";
            PurlinNumberL.Text = "4";
            PurlinNumberR.Text = "4";
            PurlinSpaceL.Text = "1533";
            PurlinSpaceR.Text = "1533";
            PurlinRL.Text = "200";
            PurlinRR.Text = "200";
            PurlinCL.Text = "200";
            PurlinCR.Text = "200";
            
            IsUniform.IsChecked = true;
            IsRoofBeam.IsChecked = true;
            IsSetWindResistantCol.IsChecked = true;
            IsNumberDefine.IsChecked = true;
           
            //将柱类型添加至下拉列表
            FilteredElementCollector col1 = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_StructuralColumns);
            IList<Element> colTypeList = col1.ToElements();
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
                RfSelection.Items.Add(item.Name);
                RfBeamSelection.Items.Add(item.Name);
                PurlinSelection.Items.Add(item.Name);
                ColBracingSelection.Items.Add(item.Name);
                RofBracingSelection.Items.Add(item.Name);
            }
            //将结构材料添加至下拉列表
            FilteredElementCollector col3 = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Materials);
            IList<Element> materialList = col3.ToElements();
            IList<Element> structuralmaterialList = new List<Element>();
            foreach (var item in materialList)
            {
                if (item.Name.Contains("金属") || item.Name.Contains("钢"))
                {
                    structuralmaterialList.Add(item);

                }
            }
            foreach (var item in structuralmaterialList)
            {
                ColMaterial.Items.Add(item.Name);
                BeamMaterial.Items.Add(item.Name);
                RfMaterial.Items.Add(item.Name);
                RfBeamMaterial.Items.Add(item.Name);
                PurlinMaterial.Items.Add(item.Name);
                ColBracingMaterial.Items.Add(item.Name);
                RofBracingMaterial.Items.Add(item.Name);
                WindResistantColMaterial.Items.Add(item.Name);
            }
            //将边界条件添加至下拉列表           
            Connection.Items.Add("固定");
            Connection.Items.Add("铰接");

            VM = new MainViewModel(Convert.ToInt32(BentNumber.Text));
            DataContext = VM;

        }



        private void IsUniform_Checked(object sender, RoutedEventArgs e)
        {
            IsRandom.IsChecked = false;
            SpaceList.IsEnabled = false;
        }


        private void IsRandom_Checked(object sender, RoutedEventArgs e)
        {
            IsUniform.IsChecked = false;
            Space.IsEnabled = false;

        }

        private void IsUniform_Unchecked(object sender, RoutedEventArgs e)
        {
            IsRandom.IsChecked = true;
            SpaceList.IsEnabled = true;
        }

        private void IsRandom_Unchecked(object sender, RoutedEventArgs e)
        {
            IsUniform.IsChecked = true;
            Space.IsEnabled = true;
        }

        private void submit_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            this.Close();
        }
        //框架数量发生变化，通知DataGrid中的行发生相应的变化。
        private void BentNumber_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(BentNumber.Text, out int result))
            {
                Length.Text = (space * (result - 1)).ToString();
                VM = new MainViewModel(result);
            }        
            else
            {
                VM = new MainViewModel(5);
            }
            DataContext = VM;
        }

        private int space;
        private void Space_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(Space.Text, out int result))
            {
                if (BentNumber.Text != null)
                {
                    Length.Text = (result * (Convert.ToInt32(BentNumber.Text) - 1)).ToString();
                    space = result;
                }
                else
                {
                    Length.Text = (result * (5 - 1)).ToString();
                    space = result;
                }
            }
            else
            {
                if (BentNumber.Text != null)
                {
                    Length.Text = (3000 * (Convert.ToInt32(BentNumber.Text) - 1)).ToString();
                    space = 3000;
                }
                else
                {
                    Length.Text = (result * (5 - 1)).ToString();
                    space = 3000;
                }
            }
        }
        private void IsSetWindResistantCol_Checked(object sender, RoutedEventArgs e)
        {
            WindResistantColSelectionTip.IsEnabled = true;
            WindResistantColSelection.IsEnabled = true;
            WindResistantColMaterialTip.IsEnabled = true;
            WindResistantColMaterial.IsEnabled = true;
            WindResistantColNumberTip.IsEnabled = true;
            WindResistantColNumber.IsEnabled = true;
        }

        private void IsSetWindResistantCol_Unchecked(object sender, RoutedEventArgs e)
        {
            WindResistantColSelectionTip.IsEnabled = false;
            WindResistantColSelection.IsEnabled = false;
            WindResistantColMaterialTip.IsEnabled = false;
            WindResistantColMaterial.IsEnabled = false;
            WindResistantColNumberTip.IsEnabled = false;
            WindResistantColNumber.IsEnabled = false;
        }

        private void IsRoofBeam_Checked(object sender, RoutedEventArgs e)
        {
            RfBeamSelectionTip.IsEnabled = true;
            RfBeamSelection.IsEnabled = true;
            RfBeamMaterialTip.IsEnabled = true;
            RfBeamMaterial.IsEnabled = true;
        }

        private void IsRoofBeam_Unchecked(object sender, RoutedEventArgs e)
        {
            RfBeamSelectionTip.IsEnabled = false;
            RfBeamSelection.IsEnabled = false;
            RfBeamMaterialTip.IsEnabled = false;
            RfBeamMaterial.IsEnabled = false;


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
