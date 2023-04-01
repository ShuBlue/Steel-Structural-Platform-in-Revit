using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HandyControl.Controls;
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
using Test.Extensions;
using Test.Model;
using System.Security.RightsManagement;
using MessageBox = System.Windows.MessageBox;

namespace Test.UserControls
{
    /// <summary>
    /// CreationRVT.xaml 的交互逻辑
    /// </summary>
    public partial class RVTCreationView : UserControl
    {


        public RVTCreationView(Document doc)
        {
            InitializeComponent();
            this.doc = doc;

            FilteredElementCollector viewfilter = new FilteredElementCollector(doc).OfClass(typeof(View));
            List<View> viewList = viewfilter.Cast<View>().Where(t => t.CanBePrinted == true).ToList();
            foreach (var item in viewList)
            {
                this.views.Items.Add(item.Name);
            }
            views.SelectedIndex = 0;

        }




        public Document doc { get; set; }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            GridAndLevelView gridAndLevelView = GridAndLevelView.GetInstance();
            FramingView framingView = FramingView.GetInstance();
            PurlinView purlinView = PurlinView.GetInstance();
            BracingView bracingView = BracingView.GetInstance();
            int bentNumber = Convert.ToInt32(gridAndLevelView.BentNumber.Text);
            double depth = UnitExtension.ConvertToFeet(Convert.ToDouble(gridAndLevelView.NetDepth.Text));
            double length = UnitExtension.ConvertToFeet(Convert.ToDouble(gridAndLevelView.Length.Text));
            double colHeight = UnitExtension.ConvertToFeet(Convert.ToDouble(framingView.ColHeight.Text));
            double roofHeight = UnitExtension.ConvertToFeet(Convert.ToDouble(framingView.RfHeight.Text));
            //.....
            double purlinNumberL = Convert.ToDouble(purlinView.PurlinNumberL.Text);
            double purlinNumberR = Convert.ToDouble(purlinView.PurlinNumberR.Text);
            double purlinSpaceL = UnitExtension.ConvertToFeet(Convert.ToDouble(purlinView.PurlinSpaceL.Text));
            double purlinSpaceR = UnitExtension.ConvertToFeet(Convert.ToDouble(purlinView.PurlinSpaceR.Text));
            double purlinRR = UnitExtension.ConvertToFeet(Convert.ToDouble(purlinView.PurlinRR.Text));
            double purlinCR = UnitExtension.ConvertToFeet(Convert.ToDouble(purlinView.PurlinCR.Text));
            double purlinCL = UnitExtension.ConvertToFeet(Convert.ToDouble(purlinView.PurlinCL.Text));
            double purlinRL = UnitExtension.ConvertToFeet(Convert.ToDouble(purlinView.PurlinRL.Text));
            double bracingNumber = Convert.ToDouble(bracingView.BracingNumber.Text);
            //拿到柱所在位置
            List<double> spaceList = new List<double>();
            if (gridAndLevelView.IsUniform.IsChecked == true)
            {
                double space = UnitExtension.ConvertToFeet(Convert.ToInt32(gridAndLevelView.Space.Text));
                for (int i = 0; i < bentNumber; i++)
                {
                    spaceList.Add(i * space);
                }
            }
            else
            {
                double sum = 0;
                spaceList.Add(0);
                foreach (SpaceInfo item in gridAndLevelView.SpaceList.Items)
                {
                    sum += item.Space;
                    spaceList.Add(UnitExtension.ConvertToFeet(sum));
                }
            }
            List<XYZ> pColL = new List<XYZ>();
            List<XYZ> pColR = new List<XYZ>();
            for (int i = 0; i < bentNumber; i++)
            {
                pColR.Add(new XYZ(spaceList[i], 0, 0));
                pColL.Add(new XYZ(spaceList[i], depth, 0));
            }
            //拿到柱底标高
            FilteredElementCollector lvlFilter = new FilteredElementCollector(doc).OfClass(typeof(Level));
            IList<Element> lvlList = lvlFilter.ToElements();
            Level ColBtmlevel = null;
            foreach (Level item in lvlList)
            {
                if (item.Elevation == 0)
                {
                    ColBtmlevel = item;
                }
            }
            //拿到柱的族类型
            FilteredElementCollector colFilter = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_StructuralColumns);
            IList<Element> colTypeList = colFilter.ToElements();
            FamilySymbol colType = null;
            foreach (FamilySymbol item in colTypeList)
            {
                if (item.Name == framingView.ColSelection.SelectedItem.ToString())
                {
                    colType = item;
                    break;
                }
            }
            //拿到抗风柱的族类型
            FilteredElementCollector windResistantColFilter = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_StructuralColumns);
            IList<Element> windResistantColTypeList = windResistantColFilter.ToElements();
            FamilySymbol windResistantColType = null;
            foreach (FamilySymbol item in windResistantColTypeList)
            {
                if (item.Name == framingView.WindResistantColSelection.SelectedItem.ToString())
                {
                    windResistantColType = item;
                    break;
                }
            }
            //拿到梁的族类型
            FilteredElementCollector beamFilter = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_StructuralFraming);
            IList<Element> beamTypeList = beamFilter.ToElements();
            FamilySymbol beamType = null;
            foreach (FamilySymbol item in beamTypeList)
            {
                if (item.Name == framingView.BeamSelection.SelectedItem.ToString())
                {
                    beamType = item;
                    break;
                }
            }

            //拿到系杆的族类型
            FilteredElementCollector xiGanFilter = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_StructuralFraming);
            IList<Element> xiGanTypeList = xiGanFilter.ToElements();
            FamilySymbol xiGanType = null;
            foreach (FamilySymbol item in xiGanTypeList)
            {
                if (item.Name == framingView.XiGanSelection.SelectedItem.ToString())
                {
                    xiGanType = item;
                    break;
                }
            }
            //拿到屋架顶点
            List<XYZ> pRoof = new List<XYZ>();
            for (int i = 0; i < bentNumber; i++)
            {
                pRoof.Add(new XYZ(spaceList[i], depth / 2, colHeight + roofHeight));
            }
            //拿到柱顶点
            List<XYZ> pColTopR = new List<XYZ>();
            List<XYZ> pColTopL = new List<XYZ>();
            foreach (var item in pColR)
            {
                pColTopR.Add(item + new XYZ(0, 0, colHeight));
            }
            foreach (var item in pColL)
            {
                pColTopL.Add(item + new XYZ(0, 0, colHeight));
            }
            //拿到檩条的族类型
            FilteredElementCollector purlinFilter = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_StructuralFraming);
            IList<Element> purlinTypeList = purlinFilter.ToElements();
            FamilySymbol purlinType = null;
            foreach (FamilySymbol item in purlinTypeList)
            {
                if (item.Name == purlinView.PurlinSelection.SelectedItem.ToString())
                {
                    purlinType = item;
                    break;
                }
            }
            //拿到柱间支撑的族类型
            FilteredElementCollector colBracingFilter = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_StructuralFraming);
            IList<Element> colBracingTypeList = colBracingFilter.ToElements();
            FamilySymbol colBracingType = null;
            foreach (FamilySymbol item in colBracingTypeList)
            {
                if (item.Name == bracingView.ColBracingSelection.SelectedItem.ToString())
                {
                    colBracingType = item;
                    break;
                }
            }
            //拿到屋面支撑的族类型
            FilteredElementCollector roofBracingFilter = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_StructuralFraming);
            IList<Element> roofBracingTypeList = roofBracingFilter.ToElements();
            FamilySymbol roofBracingType = null;
            foreach (FamilySymbol item in roofBracingTypeList)
            {
                if (item.Name == bracingView.RofBracingSelection.SelectedItem.ToString())
                {
                    roofBracingType = item;
                    break;
                }
            }
            //拿到材质
            FilteredElementCollector materialFilter = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Materials);
            IList<Element> materialList = materialFilter.ToElements();
            Material colMaterial = null;
            Material windResistantColMaterial = null;
            Material beamMaterial = null;
            Material xiGanMaterial = null;
            Material purlinMaterial = null;
            Material colBracingMaterial = null;
            Material roofBracingMaterial = null;
            foreach (Material item in materialList)
            {
                if (item.Name == framingView.ColMaterial.SelectedItem.ToString())
                {
                    colMaterial = item;
                }
                if (item.Name == framingView.BeamMaterial.SelectedItem.ToString())
                {
                    beamMaterial = item;
                }
                if (item.Name == framingView.XiGanMaterial.SelectedItem.ToString())
                {
                    xiGanMaterial = item;
                }
                if (item.Name == purlinView.PurlinMaterial.SelectedItem.ToString())
                {
                    purlinMaterial = item;
                }
                if (item.Name == bracingView.ColBracingMaterial.SelectedItem.ToString())
                {
                    colBracingMaterial = item;
                }
                if (item.Name == bracingView.RofBracingMaterial.SelectedItem.ToString())
                {
                    roofBracingMaterial = item;
                }
                if (item.Name == framingView.WindResistantColMaterial.SelectedItem.ToString())
                {
                    windResistantColMaterial = item;
                }
            }
            //拿到边界条件
            //FilteredElementCollector bcFilter = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_BoundaryConditions);
            //IList<Element> bcList = bcFilter.ToElements();
            IList<FamilyInstance> allColList = new List<FamilyInstance>();
            List<FamilyInstance> windResistantColList = new List<FamilyInstance>();
            using (Transaction ts1 = new Transaction(doc, "FactoryCreation"))
            {
                
                ts1.Start();
                //解决“重复标记”的提示框
                FailureHandlingOptions options = ts1.GetFailureHandlingOptions();
                options.SetFailuresPreprocessor(new DuplicateMarkSwallower());
                ts1.SetFailureHandlingOptions(options);
                //创建柱顶标高及相应平面
                Level ColToplevel = Level.Create(doc, colHeight);
                FilteredElementCollector viewPlanFilter = new FilteredElementCollector(doc).OfClass(typeof(ViewFamilyType));
                foreach (ViewFamilyType viewFamilyType in viewPlanFilter)
                {
                    if (viewFamilyType.ViewFamily == ViewFamily.StructuralPlan)
                    {
                        ViewPlan.Create(doc, viewFamilyType.Id, ColToplevel.Id);
                    }
                }
                //生成柱

                if (!colType.IsActive)
                {
                    colType.Activate();
                }
                foreach (XYZ item in pColL)
                {
                    FamilyInstance col = doc.Create.NewFamilyInstance(item, colType, ColBtmlevel, Autodesk.Revit.DB.Structure.StructuralType.Column);
                    col.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM).Set(ColToplevel.Id);
                    col.StructuralMaterialId = colMaterial.Id;
                    col.get_Parameter(BuiltInParameter.DOOR_NUMBER).Set("刚架柱");
                    //col.get_Parameter(BuiltInParameter.SCHEDULE_TOP_LEVEL_OFFSET_PARAM).Set();
                    allColList.Add(col);
                }
                foreach (XYZ item in pColR)
                {
                    FamilyInstance col = doc.Create.NewFamilyInstance(item, colType, ColBtmlevel, Autodesk.Revit.DB.Structure.StructuralType.Column);
                    col.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM).Set(ColToplevel.Id);
                    col.StructuralMaterialId = colMaterial.Id;
                    col.get_Parameter(BuiltInParameter.DOOR_NUMBER).Set("刚架柱");
                    //col.get_Parameter(BuiltInParameter.SCHEDULE_TOP_LEVEL_OFFSET_PARAM).Set(colHeight/304.8);
                    allColList.Add(col);
                }
                //生成边跨抗风柱
                if (framingView.IsSetWindResistantCol.IsChecked == true)
                {

                    //int windResistantColNumber = Convert.ToInt32(dialog.WindResistantColNumber.Text);
                    string windResistantColSpaceInfo = framingView.WindResistantColSpace.Text;
                    List<double> windResistantColSpaceList = new List<double>();
                    windResistantColSpaceList = RvtCreationExtension.GetWindResistantSpace(windResistantColSpaceInfo);
                    if (!windResistantColType.IsActive)
                    {
                        windResistantColType.Activate();
                    }
                    double temp = 0;
                    for (int i = 0; i < windResistantColSpaceList.Count; i++)
                    {
                        temp += windResistantColSpaceList[i];

                        if (temp <= depth / 2)
                        {
                            //生成西侧抗风柱
                            FamilyInstance windResistantColWest = doc.Create.NewFamilyInstance(pColR[0] + new XYZ(0, temp, 0), windResistantColType, ColBtmlevel, Autodesk.Revit.DB.Structure.StructuralType.Column);
                            windResistantColWest.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM).Set(ColToplevel.Id);
                            windResistantColWest.get_Parameter(BuiltInParameter.SCHEDULE_TOP_LEVEL_OFFSET_PARAM).Set(roofHeight * temp / (depth / 2));
                            windResistantColWest.StructuralMaterialId = windResistantColMaterial.Id;
                            windResistantColWest.get_Parameter(BuiltInParameter.DOOR_NUMBER).Set("抗风柱");
                            allColList.Add(windResistantColWest);
                            windResistantColList.Add(windResistantColWest);

                            //生成东侧抗风柱
                            FamilyInstance windResistantColEast = doc.Create.NewFamilyInstance(pColR[bentNumber - 1] + new XYZ(0, temp, 0), windResistantColType, ColBtmlevel, Autodesk.Revit.DB.Structure.StructuralType.Column);
                            windResistantColEast.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM).Set(ColToplevel.Id);
                            windResistantColEast.get_Parameter(BuiltInParameter.SCHEDULE_TOP_LEVEL_OFFSET_PARAM).Set(roofHeight * temp / (depth / 2));
                            windResistantColEast.StructuralMaterialId = windResistantColMaterial.Id;
                            windResistantColEast.get_Parameter(BuiltInParameter.DOOR_NUMBER).Set("抗风柱");
                            allColList.Add(windResistantColEast);
                            windResistantColList.Add(windResistantColEast);


                        }
                        else
                        {
                            //生成西侧抗风柱
                            FamilyInstance windResistantColWest = doc.Create.NewFamilyInstance(pColR[0] + new XYZ(0, temp, 0), windResistantColType, ColBtmlevel, Autodesk.Revit.DB.Structure.StructuralType.Column);
                            windResistantColWest.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM).Set(ColToplevel.Id);
                            windResistantColWest.get_Parameter(BuiltInParameter.SCHEDULE_TOP_LEVEL_OFFSET_PARAM).Set(roofHeight * (depth - temp) / (depth / 2));
                            windResistantColWest.StructuralMaterialId = windResistantColMaterial.Id;
                            windResistantColWest.get_Parameter(BuiltInParameter.DOOR_NUMBER).Set("抗风柱");
                            allColList.Add(windResistantColWest);
                            windResistantColList.Add(windResistantColWest);

                            //生成东侧抗风柱
                            FamilyInstance windResistantColEast = doc.Create.NewFamilyInstance(pColR[bentNumber - 1] + new XYZ(0, temp, 0), windResistantColType, ColBtmlevel, Autodesk.Revit.DB.Structure.StructuralType.Column);
                            windResistantColEast.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM).Set(ColToplevel.Id);
                            windResistantColEast.get_Parameter(BuiltInParameter.SCHEDULE_TOP_LEVEL_OFFSET_PARAM).Set(roofHeight * (depth - temp) / (depth / 2));
                            windResistantColEast.StructuralMaterialId = windResistantColMaterial.Id;
                            windResistantColEast.get_Parameter(BuiltInParameter.DOOR_NUMBER).Set("抗风柱");
                            allColList.Add(windResistantColEast);
                            windResistantColList.Add(windResistantColEast);
                        }
                    }


                }
                //生成系杆
                if (!xiGanType.IsActive)
                {
                    xiGanType.Activate();
                }
                for (int i = 0; i < bentNumber - 1; i++)
                {
                    Curve curve1 = Line.CreateBound(pColL[i] + new XYZ(0, 0, colHeight), pColL[i + 1] + new XYZ(0, 0, colHeight));
                    FamilyInstance xiGan1 = doc.Create.NewFamilyInstance(curve1, xiGanType, ColToplevel, Autodesk.Revit.DB.Structure.StructuralType.Beam);
                    xiGan1.StructuralMaterialId = xiGanMaterial.Id;
                    xiGan1.get_Parameter(BuiltInParameter.DOOR_NUMBER).Set("系杆");
                    Curve curve2 = Line.CreateBound(pColR[i] + new XYZ(0, 0, colHeight), pColR[i + 1] + new XYZ(0, 0, colHeight));
                    FamilyInstance xiGan2 = doc.Create.NewFamilyInstance(curve2, xiGanType, ColToplevel, Autodesk.Revit.DB.Structure.StructuralType.Beam);
                    xiGan2.StructuralMaterialId = xiGanMaterial.Id;
                    xiGan2.get_Parameter(BuiltInParameter.DOOR_NUMBER).Set("系杆");
                    Curve curve3 = Line.CreateBound(pRoof[i], pRoof[i + 1]);
                    FamilyInstance xiGan3 = doc.Create.NewFamilyInstance(curve3, xiGanType, ColToplevel, Autodesk.Revit.DB.Structure.StructuralType.Beam);
                    xiGan3.StructuralMaterialId = xiGanMaterial.Id;
                    xiGan3.get_Parameter(BuiltInParameter.DOOR_NUMBER).Set("系杆");

                }

                //生成梁
                if (!beamType.IsActive)
                {
                    beamType.Activate();
                }
                for (int i = 0; i < bentNumber; i++)
                {
                    Curve curve1 = Line.CreateBound(pColL[i] + new XYZ(0, 0, colHeight), pRoof[i]);
                    FamilyInstance rf1 = doc.Create.NewFamilyInstance(curve1, beamType, ColToplevel, Autodesk.Revit.DB.Structure.StructuralType.Beam);
                    rf1.StructuralMaterialId = beamMaterial.Id;
                    rf1.get_Parameter(BuiltInParameter.DOOR_NUMBER).Set("刚架梁");

                    Curve curve2 = Line.CreateBound(pColR[i] + new XYZ(0, 0, colHeight), pRoof[i]);
                    FamilyInstance rf2 = doc.Create.NewFamilyInstance(curve2, beamType, ColToplevel, Autodesk.Revit.DB.Structure.StructuralType.Beam);
                    rf2.StructuralMaterialId = beamMaterial.Id;
                    rf2.get_Parameter(BuiltInParameter.DOOR_NUMBER).Set("刚架梁");

                }

                //以数量定义,生成檩条                                  
                double angle = Math.Atan(roofHeight / depth * 2);
                if (!purlinType.IsActive)
                {
                    purlinType.Activate();
                }
                if (purlinView.IsNumberDefine.IsChecked == true)
                {
                    //生成右侧檩条   
                    for (int i = 0; i < bentNumber - 1; i++)
                    {
                        for (int j = 0; j <= purlinNumberR - 1; j++)
                        {
                            XYZ purlinStart = pColTopR[i] + new XYZ(0, purlinCR * Math.Cos(angle), purlinCR * Math.Sin(angle))
                                + j / (purlinNumberR - 1) * (pRoof[i] - new XYZ(0, purlinRR * Math.Cos(angle), purlinRR * Math.Sin(angle))
                                - pColTopR[i] - new XYZ(0, purlinCR * Math.Cos(angle), purlinCR * Math.Sin(angle)));
                            XYZ purlinEnd = pColTopR[i + 1] + new XYZ(0, purlinCR * Math.Cos(angle), purlinCR * Math.Sin(angle))
                                + j / (purlinNumberR - 1) * (pRoof[i + 1] - new XYZ(0, purlinRR * Math.Cos(angle), purlinRR * Math.Sin(angle))
                                - pColTopR[i + 1] - new XYZ(0, purlinCR * Math.Cos(angle), purlinCR * Math.Sin(angle)));
                            Curve curve = Line.CreateBound(purlinStart, purlinEnd);
                            FamilyInstance purlin = doc.Create.NewFamilyInstance(curve, purlinType, ColToplevel, Autodesk.Revit.DB.Structure.StructuralType.Beam);
                            //设置檩条旋转角度
                            purlin.StructuralMaterialId = purlinMaterial.Id;
                            purlin.get_Parameter(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE).Set(-angle);
                            purlin.get_Parameter(BuiltInParameter.Z_JUSTIFICATION).Set(3);
                        }
                    }
                    //生成左侧檩条
                    for (int i = 0; i < bentNumber - 1; i++)
                    {
                        for (int j = 0; j <= purlinNumberL - 1; j++)
                        {
                            XYZ purlinStart = pColTopL[i] - new XYZ(0, purlinCL * Math.Cos(angle), -purlinCL * Math.Sin(angle))
                                + j / (purlinNumberL - 1) * (pRoof[i] + new XYZ(0, purlinRL * Math.Cos(angle), -purlinRL * Math.Sin(angle))
                                - pColTopL[i] + new XYZ(0, purlinCL * Math.Cos(angle), -purlinCL * Math.Sin(angle)));
                            XYZ purlinEnd = pColTopL[i + 1] - new XYZ(0, purlinCL * Math.Cos(angle), -purlinCL * Math.Sin(angle))
                                + j / (purlinNumberL - 1) * (pRoof[i + 1] + new XYZ(0, purlinRL * Math.Cos(angle), -purlinRL * Math.Sin(angle))
                                - pColTopL[i + 1] + new XYZ(0, purlinCL * Math.Cos(angle), -purlinCL * Math.Sin(angle)));
                            Curve curve = Line.CreateBound(purlinStart, purlinEnd);
                            FamilyInstance purlin = doc.Create.NewFamilyInstance(curve, purlinType, ColToplevel, Autodesk.Revit.DB.Structure.StructuralType.Beam);
                            //设置檩条旋转角度
                            purlin.StructuralMaterialId = purlinMaterial.Id;
                            purlin.get_Parameter(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE).Set(angle);
                            purlin.get_Parameter(BuiltInParameter.Z_JUSTIFICATION).Set(3);
                        }
                    }
                }
                //以间隔定义，生成檩条
                if (purlinView.IsSpaceDefine.IsChecked == true)
                {
                    //生成右侧檩条
                    for (int i = 0; i < bentNumber - 1; i++)
                    {
                        double initialValue = 0;
                        double roofLengthR = Math.Sqrt(depth * depth / 4 + roofHeight * roofHeight);
                        while (initialValue < roofLengthR - purlinCR - purlinRR)
                        {

                            XYZ purlinStart = pColTopR[i] + new XYZ(0, purlinCR * Math.Cos(angle), purlinCR * Math.Sin(angle))
                                + new XYZ(0, initialValue * Math.Cos(angle), initialValue * Math.Sin(angle));
                            XYZ purlinEnd = pColTopR[i + 1] + new XYZ(0, purlinCR * Math.Cos(angle), purlinCR * Math.Sin(angle))
                                + new XYZ(0, initialValue * Math.Cos(angle), initialValue * Math.Sin(angle));
                            Curve curve = Line.CreateBound(purlinStart, purlinEnd);
                            FamilyInstance purlin = doc.Create.NewFamilyInstance(curve, purlinType, ColToplevel, Autodesk.Revit.DB.Structure.StructuralType.Beam);
                            //设置檩条旋转角度
                            purlin.StructuralMaterialId = purlinMaterial.Id;
                            purlin.get_Parameter(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE).Set(-angle);
                            purlin.get_Parameter(BuiltInParameter.Z_JUSTIFICATION).Set(3);
                            initialValue += purlinSpaceR;

                        }
                    }
                    //生成左侧檩条
                    for (int i = 0; i < bentNumber - 1; i++)
                    {
                        double initialValue = 0;
                        double roofLengthL = Math.Sqrt(depth * depth / 4 + roofHeight * roofHeight);
                        while (initialValue < roofLengthL - purlinCL - purlinRL)
                        {
                            XYZ purlinStart = pColTopL[i] + new XYZ(0, -purlinCL * Math.Cos(angle), purlinCL * Math.Sin(angle))
                                + new XYZ(0, -initialValue * Math.Cos(angle), initialValue * Math.Sin(angle));
                            XYZ purlinEnd = pColTopL[i + 1] + new XYZ(0, -purlinCL * Math.Cos(angle), purlinCL * Math.Sin(angle))
                                + new XYZ(0, -initialValue * Math.Cos(angle), initialValue * Math.Sin(angle));
                            Curve curve = Line.CreateBound(purlinStart, purlinEnd);
                            FamilyInstance purlin = doc.Create.NewFamilyInstance(curve, purlinType, ColToplevel, Autodesk.Revit.DB.Structure.StructuralType.Beam);
                            //设置檩条旋转角度
                            purlin.StructuralMaterialId = purlinMaterial.Id;
                            purlin.get_Parameter(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE).Set(angle);
                            purlin.get_Parameter(BuiltInParameter.Z_JUSTIFICATION).Set(3);
                            initialValue += purlinSpaceL;
                        }
                    }
                }
                //生成柱间支撑
                if (!colBracingType.IsActive)
                {
                    colBracingType.Activate();
                }
                for (int i = 0; i < bentNumber - 1; i++)
                {
                    switch ((bracingView.ColBracingList.Items[i] as ColBracingInfo).SelectedBracingTypeR)
                    {
                        case "十字形支撑":
                            Curve crossCurve1 = Line.CreateBound(pColR[i], pColTopR[i + 1]);
                            FamilyInstance crossBracingR1 = doc.Create.NewFamilyInstance(crossCurve1, colBracingType, ColToplevel, Autodesk.Revit.DB.Structure.StructuralType.Beam);
                            crossBracingR1.StructuralMaterialId = colBracingMaterial.Id;
                            crossBracingR1.get_Parameter(BuiltInParameter.DOOR_NUMBER).Set("支撑");

                            Curve crossCurve2 = Line.CreateBound(pColTopR[i], pColR[i + 1]);
                            FamilyInstance crossBracingR2 = doc.Create.NewFamilyInstance(crossCurve2, colBracingType, ColToplevel, Autodesk.Revit.DB.Structure.StructuralType.Beam);
                            crossBracingR2.StructuralMaterialId = colBracingMaterial.Id;
                            crossBracingR2.get_Parameter(BuiltInParameter.DOOR_NUMBER).Set("支撑");

                            break;
                        case "人字形支撑":
                            Curve herringboneCurve1 = Line.CreateBound(pColR[i], (pColTopR[i] + pColTopR[i + 1]) / 2);
                            FamilyInstance herringboneBracingR1 = doc.Create.NewFamilyInstance(herringboneCurve1, colBracingType, ColToplevel, Autodesk.Revit.DB.Structure.StructuralType.Beam);
                            herringboneBracingR1.StructuralMaterialId = colBracingMaterial.Id;
                            herringboneBracingR1.get_Parameter(BuiltInParameter.DOOR_NUMBER).Set("支撑");

                            Curve herringboneCurve2 = Line.CreateBound((pColTopR[i] + pColTopR[i + 1]) / 2, pColR[i + 1]);
                            FamilyInstance herringboneBracingR2 = doc.Create.NewFamilyInstance(herringboneCurve2, colBracingType, ColToplevel, Autodesk.Revit.DB.Structure.StructuralType.Beam);
                            herringboneBracingR2.StructuralMaterialId = colBracingMaterial.Id;
                            herringboneBracingR2.get_Parameter(BuiltInParameter.DOOR_NUMBER).Set("支撑");

                            break;
                        default:
                            break;
                    }
                    switch ((bracingView.ColBracingList.Items[i] as ColBracingInfo).SelectedBracingTypeL)
                    {
                        case "十字形支撑":
                            Curve crossCurve1 = Line.CreateBound(pColL[i], pColTopL[i + 1]);
                            FamilyInstance crossBracingL1 = doc.Create.NewFamilyInstance(crossCurve1, colBracingType, ColToplevel, Autodesk.Revit.DB.Structure.StructuralType.Beam);
                            crossBracingL1.StructuralMaterialId = colBracingMaterial.Id;
                            crossBracingL1.get_Parameter(BuiltInParameter.DOOR_NUMBER).Set("支撑");

                            Curve crossCurve2 = Line.CreateBound(pColTopL[i], pColL[i + 1]);
                            FamilyInstance crossBracingL2 = doc.Create.NewFamilyInstance(crossCurve2, colBracingType, ColToplevel, Autodesk.Revit.DB.Structure.StructuralType.Beam);
                            crossBracingL2.StructuralMaterialId = colBracingMaterial.Id;
                            crossBracingL2.get_Parameter(BuiltInParameter.DOOR_NUMBER).Set("支撑");

                            break;
                        case "人字形支撑":
                            Curve herringboneCurve1 = Line.CreateBound(pColL[i], (pColTopL[i] + pColTopL[i + 1]) / 2);
                            FamilyInstance herringboneBracingL1 = doc.Create.NewFamilyInstance(herringboneCurve1, colBracingType, ColToplevel, Autodesk.Revit.DB.Structure.StructuralType.Beam);
                            herringboneBracingL1.StructuralMaterialId = colBracingMaterial.Id;
                            herringboneBracingL1.get_Parameter(BuiltInParameter.DOOR_NUMBER).Set("支撑");
                            Curve herringboneCurve2 = Line.CreateBound((pColTopL[i] + pColTopL[i + 1]) / 2, pColL[i + 1]);
                            FamilyInstance herringboneBracingL2 = doc.Create.NewFamilyInstance(herringboneCurve2, colBracingType, ColToplevel, Autodesk.Revit.DB.Structure.StructuralType.Beam);
                            herringboneBracingL2.StructuralMaterialId = colBracingMaterial.Id;
                            herringboneBracingL2.get_Parameter(BuiltInParameter.DOOR_NUMBER).Set("支撑");

                            break;
                        default:
                            break;
                    }
                }
                //生成屋面支撑
                if (!roofBracingType.IsActive)
                {
                    roofBracingType.Activate();
                }
                for (int i = 0; i < bentNumber - 1; i++)
                {
                    switch ((bracingView.RofBracingList.Items[i] as RofBracingInfo).SelectedBracingTypeR)
                    {
                        case "十字形支撑":
                            for (int j = 0; j < bracingNumber; j++)
                            {
                                XYZ bracingStart1 = pColTopR[i] + j / bracingNumber * (pRoof[i] - pColTopR[i]);
                                XYZ bracingEnd1 = pColTopR[i + 1] + (j + 1) / bracingNumber * (pRoof[i + 1] - pColTopR[i + 1]);
                                Curve curve1 = Line.CreateBound(bracingStart1, bracingEnd1);
                                FamilyInstance bracing1 = doc.Create.NewFamilyInstance(curve1, roofBracingType, ColToplevel, Autodesk.Revit.DB.Structure.StructuralType.Beam);
                                bracing1.StructuralMaterialId = roofBracingMaterial.Id;
                                bracing1.get_Parameter(BuiltInParameter.DOOR_NUMBER).Set("支撑");

                                XYZ bracingStart2 = pColTopR[i + 1] + j / bracingNumber * (pRoof[i + 1] - pColTopR[i + 1]);
                                XYZ bracingEnd2 = pColTopR[i] + (j + 1) / bracingNumber * (pRoof[i] - pColTopR[i]);
                                Curve curve2 = Line.CreateBound(bracingStart2, bracingEnd2);
                                FamilyInstance bracing2 = doc.Create.NewFamilyInstance(curve2, roofBracingType, ColToplevel, Autodesk.Revit.DB.Structure.StructuralType.Beam);
                                bracing2.StructuralMaterialId = roofBracingMaterial.Id;
                                bracing2.get_Parameter(BuiltInParameter.DOOR_NUMBER).Set("支撑");

                            }
                            for (int j = 0; j < bracingNumber - 1; j++)
                            {
                                XYZ xiGanBetweenBracingStart = pColTopR[i] + (j + 1) / bracingNumber * (pRoof[i] - pColTopR[i]);
                                XYZ xiGanBetweenBracingEnd = pColTopR[i + 1] + (j + 1) / bracingNumber * (pRoof[i + 1] - pColTopR[i + 1]);
                                Curve curve = Line.CreateBound(xiGanBetweenBracingStart, xiGanBetweenBracingEnd);
                                FamilyInstance xiGan = doc.Create.NewFamilyInstance(curve, roofBracingType, ColToplevel, Autodesk.Revit.DB.Structure.StructuralType.Beam);
                                xiGan.StructuralMaterialId = xiGanMaterial.Id;
                                xiGan.get_Parameter(BuiltInParameter.DOOR_NUMBER).Set("系杆");

                            }
                            break;
                        default:
                            break;
                    }
                    switch ((bracingView.RofBracingList.Items[i] as RofBracingInfo).SelectedBracingTypeL)
                    {
                        case "十字形支撑":
                            for (int j = 0; j < bracingNumber; j++)
                            {
                                XYZ bracingStart1 = pColTopL[i] + j / bracingNumber * (pRoof[i] - pColTopL[i]);
                                XYZ bracingEnd1 = pColTopL[i + 1] + (j + 1) / bracingNumber * (pRoof[i + 1] - pColTopL[i + 1]);
                                Curve curve1 = Line.CreateBound(bracingStart1, bracingEnd1);
                                FamilyInstance bracing1 = doc.Create.NewFamilyInstance(curve1, roofBracingType, ColToplevel, Autodesk.Revit.DB.Structure.StructuralType.Beam);
                                bracing1.StructuralMaterialId = roofBracingMaterial.Id;
                                bracing1.get_Parameter(BuiltInParameter.DOOR_NUMBER).Set("支撑");

                                XYZ bracingStart2 = pColTopL[i + 1] + j / bracingNumber * (pRoof[i + 1] - pColTopL[i + 1]);
                                XYZ bracingEnd2 = pColTopL[i] + (j + 1) / bracingNumber * (pRoof[i] - pColTopL[i]);
                                Curve curve2 = Line.CreateBound(bracingStart2, bracingEnd2);
                                FamilyInstance bracing2 = doc.Create.NewFamilyInstance(curve2, roofBracingType, ColToplevel, Autodesk.Revit.DB.Structure.StructuralType.Beam);
                                bracing2.StructuralMaterialId = roofBracingMaterial.Id;
                                bracing2.get_Parameter(BuiltInParameter.DOOR_NUMBER).Set("支撑");

                            }
                            for (int j = 0; j < bracingNumber - 1; j++)
                            {
                                XYZ xiGanBetweenBracingStart = pColTopL[i] + (j + 1) / bracingNumber * (pRoof[i] - pColTopL[i]);
                                XYZ xiGanBetweenBracingEnd = pColTopL[i + 1] + (j + 1) / bracingNumber * (pRoof[i + 1] - pColTopL[i + 1]);
                                Curve curve = Line.CreateBound(xiGanBetweenBracingStart, xiGanBetweenBracingEnd);
                                FamilyInstance xiGan = doc.Create.NewFamilyInstance(curve, roofBracingType, ColToplevel, Autodesk.Revit.DB.Structure.StructuralType.Beam);
                                xiGan.StructuralMaterialId = xiGanMaterial.Id;
                                xiGan.get_Parameter(BuiltInParameter.DOOR_NUMBER).Set("系杆");

                            }
                            break;
                        default:
                            break;
                    }
                }
                ts1.Commit();
                //设置柱端连接方式
                using (Transaction ts2 = new Transaction(doc, "AdvancedSetting"))
                {
                    ts2.Start();
                    for (int i = 0; i < windResistantColList.Count; i++)
                    {
                        RvtCreationExtension.RotateByCenter(doc, windResistantColList[i], 90);
                    }
                    foreach (FamilyInstance col in allColList)
                    {
                        if (framingView.Connection.SelectedItem.ToString() == "固定")
                        {
                            Reference reference = RvtCreationExtension.GetAMColReference(col);
                            BoundaryConditions boundaryConditions = doc.Create.NewPointBoundaryConditions(reference, TranslationRotationValue.Fixed, 0, TranslationRotationValue.Fixed, 0
                                , TranslationRotationValue.Fixed, 0, TranslationRotationValue.Fixed, 0, TranslationRotationValue.Fixed, 0, TranslationRotationValue.Fixed, 0);
                        }
                        if (framingView.Connection.SelectedItem.ToString() == "铰接")
                        {
                            Reference reference = RvtCreationExtension.GetAMColReference(col);
                            BoundaryConditions boundaryConditions = doc.Create.NewPointBoundaryConditions(reference, TranslationRotationValue.Fixed, 0, TranslationRotationValue.Fixed, 0
                                , TranslationRotationValue.Fixed, 0, TranslationRotationValue.Release, 0, TranslationRotationValue.Release, 0, TranslationRotationValue.Release, 0);
                        }
                    }
                    ts2.Commit();
                    TextDialog textDialog = new TextDialog("创建成功");                   
                    textDialog.ShowDialog();

                    for (int i = 0; i < views.Items.Count; i++)
                    {
                        if (views.Items[i].ToString() == "{三维}")
                        {
                            views.SelectedIndex = i;
                            break;
                        }
                    }
                }
            }
        }


        private void Views_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                string viewName = views.SelectedItem.ToString();
                FilteredElementCollector viewFilter = new FilteredElementCollector(doc).OfClass(typeof(View));
                View selectedView = viewFilter.Cast<View>().FirstOrDefault(t => t.Name == viewName);
                if (selectedView == null) return;
                PreviewControl previewControl = Grid.Children.Cast<PreviewControl>().FirstOrDefault();
                if (previewControl != null)
                {
                    previewControl.Dispose();
                }
                Grid.Children.Add(new PreviewControl(doc, selectedView.Id));
            }
            catch 
            {
               
            }
        }

        //}
        //catch (Exception e)
        //{
        //    TaskDialog expTaskDialog = new TaskDialog("单层厂房Demo");
        //    expTaskDialog.MainContent = "无法生成模型，请重新输入参数";
        //    expTaskDialog.MainInstruction = "提示";
        //    TaskDialogResult expTaskDialogResult = expTaskDialog.Show();

        //}
    }
}
