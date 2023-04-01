using Autodesk.Revit.DB.Structure;
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
using Test.Extensions;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using HandyControl.Tools.Extension;
using System.Data.Common;
using Autodesk.Revit.DB.Lighting;
using System.Data;
using Autodesk.Revit.UI;
using System.Windows.Media.Animation;
using UIFramework;

namespace Test.UserControls
{
    /// <summary>
    /// LoadSetView.xaml 的交互逻辑
    /// </summary>
    public partial class LoadSetView : UserControl
    {
        private static LoadSetView _instance;
        public Document doc { get; set; }
        private LoadSetView()
        {
            InitializeComponent();
        }
        public void Initial(Document doc)
        {
            this.doc = doc;
            //屋面恒荷载
            roofDead.Text = "0.3";
            //墙面恒荷载
            wallDead.Text = "0";
            //屋面活荷载
            roofLive.Text = "0.5";
            //地面粗糙度
            GroundRoughness.Items.Add("A");
            GroundRoughness.Items.Add("B");
            GroundRoughness.Items.Add("C");
            GroundRoughness.Items.Add("D");
            GroundRoughness.SelectedIndex = 0;
            //房屋类型
            ClosedForm.Items.Add("封闭式");
            ClosedForm.Items.Add("部分封闭式");
            ClosedForm.Items.Add("敞开式");
            ClosedForm.SelectedIndex = 0;
            //基本风压
            BasicWindPressure.Text = "0.5";
            //调整系数
            AdjustFactor.Text = "1.1";
            //抗震设防烈度
            SeismicIntensity.Items.Add("6(0.05g)");
            SeismicIntensity.Items.Add("7(0.10g)");
            SeismicIntensity.Items.Add("7(0.15g)");
            SeismicIntensity.Items.Add("8(0.20g)");
            SeismicIntensity.Items.Add("8(0.30g)");
            SeismicIntensity.Items.Add("9(0.40g)");
            SeismicIntensity.SelectedIndex = 1;
            //地震影响系数最大值
            MaximumValueOfEarthquakeInfluenceCoefficient.Text = "0.08";
            //场地类别
            SiteCategory.Items.Add("Ⅰ0");
            SiteCategory.Items.Add("Ⅰ1");
            SiteCategory.Items.Add("Ⅱ");
            SiteCategory.Items.Add("Ⅲ");
            SiteCategory.Items.Add("Ⅳ");
            SiteCategory.SelectedIndex = 3;
            //设计地震分组
            ClassificationOfDesignEarthquake.Items.Add("第一组");
            ClassificationOfDesignEarthquake.Items.Add("第二组");
            ClassificationOfDesignEarthquake.Items.Add("第三组");
            ClassificationOfDesignEarthquake.SelectedIndex = 1;
            //特征周期
            CharacteristicPeriod.Text = "0.45";
            //周期折减系数
            PeriodTimeReductionFactor.Text = "1.00";
            //阻尼比
            DampingRatio.Text= "0.05";
            //振型组合方法
            ModesCombination.Items.Add("CQC");
            ModesCombination.Items.Add("SRSS");
            ModesCombination.SelectedIndex = 0;

        }
        public static LoadSetView GetInstance()
        {
            if (_instance == null)
            {
                _instance = new LoadSetView();
            }
            return _instance;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            double rfDead = Convert.ToDouble(roofDead.Text);
            double rfLive = Convert.ToDouble(roofLive.Text);
            double walDead = Convert.ToDouble(wallDead.Text);
            string groundRoughness = GroundRoughness.Text;
            double basicWindPressure = Convert.ToDouble(BasicWindPressure.Text);
            double adjustFactor = Convert.ToDouble(AdjustFactor.Text);
            StringBuilder str = new StringBuilder();
            FilteredElementCollector columnCol = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_StructuralColumns).OfClass(typeof(FamilyInstance));
            List<FamilyInstance> allColumns = columnCol.Cast<FamilyInstance>().ToList();
            List<ElementId> allColumnsIds = new List<ElementId>();
            List<FamilyInstance> columns = new List<FamilyInstance>();
            List<ElementId> columnsIds = new List<ElementId>(); 
            foreach (FamilyInstance item in allColumns)
            {
                allColumnsIds.Add(item.Id);
                string token = item.get_Parameter(BuiltInParameter.DOOR_NUMBER).AsString();
                if (token == "刚架柱")
                {
                    columns.Add(item);
                    columnsIds.Add(item.Id);
                }
            }
             
            FilteredElementCollector beamCol = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_StructuralFraming).OfClass(typeof(FamilyInstance));
            IList<Element> allBeams = beamCol.ToElements();
            List<FamilyInstance> beams = new List<FamilyInstance>();
            List<ElementId> beamsIds = new List<ElementId>();
            foreach (FamilyInstance item in allBeams)
            {
                string token = item.get_Parameter(BuiltInParameter.DOOR_NUMBER).AsString();
                if (token == "刚架梁")
                {
                    beams.Add(item);
                    beamsIds.Add(item.Id);
                }
            }
            #region 工况设置
            FilteredElementCollector lineLoadTypeCol = new FilteredElementCollector(doc).OfClass(typeof(LineLoadType));
            LineLoadType lineLoadType = lineLoadTypeCol.Cast<LineLoadType>().First(t => t.Name == "线荷载 1");

            FilteredElementCollector loadNatureCol = new FilteredElementCollector(doc).OfClass(typeof(LoadNature));
            LoadNature deadLoadNature = loadNatureCol.Cast<LoadNature>().First(t => t.Name == "恒");
            LoadNature liveLoadNature = loadNatureCol.Cast<LoadNature>().First(t => t.Name == "活");
            LoadNature windLoadNature = loadNatureCol.Cast<LoadNature>().First(t => t.Name == "风");
            LoadNature earthquakeLoadNature = loadNatureCol.Cast<LoadNature>().First(t => t.Name == "地震");


            FilteredElementCollector viewCol = new FilteredElementCollector(doc);
            View3D view3D = viewCol.OfClass(typeof(View3D)).Cast<View3D>().First(t => t.Name == "{三维}");

            LoadCase deadLoadCase = null;//Dead
            LoadCase liveLoadCase = null;//Live
            //后缀0表示（+i）内压为压力，1表示（-i）内压为吸力
            LoadCase windYPlusLoadCase_0 = null; //WY+(+i)
            LoadCase windYPlusLoadCase_1 = null; //WY+(-i)
            LoadCase windYMinusLoadCase_0 = null;//WY-(+i)
            LoadCase windYMinusLoadCase_1 = null;//WY-(-i)
            LoadCase windXPlusLoadCase_0 = null;//WX+(+i)
            LoadCase windXPlusLoadCase_1 = null; //WX+(-i)
            LoadCase windXMinusLoadCase_0 = null;//WX-(+i)
            LoadCase windXMinusLoadCase_1 = null;//WX-(-i)

            LoadCase EarthquakeX = null;
            LoadCase EarthquakeY = null;
            FilteredElementCollector loadCaseFilter = new FilteredElementCollector(doc).OfClass(typeof(LoadCase));
            List<LoadCase> loadCases= loadCaseFilter.Cast<LoadCase>().ToList();
            for (int i = 0; i < loadCases.Count; i++)
            {
                if (loadCases[i].Name == "Dead")
                {
                    deadLoadCase = loadCases[i];
                }
                if (loadCases[i].Name == "Live")
                {
                    liveLoadCase = loadCases[i];
                }
                if (loadCases[i].Name == "WY+(+i)")
                {
                    windYPlusLoadCase_0 = loadCases[i];
                }
                if (loadCases[i].Name == "WY+(-i)")
                {
                    windYPlusLoadCase_1 = loadCases[i];
                }
                if (loadCases[i].Name == "WY-(+i)")
                {
                    windYMinusLoadCase_0 = loadCases[i];
                }
                if (loadCases[i].Name == "WY-(-i)")
                {
                    windYMinusLoadCase_1 = loadCases[i];
                }
                if (loadCases[i].Name == "WX+(+i)")
                {
                    windXPlusLoadCase_0 = loadCases[i];
                }
                if (loadCases[i].Name == "WX+(-i)")
                {
                    windXPlusLoadCase_1 = loadCases[i];
                }
                if (loadCases[i].Name == "WX-(+i)")
                {
                    windXMinusLoadCase_0 = loadCases[i];
                }
                if (loadCases[i].Name == "WX-(-i)")
                {
                    windXMinusLoadCase_1 = loadCases[i];
                }
                if (loadCases[i].Name == "EX")
                {
                   EarthquakeX = loadCases[i];
                }
                if (loadCases[i].Name == "EY")
                {
                    EarthquakeY = loadCases[i];
                }
            }


            using (Transaction ts = new Transaction(doc, "工况设置"))
            {
                ts.Start();
                if (deadLoadCase == null)
                {
                    deadLoadCase = LoadCase.Create(doc, "Dead", deadLoadNature.Id, LoadCaseCategory.Dead);
                }
                if (liveLoadCase == null)
                {
                    liveLoadCase = LoadCase.Create(doc, "Live", liveLoadNature.Id, LoadCaseCategory.Live);
                }
                if (windYPlusLoadCase_0 == null)
                {
                    windYPlusLoadCase_0 = LoadCase.Create(doc, "WY+(+i)", windLoadNature.Id, LoadCaseCategory.Wind);
                }
                if (windYPlusLoadCase_1 == null)
                {
                    windYPlusLoadCase_1 = LoadCase.Create(doc, "WY+(-i)", windLoadNature.Id, LoadCaseCategory.Wind);
                }
                if (windYMinusLoadCase_0 == null)
                {
                    windYMinusLoadCase_0 = LoadCase.Create(doc, "WY-(+i)", windLoadNature.Id, LoadCaseCategory.Wind);
                }
                if (windYMinusLoadCase_1 == null)
                {
                    windYMinusLoadCase_1 = LoadCase.Create(doc, "WY-(-i)", windLoadNature.Id, LoadCaseCategory.Wind);
                }
                if (windXPlusLoadCase_0 == null)
                {
                    windXPlusLoadCase_0 = LoadCase.Create(doc, "WX+(+i)", windLoadNature.Id, LoadCaseCategory.Wind);
                }
                if (windXPlusLoadCase_1 == null)
                {
                    windXPlusLoadCase_1 = LoadCase.Create(doc, "WX+(-i)", windLoadNature.Id, LoadCaseCategory.Wind);
                }
                if (windXMinusLoadCase_0 == null)
                {
                    windXMinusLoadCase_0 = LoadCase.Create(doc, "WX-(+i)", windLoadNature.Id, LoadCaseCategory.Wind);
                }
                if (windXMinusLoadCase_1 == null)
                {
                    windXMinusLoadCase_1 = LoadCase.Create(doc, "WX-(-i)", windLoadNature.Id, LoadCaseCategory.Wind);
                }
                if (EarthquakeX == null)
                {
                    EarthquakeX = LoadCase.Create(doc, "EX", earthquakeLoadNature.Id, LoadCaseCategory.Seismic);
                }
                if (EarthquakeY == null)
                {
                    EarthquakeY = LoadCase.Create(doc, "EY", earthquakeLoadNature.Id, LoadCaseCategory.Seismic);
                }
                ts.Commit();
            }
            #endregion  




            #region 施加恒/活荷载
            using (Transaction ts = new Transaction(doc, "施加恒/活荷载"))
            {
                ts.Start();
                //施加屋面恒荷载
                if (rfDead != 0)
                {
                    for (int i = 0; i < beams.Count; i++)
                    {
                        BoundingBoxXYZ box = beams[i].get_BoundingBox(view3D);
                        XYZ center = box.Min.Add(box.Max).Multiply(0.5);
                        ReferenceIntersector referenceIntersector = new ReferenceIntersector(beamsIds, FindReferenceTarget.Element, view3D);
                        //找X正向的最近梁并计算距离
                        double lengthR = 0;
                        ReferenceWithContext referenceWithContextR = referenceIntersector.FindNearest(center, XYZ.BasisX);
                        if (referenceWithContextR != null)
                        {
                            FamilyInstance other = doc.GetElement(referenceWithContextR.GetReference()) as FamilyInstance;
                            lengthR = UnitExtension.ConvertToMillimeters((beams[i].Location as LocationCurve).Curve.Distance((other.Location as LocationCurve).Curve.GetEndPoint(0)));
                        }
                        //找X负向的最近梁并计算距离
                        double lengthL = 0;
                        ReferenceWithContext referenceWithContextL = referenceIntersector.FindNearest(center, -XYZ.BasisX);
                        if (referenceWithContextL != null)
                        {

                            FamilyInstance other = doc.GetElement(referenceWithContextL.GetReference()) as FamilyInstance;
                            lengthL = UnitExtension.ConvertToMillimeters((beams[i].Location as LocationCurve).Curve.Distance((other.Location as LocationCurve).Curve.GetEndPoint(0)));
                        }
                        double computeLength = (lengthL + lengthR) / 2;

                        //屋面恒荷载传至梁
                        XYZ start = ((beams[i].Location as LocationCurve).Curve as Line).Origin;
                        double length = ((beams[i].Location as LocationCurve).Curve as Line).Length;
                        XYZ direction = ((beams[i].Location as LocationCurve).Curve as Line).Direction;
                        XYZ end = new XYZ(start.X + length * direction.X / Math.Sqrt((direction.X * direction.X + direction.Y * direction.Y + direction.Z * direction.Z)),
                           start.Y + length * direction.Y / Math.Sqrt((direction.X * direction.X + direction.Y * direction.Y + direction.Z * direction.Z)),
                           start.Z + length * direction.Z / Math.Sqrt((direction.X * direction.X + direction.Y * direction.Y + direction.Z * direction.Z)));
                        LineLoad rfDeadLineLoad = LineLoad.Create(doc, start, end, XYZ.BasisZ, XYZ.Zero, lineLoadType, null);
                                                        
                        //更改荷载大小
                        rfDeadLineLoad.get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set(-computeLength * rfDead);

                        //赋予荷载工况
                        rfDeadLineLoad.get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(deadLoadCase.Id);
                        //附加说明
                        rfDeadLineLoad.get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + beams[i].Id.ToString());
                        rfDeadLineLoad.get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    }   
                }

                //施加屋面活荷载
                if (rfLive != 0)
                {
                    for (int i = 0; i < beams.Count; i++)
                    {   
                        BoundingBoxXYZ box = beams[i].get_BoundingBox(view3D);
                        XYZ center = box.Min.Add(box.Max).Multiply(0.5);
                        ReferenceIntersector referenceIntersector = new ReferenceIntersector(beamsIds, FindReferenceTarget.Element, view3D);
                        //找X正向的最近梁并计算距离
                        double lengthR = 0;
                        ReferenceWithContext referenceWithContextR = referenceIntersector.FindNearest(center, XYZ.BasisX);
                        if (referenceWithContextR != null)
                        {
                            FamilyInstance other = doc.GetElement(referenceWithContextR.GetReference()) as FamilyInstance;
                            lengthR = UnitExtension.ConvertToMillimeters((beams[i].Location as LocationCurve).Curve.Distance((other.Location as LocationCurve).Curve.GetEndPoint(0)));
                        }
                        //找X负向的最近梁并计算距离
                        double lengthL = 0;
                        ReferenceWithContext referenceWithContextL = referenceIntersector.FindNearest(center, -XYZ.BasisX);
                        if (referenceWithContextL != null)
                        {
                            FamilyInstance other = doc.GetElement(referenceWithContextL.GetReference()) as FamilyInstance;
                            lengthL = UnitExtension.ConvertToMillimeters((beams[i].Location as LocationCurve).Curve.Distance((other.Location as LocationCurve).Curve.GetEndPoint(0)));
                        }
                        double computeLength = (lengthL + lengthR) / 2;
                        //屋面活荷载传至梁
                        XYZ start = ((beams[i].Location as LocationCurve).Curve as Line).Origin;
                        double length = ((beams[i].Location as LocationCurve).Curve as Line).Length;
                        XYZ direction = ((beams[i].Location as LocationCurve).Curve as Line).Direction;
                        XYZ end = new XYZ(start.X + length * direction.X / Math.Sqrt((direction.X * direction.X + direction.Y * direction.Y + direction.Z * direction.Z)),
                           start.Y + length * direction.Y / Math.Sqrt((direction.X * direction.X + direction.Y * direction.Y + direction.Z * direction.Z)),
                           start.Z + length * direction.Z / Math.Sqrt((direction.X * direction.X + direction.Y * direction.Y + direction.Z * direction.Z)));
                        LineLoad rfLiveLineLoad = LineLoad.Create(doc, start, end, XYZ.BasisZ, XYZ.Zero, lineLoadType, null);

                        //更改荷载大小
                        rfLiveLineLoad.get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set(-computeLength * rfLive);

                        //赋予荷载工况
                        rfLiveLineLoad.get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(liveLoadCase.Id);
                        //附加说明
                        rfLiveLineLoad.get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + beams[i].Id.ToString());
                        rfLiveLineLoad.get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");

                    }
                }
                if (walDead != 0)
                {
                    //施加墙面恒荷载
                    for (int i = 0; i < columns.Count; i++)
                    {

                        BoundingBoxXYZ box = columns[i].get_BoundingBox(view3D);
                        XYZ center = box.Min.Add(box.Max).Multiply(0.5);
                        ReferenceIntersector referenceIntersector = new ReferenceIntersector(columnsIds, FindReferenceTarget.Element, view3D);
                        //找X正向的最近柱并计算距离
                        double lengthR = 0;
                        ReferenceWithContext referenceWithContextR = referenceIntersector.FindNearest(center, XYZ.BasisX);
                        if (referenceWithContextR != null)
                        {
                            FamilyInstance other = doc.GetElement(referenceWithContextR.GetReference()) as FamilyInstance;
                            double LengthR= UnitExtension.ConvertToMillimeters((columns[i].Location as LocationPoint).Point.DistanceTo((other.Location as LocationPoint).Point));
                        }
                        //找X负向的最近柱并计算距离
                        double lengthL = 0;
                        ReferenceWithContext referenceWithContextL = referenceIntersector.FindNearest(center, -XYZ.BasisX);
                        if (referenceWithContextL != null)
                        {
                            FamilyInstance other = doc.GetElement(referenceWithContextL.GetReference()) as FamilyInstance;
                            double LengthL = UnitExtension.ConvertToMillimeters((columns[i].Location as LocationPoint).Point.DistanceTo((other.Location as LocationPoint).Point));
                        }
                        double computeLength = (lengthL + lengthR) / 2;
                        //墙面恒荷载传至柱
                        XYZ start = (columns[i].Location as LocationPoint).Point;
                        double height = (doc.GetElement(columns[i].get_Parameter(BuiltInParameter.SCHEDULE_TOP_LEVEL_PARAM).AsElementId()) as Level).Elevation;
                        XYZ end = (columns[i].Location as LocationPoint).Point + new XYZ(0, 0, height);
                        LineLoad wallDeadLineload = LineLoad.Create(doc, start, end, XYZ.BasisZ, XYZ.Zero, lineLoadType, null);

                        //更改荷载大小
                        wallDeadLineload.get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set(-computeLength * walDead);

                        //赋予荷载工况 
                        wallDeadLineload.get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(deadLoadCase.Id);
                        //附加说明
                        wallDeadLineload.get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + columns[i].Id.ToString());
                        wallDeadLineload.get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");


                    }

                }
                ts.Commit();
            }
            #endregion

            #region 施加风荷载



            #region 主刚架横向风荷载系数
            //第一个后缀h表示水平风荷载 L表示纵向风荷载   第二个后缀0表示（+i） 即内压为压力  1表示（-i）即内压为吸力
            //factor1E_0：1E分区在（+i）工况下的风荷载系数
            double factor1E_H_0 = 0;
            double factor1E_H_1 = 0;
            double factor2E_H_0 = 0;
            double factor2E_H_1 = 0;
            double factor3E_H_0 = 0;
            double factor3E_H_1 = 0;
            double factor4E_H_0 = 0;
            double factor4E_H_1 = 0;
            double factor1_H_0 = 0;
            double factor1_H_1 = 0;
            double factor2_H_0 = 0;
            double factor2_H_1 = 0;
            double factor3_H_0 = 0;
            double factor3_H_1 = 0;
            double factor4_H_0 = 0;
            double factor4_H_1 = 0;
            double factor5And6_H_0 = 0;
            double factor5And6_H_1 = 0;
            //计算屋面坡度角
            double angle = Math.Abs(Math.Atan(beams.FirstOrDefault().HandOrientation.Z / beams.FirstOrDefault().HandOrientation.Y)) * 180 / Math.PI;
            if (ClosedForm.Text == "封闭式")
            {
                string sql = "select * from ClosedHorizentalWindFactor";
                DBHelper.PrepareSql(sql);
                DataTable dt = DBHelper.ExecQuery();
                int count = 0;
                for (int i = 0; i < dt.Rows.Count; i += 2)
                {
                    if (angle <= Convert.ToDouble(dt.Rows[i]["Angle"]))
                    {
                        count = i;
                        break;
                    }
                }
                if (count == 0)
                {
                    factor1E_H_0 = Convert.ToDouble(dt.Rows[0]["Factor1E"]);
                    factor1E_H_1 = Convert.ToDouble(dt.Rows[1]["Factor1E"]);
                    factor2E_H_0 = Convert.ToDouble(dt.Rows[0]["Factor2E"]);
                    factor2E_H_1 = Convert.ToDouble(dt.Rows[1]["Factor2E"]);
                    factor3E_H_0 = Convert.ToDouble(dt.Rows[0]["Factor3E"]);
                    factor3E_H_1 = Convert.ToDouble(dt.Rows[1]["Factor3E"]);
                    factor4E_H_0 = Convert.ToDouble(dt.Rows[0]["Factor4E"]);
                    factor4E_H_1 = Convert.ToDouble(dt.Rows[1]["Factor4E"]);
                    factor1_H_0 = Convert.ToDouble(dt.Rows[0]["Factor1"]);
                    factor1_H_1 = Convert.ToDouble(dt.Rows[1]["Factor1"]);
                    factor2_H_0 = Convert.ToDouble(dt.Rows[0]["Factor2"]);
                    factor2_H_1 = Convert.ToDouble(dt.Rows[1]["Factor2"]);
                    factor3_H_0 = Convert.ToDouble(dt.Rows[0]["Factor3"]);
                    factor3_H_1 = Convert.ToDouble(dt.Rows[1]["Factor3"]);
                    factor4_H_0 = Convert.ToDouble(dt.Rows[0]["Factor4"]);
                    factor4_H_1 = Convert.ToDouble(dt.Rows[1]["Factor4"]);
                    factor5And6_H_0 = Convert.ToDouble(dt.Rows[0]["Factor5And6"]);
                    factor5And6_H_1 = Convert.ToDouble(dt.Rows[1]["Factor5And6"]);
                }
                else
                {
                    factor1E_H_0 = Convert.ToDouble(dt.Rows[count - 2]["Factor1E"]) + (angle - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) / (Convert.ToDouble(dt.Rows[count]["Angle"]) - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) * (Convert.ToDouble(dt.Rows[count]["Factor1E"]) - Convert.ToDouble(dt.Rows[count - 2]["Factor1E"]));
                    factor1E_H_1 = Convert.ToDouble(dt.Rows[count - 1]["Factor1E"]) + (angle - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) / (Convert.ToDouble(dt.Rows[count]["Angle"]) - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) * (Convert.ToDouble(dt.Rows[count+1]["Factor1E"]) - Convert.ToDouble(dt.Rows[count - 1]["Factor1E"]));
                    factor2E_H_0 = Convert.ToDouble(dt.Rows[count - 2]["Factor2E"]) + (angle - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) / (Convert.ToDouble(dt.Rows[count]["Angle"]) - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) * (Convert.ToDouble(dt.Rows[count]["Factor2E"]) - Convert.ToDouble(dt.Rows[count - 2]["Factor2E"]));
                    factor2E_H_1 = Convert.ToDouble(dt.Rows[count - 1]["Factor2E"]) + (angle - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) / (Convert.ToDouble(dt.Rows[count]["Angle"]) - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) * (Convert.ToDouble(dt.Rows[count+1]["Factor2E"]) - Convert.ToDouble(dt.Rows[count - 1]["Factor2E"]));
                    factor3E_H_0 = Convert.ToDouble(dt.Rows[count - 2]["Factor3E"]) + (angle - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) / (Convert.ToDouble(dt.Rows[count]["Angle"]) - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) * (Convert.ToDouble(dt.Rows[count]["Factor3E"]) - Convert.ToDouble(dt.Rows[count - 2]["Factor3E"]));
                    factor3E_H_1 = Convert.ToDouble(dt.Rows[count - 1]["Factor3E"]) + (angle - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) / (Convert.ToDouble(dt.Rows[count]["Angle"]) - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) * (Convert.ToDouble(dt.Rows[count+1]["Factor3E"]) - Convert.ToDouble(dt.Rows[count - 1]["Factor3E"]));
                    factor4E_H_0 = Convert.ToDouble(dt.Rows[count - 2]["Factor4E"]) + (angle - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) / (Convert.ToDouble(dt.Rows[count]["Angle"]) - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) * (Convert.ToDouble(dt.Rows[count]["Factor4E"]) - Convert.ToDouble(dt.Rows[count - 2]["Factor4E"]));
                    factor4E_H_1 = Convert.ToDouble(dt.Rows[count - 1]["Factor4E"]) + (angle - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) / (Convert.ToDouble(dt.Rows[count]["Angle"]) - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) * (Convert.ToDouble(dt.Rows[count+1]["Factor4E"]) - Convert.ToDouble(dt.Rows[count - 1]["Factor4E"]));
                    factor1_H_0 = Convert.ToDouble(dt.Rows[count - 2]["Factor1"]) + (angle - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) / (Convert.ToDouble(dt.Rows[count]["Angle"]) - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) * (Convert.ToDouble(dt.Rows[count]["Factor1"]) - Convert.ToDouble(dt.Rows[count - 2]["Factor1"]));
                    factor1_H_1 = Convert.ToDouble(dt.Rows[count - 1]["Factor1"]) + (angle - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) / (Convert.ToDouble(dt.Rows[count]["Angle"]) - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) * (Convert.ToDouble(dt.Rows[count+1]["Factor1"]) - Convert.ToDouble(dt.Rows[count - 1]["Factor1"]));
                    factor2_H_0 = Convert.ToDouble(dt.Rows[count - 2]["Factor2"]) + (angle - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) / (Convert.ToDouble(dt.Rows[count]["Angle"]) - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) * (Convert.ToDouble(dt.Rows[count]["Factor2"]) - Convert.ToDouble(dt.Rows[count - 2]["Factor2"]));
                    factor2_H_1 = Convert.ToDouble(dt.Rows[count - 1]["Factor2"]) + (angle - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) / (Convert.ToDouble(dt.Rows[count]["Angle"]) - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) * (Convert.ToDouble(dt.Rows[count+1]["Factor2"]) - Convert.ToDouble(dt.Rows[count - 1]["Factor2"]));
                    factor3_H_0 = Convert.ToDouble(dt.Rows[count - 2]["Factor3"]) + (angle - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) / (Convert.ToDouble(dt.Rows[count]["Angle"]) - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) * (Convert.ToDouble(dt.Rows[count]["Factor3"]) - Convert.ToDouble(dt.Rows[count - 2]["Factor3"]));
                    factor3_H_1 = Convert.ToDouble(dt.Rows[count - 1]["Factor3"]) + (angle - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) / (Convert.ToDouble(dt.Rows[count]["Angle"]) - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) * (Convert.ToDouble(dt.Rows[count+1]["Factor3"]) - Convert.ToDouble(dt.Rows[count - 1]["Factor3"]));
                    factor4_H_0 = Convert.ToDouble(dt.Rows[count - 2]["Factor4"]) + (angle - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) / (Convert.ToDouble(dt.Rows[count]["Angle"]) - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) * (Convert.ToDouble(dt.Rows[count]["Factor4"]) - Convert.ToDouble(dt.Rows[count - 2]["Factor4"]));
                    factor4_H_1 = Convert.ToDouble(dt.Rows[count - 1]["Factor4"]) + (angle - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) / (Convert.ToDouble(dt.Rows[count]["Angle"]) - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) * (Convert.ToDouble(dt.Rows[count+1]["Factor4"]) - Convert.ToDouble(dt.Rows[count - 1]["Factor4"]));
                    factor5And6_H_0 = Convert.ToDouble(dt.Rows[count - 2]["Factor5And6"]) + (angle - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) / (Convert.ToDouble(dt.Rows[count]["Angle"]) - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) * (Convert.ToDouble(dt.Rows[count]["Factor5And6"]) - Convert.ToDouble(dt.Rows[count - 2]["Factor5And6"]));
                    factor5And6_H_1 = Convert.ToDouble(dt.Rows[count - 1]["Factor5And6"]) + (angle - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) / (Convert.ToDouble(dt.Rows[count]["Angle"]) - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) * (Convert.ToDouble(dt.Rows[count+1]["Factor5And6"]) - Convert.ToDouble(dt.Rows[count - 1]["Factor5And6"]));
                }
            }
            if (ClosedForm.Text == "部分封闭式")
            {
                string sql = "select * from PartialClosedHorizentalWindFactor";
                DBHelper.PrepareSql(sql);
                DataTable dt = DBHelper.ExecQuery();
                int count = 0;
                for (int i = 0; i < dt.Rows.Count; i += 2)
                {
                    if (angle <= Convert.ToDouble(dt.Rows[i]["Angle"]))
                    {
                        count = i;
                        break;
                    }
                }
                if (count == 0)
                {
                    factor1E_H_0 = Convert.ToDouble(dt.Rows[0]["Factor1E"]);
                    factor1E_H_1 = Convert.ToDouble(dt.Rows[1]["Factor1E"]);
                    factor2E_H_0 = Convert.ToDouble(dt.Rows[0]["Factor2E"]);
                    factor2E_H_1 = Convert.ToDouble(dt.Rows[1]["Factor2E"]);
                    factor3E_H_0 = Convert.ToDouble(dt.Rows[0]["Factor3E"]);
                    factor3E_H_1 = Convert.ToDouble(dt.Rows[1]["Factor3E"]);
                    factor4E_H_0 = Convert.ToDouble(dt.Rows[0]["Factor4E"]);
                    factor4E_H_1 = Convert.ToDouble(dt.Rows[1]["Factor4E"]);
                    factor1_H_0 = Convert.ToDouble(dt.Rows[0]["Factor1"]);
                    factor1_H_1 = Convert.ToDouble(dt.Rows[1]["Factor1"]);
                    factor2_H_0 = Convert.ToDouble(dt.Rows[0]["Factor2"]);
                    factor2_H_1 = Convert.ToDouble(dt.Rows[1]["Factor2"]);
                    factor3_H_0 = Convert.ToDouble(dt.Rows[0]["Factor3"]);
                    factor3_H_1 = Convert.ToDouble(dt.Rows[1]["Factor3"]);
                    factor4_H_0 = Convert.ToDouble(dt.Rows[0]["Factor4"]);
                    factor4_H_1 = Convert.ToDouble(dt.Rows[1]["Factor4"]);
                    factor5And6_H_0 = Convert.ToDouble(dt.Rows[0]["Factor5And6"]);
                    factor5And6_H_1 = Convert.ToDouble(dt.Rows[1]["Factor5And6"]);
                }
                else
                {
                    factor1E_H_0 = Convert.ToDouble(dt.Rows[count - 2]["Factor1E"]) + (angle - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) / (Convert.ToDouble(dt.Rows[count]["Angle"]) - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) * (Convert.ToDouble(dt.Rows[count]["Factor1E"]) - Convert.ToDouble(dt.Rows[count - 2]["Factor1E"]));
                    factor1E_H_1 = Convert.ToDouble(dt.Rows[count - 1]["Factor1E"]) + (angle - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) / (Convert.ToDouble(dt.Rows[count]["Angle"]) - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) * (Convert.ToDouble(dt.Rows[count + 1]["Factor1E"]) - Convert.ToDouble(dt.Rows[count - 1]["Factor1E"]));
                    factor2E_H_0 = Convert.ToDouble(dt.Rows[count - 2]["Factor2E"]) + (angle - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) / (Convert.ToDouble(dt.Rows[count]["Angle"]) - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) * (Convert.ToDouble(dt.Rows[count]["Factor2E"]) - Convert.ToDouble(dt.Rows[count - 2]["Factor2E"]));
                    factor2E_H_1 = Convert.ToDouble(dt.Rows[count - 1]["Factor2E"]) + (angle - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) / (Convert.ToDouble(dt.Rows[count]["Angle"]) - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) * (Convert.ToDouble(dt.Rows[count + 1]["Factor2E"]) - Convert.ToDouble(dt.Rows[count - 1]["Factor2E"]));
                    factor3E_H_0 = Convert.ToDouble(dt.Rows[count - 2]["Factor3E"]) + (angle - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) / (Convert.ToDouble(dt.Rows[count]["Angle"]) - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) * (Convert.ToDouble(dt.Rows[count]["Factor3E"]) - Convert.ToDouble(dt.Rows[count - 2]["Factor3E"]));
                    factor3E_H_1 = Convert.ToDouble(dt.Rows[count - 1]["Factor3E"]) + (angle - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) / (Convert.ToDouble(dt.Rows[count]["Angle"]) - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) * (Convert.ToDouble(dt.Rows[count + 1]["Factor3E"]) - Convert.ToDouble(dt.Rows[count - 1]["Factor3E"]));
                    factor4E_H_0 = Convert.ToDouble(dt.Rows[count - 2]["Factor4E"]) + (angle - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) / (Convert.ToDouble(dt.Rows[count]["Angle"]) - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) * (Convert.ToDouble(dt.Rows[count]["Factor4E"]) - Convert.ToDouble(dt.Rows[count - 2]["Factor4E"]));
                    factor4E_H_1 = Convert.ToDouble(dt.Rows[count - 1]["Factor4E"]) + (angle - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) / (Convert.ToDouble(dt.Rows[count]["Angle"]) - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) * (Convert.ToDouble(dt.Rows[count + 1]["Factor4E"]) - Convert.ToDouble(dt.Rows[count - 1]["Factor4E"]));
                    factor1_H_0 = Convert.ToDouble(dt.Rows[count - 2]["Factor1"]) + (angle - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) / (Convert.ToDouble(dt.Rows[count]["Angle"]) - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) * (Convert.ToDouble(dt.Rows[count]["Factor1"]) - Convert.ToDouble(dt.Rows[count - 2]["Factor1"]));
                    factor1_H_1 = Convert.ToDouble(dt.Rows[count - 1]["Factor1"]) + (angle - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) / (Convert.ToDouble(dt.Rows[count]["Angle"]) - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) * (Convert.ToDouble(dt.Rows[count + 1]["Factor1"]) - Convert.ToDouble(dt.Rows[count - 1]["Factor1"]));
                    factor2_H_0 = Convert.ToDouble(dt.Rows[count - 2]["Factor2"]) + (angle - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) / (Convert.ToDouble(dt.Rows[count]["Angle"]) - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) * (Convert.ToDouble(dt.Rows[count]["Factor2"]) - Convert.ToDouble(dt.Rows[count - 2]["Factor2"]));
                    factor2_H_1 = Convert.ToDouble(dt.Rows[count - 1]["Factor2"]) + (angle - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) / (Convert.ToDouble(dt.Rows[count]["Angle"]) - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) * (Convert.ToDouble(dt.Rows[count + 1]["Factor2"]) - Convert.ToDouble(dt.Rows[count - 1]["Factor2"]));
                    factor3_H_0 = Convert.ToDouble(dt.Rows[count - 2]["Factor3"]) + (angle - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) / (Convert.ToDouble(dt.Rows[count]["Angle"]) - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) * (Convert.ToDouble(dt.Rows[count]["Factor3"]) - Convert.ToDouble(dt.Rows[count - 2]["Factor3"]));
                    factor3_H_1 = Convert.ToDouble(dt.Rows[count - 1]["Factor3"]) + (angle - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) / (Convert.ToDouble(dt.Rows[count]["Angle"]) - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) * (Convert.ToDouble(dt.Rows[count + 1]["Factor3"]) - Convert.ToDouble(dt.Rows[count - 1]["Factor3"]));
                    factor4_H_0 = Convert.ToDouble(dt.Rows[count - 2]["Factor4"]) + (angle - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) / (Convert.ToDouble(dt.Rows[count]["Angle"]) - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) * (Convert.ToDouble(dt.Rows[count]["Factor4"]) - Convert.ToDouble(dt.Rows[count - 2]["Factor4"]));
                    factor4_H_1 = Convert.ToDouble(dt.Rows[count - 1]["Factor4"]) + (angle - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) / (Convert.ToDouble(dt.Rows[count]["Angle"]) - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) * (Convert.ToDouble(dt.Rows[count + 1]["Factor4"]) - Convert.ToDouble(dt.Rows[count - 1]["Factor4"]));
                    factor5And6_H_0 = Convert.ToDouble(dt.Rows[count - 2]["Factor5And6"]) + (angle - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) / (Convert.ToDouble(dt.Rows[count]["Angle"]) - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) * (Convert.ToDouble(dt.Rows[count]["Factor5And6"]) - Convert.ToDouble(dt.Rows[count - 2]["Factor5And6"]));
                    factor5And6_H_1 = Convert.ToDouble(dt.Rows[count - 1]["Factor5And6"]) + (angle - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) / (Convert.ToDouble(dt.Rows[count]["Angle"]) - Convert.ToDouble(dt.Rows[count - 2]["Angle"])) * (Convert.ToDouble(dt.Rows[count + 1]["Factor5And6"]) - Convert.ToDouble(dt.Rows[count - 1]["Factor5And6"]));
                }
            }

            #endregion

            #region 主刚架纵向风荷载系数
            //第一个后缀h表示水平风荷载 L表示纵向风荷载   第二个后缀0表示（+i） 即内压为压力  1表示（-i）即内压为吸力
            //factor1E_0：1E分区在（+i）工况下的风荷载系数
            double factor1E_L_0 = 0;
            double factor1E_L_1 = 0;
            double factor2E_L_0 = 0;
            double factor2E_L_1 = 0;
            double factor3E_L_0 = 0;
            double factor3E_L_1 = 0;
            double factor4E_L_0 = 0;
            double factor4E_L_1 = 0;
            double factor1_L_0 = 0;
            double factor1_L_1 = 0;
            double factor2_L_0 = 0;
            double factor2_L_1 = 0;
            double factor3_L_0 = 0;
            double factor3_L_1 = 0;
            double factor4_L_0 = 0;
            double factor4_L_1 = 0;
            double factor5And6_L_0 = 0;
            double factor5And6_L_1 = 0;
            if (ClosedForm.Text == "封闭式")
            {
                string sql = "select * from ClosedLongitudinalWindFactor";
                DBHelper.PrepareSql(sql);
                DataTable dt = DBHelper.ExecQuery();
                factor1E_L_0 = Convert.ToDouble(dt.Rows[0]["Factor1E"]);
                factor1E_L_1 = Convert.ToDouble(dt.Rows[1]["Factor1E"]);
                factor2E_L_0 = Convert.ToDouble(dt.Rows[0]["Factor2E"]);
                factor2E_L_1 = Convert.ToDouble(dt.Rows[1]["Factor2E"]);
                factor3E_L_0 = Convert.ToDouble(dt.Rows[0]["Factor3E"]);
                factor3E_L_1= Convert.ToDouble(dt.Rows[1]["Factor3E"]);
                factor4E_L_0 = Convert.ToDouble(dt.Rows[0]["Factor4E"]);
                factor4E_L_1 = Convert.ToDouble(dt.Rows[1]["Factor4E"]);
                factor1_L_0 = Convert.ToDouble(dt.Rows[0]["Factor1"]);
                factor1_L_1 = Convert.ToDouble(dt.Rows[1]["Factor1"]);
                factor2_L_0 = Convert.ToDouble(dt.Rows[0]["Factor2"]);
                factor2_L_1 = Convert.ToDouble(dt.Rows[1]["Factor2"]);
                factor3_L_0 = Convert.ToDouble(dt.Rows[0]["Factor3"]);
                factor3_L_1 = Convert.ToDouble(dt.Rows[1]["Factor3"]);
                factor4_L_0 = Convert.ToDouble(dt.Rows[0]["Factor4"]);
                factor4_L_1 = Convert.ToDouble(dt.Rows[1]["Factor4"]);
                factor5And6_L_0 = Convert.ToDouble(dt.Rows[0]["Factor5And6"]);
                factor5And6_L_1 = Convert.ToDouble(dt.Rows[1]["Factor5And6"]);
            }
            if (ClosedForm.Text == "部分封闭式")
            {
                string sql = "select * from PartialClosedLongitudinalWindFactor";
                DBHelper.PrepareSql(sql);
                DataTable dt = DBHelper.ExecQuery();
                factor1E_L_0 = Convert.ToDouble(dt.Rows[0]["Factor1E"]);
                factor1E_L_1 = Convert.ToDouble(dt.Rows[1]["Factor1E"]);
                factor2E_L_0 = Convert.ToDouble(dt.Rows[0]["Factor2E"]);
                factor2E_L_1 = Convert.ToDouble(dt.Rows[1]["Factor2E"]);
                factor3E_L_0 = Convert.ToDouble(dt.Rows[0]["Factor3E"]);
                factor3E_L_1 = Convert.ToDouble(dt.Rows[1]["Factor3E"]);
                factor4E_L_0 = Convert.ToDouble(dt.Rows[0]["Factor4E"]);
                factor4E_L_1 = Convert.ToDouble(dt.Rows[1]["Factor4E"]);
                factor1_L_0 = Convert.ToDouble(dt.Rows[0]["Factor1"]);
                factor1_L_1 = Convert.ToDouble(dt.Rows[1]["Factor1"]);
                factor2_L_0 = Convert.ToDouble(dt.Rows[0]["Factor2"]);
                factor2_L_1 = Convert.ToDouble(dt.Rows[1]["Factor2"]);
                factor3_L_0 = Convert.ToDouble(dt.Rows[0]["Factor3"]);
                factor3_L_1 = Convert.ToDouble(dt.Rows[1]["Factor3"]);
                factor4_L_0 = Convert.ToDouble(dt.Rows[0]["Factor4"]);
                factor4_L_1 = Convert.ToDouble(dt.Rows[1]["Factor4"]);
                factor5And6_L_0 = Convert.ToDouble(dt.Rows[0]["Factor5And6"]);
                factor5And6_L_1 = Convert.ToDouble(dt.Rows[1]["Factor5And6"]);
            }
            #endregion
           




            using (Transaction ts=new Transaction(doc,"施加风荷载"))
            {
                ts.Start();
                //a-计算围护结构构件时的房屋边缘带宽度，取房屋最小水平尺寸的10%或0.4h之中较小值，但不小于房屋最小尺寸的4%或1m。
                //h
                double h =UnitExtension.ConvertToMillimeters((doc.GetElement(columns.FirstOrDefault().get_Parameter(BuiltInParameter.SCHEDULE_TOP_LEVEL_PARAM).AsElementId()) as Level).Elevation);
                //房屋最小尺寸
                List<double> yLength=new List<double>();
                for (int i = 0; i < columns.Count; i++)
                {
                    BoundingBoxXYZ box = columns[i].get_BoundingBox(view3D);
                    XYZ center = box.Min.Add(box.Max).Multiply(0.5);
                    ReferenceIntersector referenceIntersector = new ReferenceIntersector(columnsIds, FindReferenceTarget.Element, view3D);
                    //找Y正向柱并计算距离
                    List<double> lengthYPlus=new List<double>();
                    IList<ReferenceWithContext> referenceWithContextYPlusList = referenceIntersector.Find(center, XYZ.BasisY);
                    if (referenceWithContextYPlusList.Count > 0)
                    {
                        for (int j = 0; j < referenceWithContextYPlusList.Count; j++)
                        {
                            FamilyInstance instance = doc.GetElement(referenceWithContextYPlusList[j].GetReference()) as FamilyInstance;
                            double distance=(instance.Location as LocationPoint).Point.DistanceTo((columns[i].Location as LocationPoint).Point);
                            lengthYPlus.Add(distance);
                        }
                        yLength.Add(lengthYPlus.Max());
                    }
                }
                List<double> xLength = new List<double>();

                for (int i = 0; i < columns.Count; i++)
                {
                    BoundingBoxXYZ box = columns[i].get_BoundingBox(view3D);
                    XYZ center = box.Min.Add(box.Max).Multiply(0.5);
                    ReferenceIntersector referenceIntersector = new ReferenceIntersector(columnsIds, FindReferenceTarget.Element, view3D);
                    //找X正向柱并计算距离
                    List<double> lengthXPlus = new List<double>();
                    IList<ReferenceWithContext> referenceWithContextXPlusList = referenceIntersector.Find(center, XYZ.BasisX);
                    if (referenceWithContextXPlusList.Count > 0)
                    {
                        for (int j = 0; j < referenceWithContextXPlusList.Count; j++)
                        {
                            lengthXPlus.Add(referenceWithContextXPlusList[j].Proximity);
                        }
                        xLength.Add(lengthXPlus.Max());
                    }
                }
                double minSize =UnitExtension.ConvertToMillimeters(Math.Min(yLength.Max(), xLength.Max()));
                //射线算法得出的距离还有一些误差，后续需要改进
                MessageBox.Show(minSize.ToString());
                //a
                double a;
                if (0.04 * minSize > 1000)
                {
                    a = 1000;
                }
                else
                {
                    a=0.04*minSize;
                }
                if (0.1 * minSize > a)
                {
                    a = 0.1 * minSize;
                }
                if (0.4 * h < a)
                {
                    a = 0.4 * h;
                }
                //MessageBox.Show(a.ToString());
                //WY+  0.438 -1.25 -0.716 -0.609    0.225 -0.87 -0.554 -0.475
                
                //对柱和梁的集合先根据坐标Y进行排序，在此基础上根据坐标X进行排序
                List<FamilyInstance> orderedColumns=columns.OrderBy(t => (t.Location as LocationPoint).Point.Y).OrderBy(t => (t.Location as LocationPoint).Point.X).ToList();
                List<FamilyInstance> orderedBeams = beams.OrderBy(t =>(t.get_BoundingBox(view3D).Max.Add(t.get_BoundingBox(view3D).Min).Multiply(0.5).Y)).OrderBy(t=>(t.get_BoundingBox(view3D).Max.Add(t.get_BoundingBox(view3D).Min).Multiply(0.5).X)).ToList();
                List<List<FamilyInstance>> framings = new List<List<FamilyInstance>>();
                if (orderedColumns.Count != orderedBeams.Count)
                {
                    TextDialog textDialog1 = new TextDialog("梁柱对应数量有误,非单层单跨结构");
                    textDialog1.ShowDialog();
                    return;
                }
                //按柱、梁、梁、柱的一榀框架顺序放入framings集合中
                for (int i = 0; i < orderedColumns.Count; i+=2)
                {
                    List<FamilyInstance> temp = new List<FamilyInstance>();
                    temp.Add(orderedColumns[i]);
                    temp.Add(orderedBeams[i]);
                    temp.Add(orderedBeams[i + 1]);
                    temp.Add(orderedColumns[i + 1]);
                    framings.Add(temp);
                }
                //用recordWest和recordEast来记录WY工况下2a到边跨柱的哪一跨
                //用recordL2West来记录WX工况下L/2到边跨柱的哪一跨


                int record2aWest = 0;
                for (int i = 0; i < framings.Count; i++)
                {
                    double dis1 =UnitExtension.ConvertToMillimeters((framings[0][0].Location as LocationPoint).Point.DistanceTo((framings[i][0].Location as LocationPoint).Point));
                    if (dis1 > 2 * a)
                    {
                        record2aWest = i;
                        break;
                    }
                }
                int record2aEast = 0;
                for (int i = framings.Count-1; i > 0; i--)
                {
                    double dis2 = UnitExtension.ConvertToMillimeters((framings[framings.Count-1][0].Location as LocationPoint).Point.DistanceTo((framings[i][0].Location as LocationPoint).Point));
                    if (dis2 > 2 * a)
                    {
                        record2aEast = i;
                        break;
                    }
                }
                int recordL2West = 0;
                for (int i = 0; i < framings.Count; i++)
                {
                    double dis3 = UnitExtension.ConvertToMillimeters((framings[0][0].Location as LocationPoint).Point.DistanceTo((framings[i][0].Location as LocationPoint).Point));
                    if (dis3 > UnitExtension.ConvertToMillimeters(xLength.Max() / 2))
                    {
                        recordL2West = i;
                        break;
                    }
                }


                #region WY工况下主框架风荷载
                int startMiddle = record2aWest;
                int endMiddle = record2aEast;

                //通过2a所在位置划分中间跨和边跨，在startMiddle和endMiddle之间为中间跨，0和startMiddle之间为左边跨，endMiddle和framings.count-1之间为右边跨
                ReferenceIntersector referenceIntersectorColumn = new ReferenceIntersector(columnsIds, FindReferenceTarget.Element, view3D);
                //找X负向的最近柱并计算距离               
                double disRecordWestXMinus = SetLoadExtension.CalculateNearestDistance(doc, framings[record2aWest][0],-XYZ.BasisX,referenceIntersectorColumn);
                double tempVal1 = UnitExtension.ConvertToMillimeters((framings[record2aWest][0].Location as LocationPoint).Point.DistanceTo((framings[0][0].Location as LocationPoint).Point));
                if (tempVal1 - 2 * a <= disRecordWestXMinus / 2)
                {
                    startMiddle = record2aWest + 1;
                }

                //找x正向的最近柱并计算距离
                double disRecordEastXPlus = SetLoadExtension.CalculateNearestDistance(doc, framings[record2aEast][0], XYZ.BasisX, referenceIntersectorColumn);
                double tempVal2 = UnitExtension.ConvertToMillimeters((framings[record2aEast][0].Location as LocationPoint).Point.DistanceTo((framings[framings.Count - 1][0].Location as LocationPoint).Point));
                if (tempVal2 - 2 * a <= disRecordEastXPlus / 2)
                {
                    endMiddle = record2aEast - 1;
                }

                #region 临界跨（西）第startMiddle-1跨
                int criticalWest = startMiddle - 1;

                //找X负向的最近柱并计算距离
                double disCriticalWestXMinus = SetLoadExtension.CalculateNearestDistance(doc, framings[criticalWest][0], -XYZ.BasisX, referenceIntersectorColumn);
                //找x正向的最近柱并计算距离
                double disCriticalWestXPlus = SetLoadExtension.CalculateNearestDistance(doc, framings[criticalWest][0], XYZ.BasisX, referenceIntersectorColumn);
                //1柱
                XYZ startCriticalWest1 = (framings[criticalWest][0].Location as LocationPoint).Point;
                double heightCriticalWest1 = (doc.GetElement(framings[criticalWest][0].get_Parameter(BuiltInParameter.SCHEDULE_TOP_LEVEL_PARAM).AsElementId()) as Level).Elevation;
                XYZ endCriticalWest1 = (framings[criticalWest][0].Location as LocationPoint).Point + new XYZ(0, 0, heightCriticalWest1);
                LineLoad[] WYCriticalWest_1 = new LineLoad[4]; //WYCriticalWest_1[0]:1柱WY+(+i) WYCriticalWest_1[1]:1柱WY+(-i) WYCriticalWest_1[2]:1柱WY-(+i) WYCriticalWest_1[3]:1柱WY-(-i) 
                for (int i = 0; i < 4; i++)
                {
                    WYCriticalWest_1[i] = LineLoad.Create(doc, startCriticalWest1, endCriticalWest1, XYZ.BasisY, XYZ.Zero, lineLoadType, null);
                }
                double adjustFactorCriticalWest_1 = SetLoadExtension.GetHeightVariationCoefficientOfWindPressure(doc, framings[criticalWest][0], groundRoughness);
                //赋予荷载工况 
                WYCriticalWest_1[0].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYPlusLoadCase_0.Id);
                WYCriticalWest_1[1].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYPlusLoadCase_1.Id);
                WYCriticalWest_1[2].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYMinusLoadCase_0.Id);
                WYCriticalWest_1[3].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYMinusLoadCase_1.Id);
                //附加说明
                WYCriticalWest_1[0].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[criticalWest][0].Id.ToString());
                WYCriticalWest_1[1].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[criticalWest][0].Id.ToString());
                WYCriticalWest_1[2].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[criticalWest][0].Id.ToString());
                WYCriticalWest_1[3].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[criticalWest][0].Id.ToString());

                WYCriticalWest_1[0].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                WYCriticalWest_1[1].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                WYCriticalWest_1[2].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                WYCriticalWest_1[3].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");

                //2梁
                XYZ startCriticalWest2 = ((framings[criticalWest][1].Location as LocationCurve).Curve as Line).Origin;
                double lengthCriticalWest2 = ((framings[criticalWest][1].Location as LocationCurve).Curve as Line).Length;
                XYZ directionCriticalWest2 = ((framings[criticalWest][1].Location as LocationCurve).Curve as Line).Direction;
                XYZ endCriticalWest2 = new XYZ(startCriticalWest2.X + lengthCriticalWest2 * directionCriticalWest2.X / Math.Sqrt(directionCriticalWest2.X * directionCriticalWest2.X + directionCriticalWest2.Y * directionCriticalWest2.Y + directionCriticalWest2.Z * directionCriticalWest2.Z),
                   startCriticalWest2.Y + lengthCriticalWest2 * directionCriticalWest2.Y / Math.Sqrt(directionCriticalWest2.X * directionCriticalWest2.X + directionCriticalWest2.Y * directionCriticalWest2.Y + directionCriticalWest2.Z * directionCriticalWest2.Z),
                   startCriticalWest2.Z + lengthCriticalWest2 * directionCriticalWest2.Z / Math.Sqrt(directionCriticalWest2.X * directionCriticalWest2.X + directionCriticalWest2.Y * directionCriticalWest2.Y + directionCriticalWest2.Z * directionCriticalWest2.Z));
                LineLoad[] WYCriticalWest_2 = new LineLoad[4];
                for (int i = 0; i < 4; i++)
                {
                    WYCriticalWest_2[i]= LineLoad.Create(doc, startCriticalWest2, endCriticalWest2, XYZ.BasisZ, XYZ.Zero, lineLoadType, null);
                }
                double adjustFactorCriticalWest_2 = SetLoadExtension.GetHeightVariationCoefficientOfWindPressure(doc, framings[criticalWest][1], groundRoughness);

                //赋予荷载工况 
                WYCriticalWest_2[0].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYPlusLoadCase_0.Id);
                WYCriticalWest_2[1].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYPlusLoadCase_1.Id);
                WYCriticalWest_2[2].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYMinusLoadCase_0.Id);
                WYCriticalWest_2[3].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYMinusLoadCase_1.Id);
                //附加说明
                WYCriticalWest_2[0].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[criticalWest][1].Id.ToString());
                WYCriticalWest_2[1].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[criticalWest][1].Id.ToString());
                WYCriticalWest_2[2].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[criticalWest][1].Id.ToString());
                WYCriticalWest_2[3].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[criticalWest][1].Id.ToString());

                WYCriticalWest_2[0].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                WYCriticalWest_2[1].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                WYCriticalWest_2[2].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                WYCriticalWest_2[3].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");

                //3梁
                XYZ startCriticalWest3 = ((framings[criticalWest][2].Location as LocationCurve).Curve as Line).Origin;
                double lengthCriticalWest3 = ((framings[criticalWest][2].Location as LocationCurve).Curve as Line).Length;
                XYZ directionCriticalWest3 = ((framings[criticalWest][2].Location as LocationCurve).Curve as Line).Direction;
                XYZ endCriticalWest3 = new XYZ(startCriticalWest3.X + lengthCriticalWest3 * directionCriticalWest3.X / Math.Sqrt(directionCriticalWest3.X * directionCriticalWest3.X + directionCriticalWest3.Y * directionCriticalWest3.Y + directionCriticalWest3.Z * directionCriticalWest3.Z),
                                           startCriticalWest3.Y + lengthCriticalWest3 * directionCriticalWest3.Y / Math.Sqrt(directionCriticalWest3.X * directionCriticalWest3.X + directionCriticalWest3.Y * directionCriticalWest3.Y + directionCriticalWest3.Z * directionCriticalWest3.Z),
                                           startCriticalWest3.Z + lengthCriticalWest3 * directionCriticalWest3.Z / Math.Sqrt(directionCriticalWest3.X * directionCriticalWest3.X + directionCriticalWest3.Y * directionCriticalWest3.Y + directionCriticalWest3.Z * directionCriticalWest3.Z));
                LineLoad[] WYCriticalWest_3 = new LineLoad[4];
                for (int i = 0; i < 4; i++)
                {
                    WYCriticalWest_3[i] = LineLoad.Create(doc, startCriticalWest3, endCriticalWest3, XYZ.BasisZ, XYZ.Zero, lineLoadType, null);
                }
                double adjustFactorCriticalWest_3 = SetLoadExtension.GetHeightVariationCoefficientOfWindPressure(doc, framings[criticalWest][2], groundRoughness);

                //赋予荷载工况 
                WYCriticalWest_3[0].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYPlusLoadCase_0.Id);
                WYCriticalWest_3[1].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYPlusLoadCase_1.Id);
                WYCriticalWest_3[2].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYMinusLoadCase_0.Id);
                WYCriticalWest_3[3].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYMinusLoadCase_1.Id);
                //附加说明
                WYCriticalWest_3[0].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[criticalWest][2].Id.ToString());
                WYCriticalWest_3[1].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[criticalWest][2].Id.ToString());
                WYCriticalWest_3[2].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[criticalWest][2].Id.ToString());
                WYCriticalWest_3[3].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[criticalWest][2].Id.ToString());

                WYCriticalWest_3[0].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                WYCriticalWest_3[1].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                WYCriticalWest_3[2].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                WYCriticalWest_3[3].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                //4柱
                XYZ startCriticalWest4 = (framings[criticalWest][3].Location as LocationPoint).Point;
                double heightCriticalWest4 = (doc.GetElement(framings[criticalWest][3].get_Parameter(BuiltInParameter.SCHEDULE_TOP_LEVEL_PARAM).AsElementId()) as Level).Elevation;
                XYZ endCriticalWest4 = (framings[criticalWest][3].Location as LocationPoint).Point + new XYZ(0, 0, heightCriticalWest4);
                LineLoad[] WYCriticalWest_4 = new LineLoad[4]; //WYCriticalWest_1[0]:1柱WY+(+i) WYCriticalWest_1[1]:1柱WY+(-i) WYCriticalWest_1[2]:1柱WY-(+i) WYCriticalWest_1[3]:1柱WY-(-i) 
                for (int i = 0; i < 4; i++)
                {
                    WYCriticalWest_4[i] = LineLoad.Create(doc, startCriticalWest4, endCriticalWest4, XYZ.BasisY, XYZ.Zero, lineLoadType, null);
                }
                double adjustFactorCriticalWest_4 = SetLoadExtension.GetHeightVariationCoefficientOfWindPressure(doc, framings[criticalWest][3], groundRoughness);

                //赋予荷载工况 
                WYCriticalWest_4[0].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYPlusLoadCase_0.Id);
                WYCriticalWest_4[1].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYPlusLoadCase_1.Id);
                WYCriticalWest_4[2].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYMinusLoadCase_0.Id);
                WYCriticalWest_4[3].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYMinusLoadCase_1.Id);
                //附加说明
                WYCriticalWest_4[0].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[criticalWest][3].Id.ToString());
                WYCriticalWest_4[1].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[criticalWest][3].Id.ToString());
                WYCriticalWest_4[2].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[criticalWest][3].Id.ToString());
                WYCriticalWest_4[3].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[criticalWest][3].Id.ToString());

                WYCriticalWest_4[0].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                WYCriticalWest_4[1].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                WYCriticalWest_4[2].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                WYCriticalWest_4[3].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");

                double tempVal3 = UnitExtension.ConvertToMillimeters((framings[criticalWest][0].Location as LocationPoint).Point.DistanceTo((framings[0][0].Location as LocationPoint).Point));
                if (2 * a - tempVal3 < 0)//此时说明2a这条分界线在临界柱的左侧
                {

                    //更改1柱荷载大小

                    WYCriticalWest_1[0].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).Set((disCriticalWestXMinus / 2 - (tempVal3 - 2 * a)) * factor1E_H_0 * basicWindPressure * adjustFactorCriticalWest_1 * adjustFactor + ((tempVal3 - 2 * a) + disCriticalWestXPlus / 2) * factor1_H_0 * basicWindPressure * adjustFactorCriticalWest_1 * adjustFactor);
                    WYCriticalWest_1[1].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).Set((disCriticalWestXMinus / 2 - (tempVal3 - 2 * a)) * factor1E_H_1 * basicWindPressure * adjustFactorCriticalWest_1 * adjustFactor + ((tempVal3 - 2 * a) + disCriticalWestXPlus / 2) * factor1_H_1 * basicWindPressure * adjustFactorCriticalWest_1 * adjustFactor);
                    WYCriticalWest_1[2].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).Set((disCriticalWestXMinus / 2 - (tempVal3 - 2 * a)) * factor4E_H_0 * basicWindPressure * adjustFactorCriticalWest_1 * adjustFactor + ((tempVal3 - 2 * a) + disCriticalWestXPlus / 2) * factor4_H_0 * basicWindPressure * adjustFactorCriticalWest_1 * adjustFactor);
                    WYCriticalWest_1[3].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).Set((disCriticalWestXMinus / 2 - (tempVal3 - 2 * a)) * factor4E_H_1 * basicWindPressure * adjustFactorCriticalWest_1 * adjustFactor + ((tempVal3 - 2 * a) + disCriticalWestXPlus / 2) * factor4_H_1 * basicWindPressure * adjustFactorCriticalWest_1 * adjustFactor);

                    //更改2梁荷载大小
                    WYCriticalWest_2[0].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set((disCriticalWestXMinus / 2 - (tempVal3 - 2 * a)) * -factor2E_H_0 * basicWindPressure * adjustFactorCriticalWest_2 * adjustFactor + ((tempVal3 - 2 * a) + disCriticalWestXPlus / 2) * -factor2_H_0 * basicWindPressure * adjustFactorCriticalWest_2 * adjustFactor);
                    WYCriticalWest_2[1].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set((disCriticalWestXMinus / 2 - (tempVal3 - 2 * a)) * -factor2E_H_1 * basicWindPressure * adjustFactorCriticalWest_2 * adjustFactor + ((tempVal3 - 2 * a) + disCriticalWestXPlus / 2) * -factor2_H_1 * basicWindPressure * adjustFactorCriticalWest_2 * adjustFactor);
                    WYCriticalWest_2[2].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set((disCriticalWestXMinus / 2 - (tempVal3 - 2 * a)) * -factor3E_H_0 * basicWindPressure * adjustFactorCriticalWest_2 * adjustFactor + ((tempVal3 - 2 * a) + disCriticalWestXPlus / 2) * -factor3_H_0 * basicWindPressure * adjustFactorCriticalWest_2 * adjustFactor);
                    WYCriticalWest_2[3].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set((disCriticalWestXMinus / 2 - (tempVal3 - 2 * a)) * -factor3E_H_1 * basicWindPressure * adjustFactorCriticalWest_2 * adjustFactor + ((tempVal3 - 2 * a) + disCriticalWestXPlus / 2) * -factor3_H_1 * basicWindPressure * adjustFactorCriticalWest_2 * adjustFactor);

                    //更改3梁荷载大小
                    WYCriticalWest_3[0].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set((disCriticalWestXMinus / 2 - (tempVal3 - 2 * a)) * -factor3E_H_0 * basicWindPressure * adjustFactorCriticalWest_3 * adjustFactor + ((tempVal3 - 2 * a) + disCriticalWestXPlus / 2) * -factor3_H_0 * basicWindPressure * adjustFactorCriticalWest_3 * adjustFactor);
                    WYCriticalWest_3[1].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set((disCriticalWestXMinus / 2 - (tempVal3 - 2 * a)) * -factor3E_H_1 * basicWindPressure * adjustFactorCriticalWest_3 * adjustFactor + ((tempVal3 - 2 * a) + disCriticalWestXPlus / 2) * -factor3_H_1 * basicWindPressure * adjustFactorCriticalWest_3 * adjustFactor);
                    WYCriticalWest_3[2].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set((disCriticalWestXMinus / 2 - (tempVal3 - 2 * a)) * -factor2E_H_0 * basicWindPressure * adjustFactorCriticalWest_3 * adjustFactor + ((tempVal3 - 2 * a) + disCriticalWestXPlus / 2) * -factor2_H_0 * basicWindPressure * adjustFactorCriticalWest_3 * adjustFactor);
                    WYCriticalWest_3[3].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set((disCriticalWestXMinus / 2 - (tempVal3 - 2 * a)) * -factor2E_H_1 * basicWindPressure * adjustFactorCriticalWest_3 * adjustFactor + ((tempVal3 - 2 * a) + disCriticalWestXPlus / 2) * -factor2_H_1 * basicWindPressure * adjustFactorCriticalWest_3 * adjustFactor);

                    //更改4柱荷载大小
                    WYCriticalWest_4[0].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).Set((disCriticalWestXMinus / 2 - (tempVal3 - 2 * a)) * -factor4E_H_0 * basicWindPressure * adjustFactorCriticalWest_4 * adjustFactor + ((tempVal3 - 2 * a) + disCriticalWestXPlus / 2) * -factor4_H_0 * basicWindPressure * adjustFactorCriticalWest_4 * adjustFactor);
                    WYCriticalWest_4[1].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).Set((disCriticalWestXMinus / 2 - (tempVal3 - 2 * a)) * -factor4E_H_1 * basicWindPressure * adjustFactorCriticalWest_4 * adjustFactor + ((tempVal3 - 2 * a) + disCriticalWestXPlus / 2) * -factor4_H_1 * basicWindPressure * adjustFactorCriticalWest_4 * adjustFactor);
                    WYCriticalWest_4[2].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).Set((disCriticalWestXMinus / 2 - (tempVal3 - 2 * a)) * -factor1E_H_0 * basicWindPressure * adjustFactorCriticalWest_4 * adjustFactor + ((tempVal3 - 2 * a) + disCriticalWestXPlus / 2) * -factor1_H_0 * basicWindPressure * adjustFactorCriticalWest_4 * adjustFactor);
                    WYCriticalWest_4[3].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).Set((disCriticalWestXMinus / 2 - (tempVal3 - 2 * a)) * -factor1E_H_1 * basicWindPressure * adjustFactorCriticalWest_4 * adjustFactor + ((tempVal3 - 2 * a) + disCriticalWestXPlus / 2) * -factor1_H_1 * basicWindPressure * adjustFactorCriticalWest_4 * adjustFactor);
                }
                else//说明2a这条分界线在临界柱的右侧
                {
                    //更改1柱荷载大小
                    WYCriticalWest_1[0].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).Set((disCriticalWestXMinus / 2 + 2 * a - tempVal3) * factor1E_H_0 * basicWindPressure * adjustFactorCriticalWest_1 * adjustFactor + (disCriticalWestXPlus / 2 - (2 * a - tempVal3)) * factor1_H_0 * basicWindPressure * adjustFactorCriticalWest_1 * adjustFactor);
                    WYCriticalWest_1[1].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).Set((disCriticalWestXMinus / 2 + 2 * a - tempVal3) * factor1E_H_1 * basicWindPressure * adjustFactorCriticalWest_1 * adjustFactor + (disCriticalWestXPlus / 2 - (2 * a - tempVal3)) * factor1_H_1 * basicWindPressure * adjustFactorCriticalWest_1 * adjustFactor);
                    WYCriticalWest_1[2].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).Set((disCriticalWestXMinus / 2 + 2 * a - tempVal3) * factor4E_H_0 * basicWindPressure * adjustFactorCriticalWest_1 * adjustFactor + (disCriticalWestXPlus / 2 - (2 * a - tempVal3)) * factor4_H_0 * basicWindPressure * adjustFactorCriticalWest_1 * adjustFactor);
                    WYCriticalWest_1[3].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).Set((disCriticalWestXMinus / 2 + 2 * a - tempVal3) * factor4E_H_1 * basicWindPressure * adjustFactorCriticalWest_1 * adjustFactor + (disCriticalWestXPlus / 2 - (2 * a - tempVal3)) * factor4_H_1 * basicWindPressure * adjustFactorCriticalWest_1 * adjustFactor);
                    //更改2梁荷载大小
                    WYCriticalWest_1[0].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set((disCriticalWestXMinus / 2 + 2 * a - tempVal3) * -factor2E_H_0 * basicWindPressure * adjustFactorCriticalWest_2 * adjustFactor + (disCriticalWestXPlus / 2 - (2 * a - tempVal3)) * -factor2_H_0 * basicWindPressure * adjustFactorCriticalWest_2 * adjustFactor);
                    WYCriticalWest_1[1].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set((disCriticalWestXMinus / 2 + 2 * a - tempVal3) * -factor2E_H_1 * basicWindPressure * adjustFactorCriticalWest_2 * adjustFactor + (disCriticalWestXPlus / 2 - (2 * a - tempVal3)) * -factor2_H_1 * basicWindPressure * adjustFactorCriticalWest_2 * adjustFactor);
                    WYCriticalWest_1[2].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set((disCriticalWestXMinus / 2 + 2 * a - tempVal3) * -factor3E_H_0 * basicWindPressure * adjustFactorCriticalWest_2 * adjustFactor + (disCriticalWestXPlus / 2 - (2 * a - tempVal3)) * -factor3_H_0 * basicWindPressure * adjustFactorCriticalWest_2 * adjustFactor);
                    WYCriticalWest_1[3].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set((disCriticalWestXMinus / 2 + 2 * a - tempVal3) * -factor3E_H_1 * basicWindPressure * adjustFactorCriticalWest_2 * adjustFactor + (disCriticalWestXPlus / 2 - (2 * a - tempVal3)) * -factor3_H_1 * basicWindPressure * adjustFactorCriticalWest_2 * adjustFactor);
                    //更改3梁荷载大小
                    WYCriticalWest_3[0].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set((disCriticalWestXMinus / 2 + 2 * a - tempVal3) * -factor3E_H_0 * basicWindPressure * adjustFactorCriticalWest_3 * adjustFactor + (disCriticalWestXPlus / 2 - (2 * a - tempVal3)) * -factor3_H_0 * basicWindPressure * adjustFactorCriticalWest_3 * adjustFactor);
                    WYCriticalWest_3[1].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set((disCriticalWestXMinus / 2 + 2 * a - tempVal3) * -factor3E_H_1 * basicWindPressure * adjustFactorCriticalWest_3 * adjustFactor + (disCriticalWestXPlus / 2 - (2 * a - tempVal3)) * -factor3_H_1 * basicWindPressure * adjustFactorCriticalWest_3 * adjustFactor);
                    WYCriticalWest_3[2].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set((disCriticalWestXMinus / 2 + 2 * a - tempVal3) * -factor2E_H_0 * basicWindPressure * adjustFactorCriticalWest_3 * adjustFactor + (disCriticalWestXPlus / 2 - (2 * a - tempVal3)) * -factor2_H_0 * basicWindPressure * adjustFactorCriticalWest_3 * adjustFactor);
                    WYCriticalWest_3[3].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set((disCriticalWestXMinus / 2 + 2 * a - tempVal3) * -factor2E_H_1 * basicWindPressure * adjustFactorCriticalWest_3 * adjustFactor + (disCriticalWestXPlus / 2 - (2 * a - tempVal3)) * -factor2_H_1 * basicWindPressure * adjustFactorCriticalWest_3 * adjustFactor);
                    //更改4柱荷载大小
                    WYCriticalWest_4[0].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).Set((disCriticalWestXMinus / 2 + 2 * a - tempVal3) * -factor4E_H_0 * basicWindPressure * adjustFactorCriticalWest_4 * adjustFactor + (disCriticalWestXPlus / 2 - (2 * a - tempVal3)) * -factor4_H_0 * basicWindPressure * adjustFactorCriticalWest_4 * adjustFactor);
                    WYCriticalWest_4[1].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).Set((disCriticalWestXMinus / 2 + 2 * a - tempVal3) * -factor4E_H_1 * basicWindPressure * adjustFactorCriticalWest_4 * adjustFactor + (disCriticalWestXPlus / 2 - (2 * a - tempVal3)) * -factor4_H_1 * basicWindPressure * adjustFactorCriticalWest_4 * adjustFactor);
                    WYCriticalWest_4[2].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).Set((disCriticalWestXMinus / 2 + 2 * a - tempVal3) * -factor1E_H_0 * basicWindPressure * adjustFactorCriticalWest_4 * adjustFactor + (disCriticalWestXPlus / 2 - (2 * a - tempVal3)) * -factor1_H_0 * basicWindPressure * adjustFactorCriticalWest_4 * adjustFactor);
                    WYCriticalWest_4[3].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).Set((disCriticalWestXMinus / 2 + 2 * a - tempVal3) * -factor1E_H_1 * basicWindPressure * adjustFactorCriticalWest_4 * adjustFactor + (disCriticalWestXPlus / 2 - (2 * a - tempVal3)) * -factor1_H_1 * basicWindPressure * adjustFactorCriticalWest_4 * adjustFactor);

                }
                #endregion

                #region 临界跨（东）第endMiddle+1跨
                int criticalEast = endMiddle + 1;
                //找X负向的最近柱并计算距离
                double disCriticalEastXMinus = SetLoadExtension.CalculateNearestDistance(doc, framings[criticalEast][0], -XYZ.BasisX, referenceIntersectorColumn);
                //找x正向的最近柱并计算距离
                double disCriticalEastXPlus = SetLoadExtension.CalculateNearestDistance(doc, framings[criticalEast][0], XYZ.BasisX, referenceIntersectorColumn);
                //1柱
                XYZ startCriticalEast1 = (framings[criticalEast][0].Location as LocationPoint).Point;
                double heightCriticalEast1 = (doc.GetElement(framings[criticalEast][0].get_Parameter(BuiltInParameter.SCHEDULE_TOP_LEVEL_PARAM).AsElementId()) as Level).Elevation;
                XYZ endCriticalEast1 = (framings[criticalEast][0].Location as LocationPoint).Point + new XYZ(0, 0, heightCriticalEast1);
                LineLoad[] WYCriticalEast_1 = new LineLoad[4];
                for (int i = 0; i < 4; i++)
                {
                    WYCriticalEast_1[i] = LineLoad.Create(doc, startCriticalEast1, endCriticalEast1, XYZ.BasisY, XYZ.Zero, lineLoadType, null);
                }
                double adjustFactorCriticalEast_1 = SetLoadExtension.GetHeightVariationCoefficientOfWindPressure(doc, framings[criticalEast][0],groundRoughness);
                //赋予荷载工况 
                WYCriticalEast_1[0].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYPlusLoadCase_0.Id);
                WYCriticalEast_1[1].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYPlusLoadCase_1.Id);
                WYCriticalEast_1[2].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYMinusLoadCase_0.Id);
                WYCriticalEast_1[3].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYMinusLoadCase_1.Id);
                //附加说明
                WYCriticalEast_1[0].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[criticalEast][0].Id.ToString());
                WYCriticalEast_1[1].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[criticalEast][0].Id.ToString());
                WYCriticalEast_1[2].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[criticalEast][0].Id.ToString());
                WYCriticalEast_1[3].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[criticalEast][0].Id.ToString());

                WYCriticalEast_1[0].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                WYCriticalEast_1[1].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                WYCriticalEast_1[2].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                WYCriticalEast_1[3].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");

                //2梁
                XYZ startCriticalEast2 = ((framings[criticalEast][1].Location as LocationCurve).Curve as Line).Origin;
                double lengthCriticalEast2 = ((framings[criticalEast][1].Location as LocationCurve).Curve as Line).Length;
                XYZ directionCriticalEast2 = ((framings[criticalEast][1].Location as LocationCurve).Curve as Line).Direction;
                XYZ endCriticalEast2 = new XYZ(startCriticalEast2.X + lengthCriticalEast2 * directionCriticalEast2.X / Math.Sqrt(directionCriticalEast2.X * directionCriticalEast2.X + directionCriticalEast2.Y * directionCriticalEast2.Y + directionCriticalEast2.Z * directionCriticalEast2.Z),
                   startCriticalEast2.Y + lengthCriticalEast2 * directionCriticalEast2.Y / Math.Sqrt(directionCriticalEast2.X * directionCriticalEast2.X + directionCriticalEast2.Y * directionCriticalEast2.Y + directionCriticalEast2.Z * directionCriticalEast2.Z),
                   startCriticalEast2.Z + lengthCriticalEast2 * directionCriticalEast2.Z / Math.Sqrt(directionCriticalEast2.X * directionCriticalEast2.X + directionCriticalEast2.Y * directionCriticalEast2.Y + directionCriticalEast2.Z * directionCriticalEast2.Z));
                LineLoad[] WYCriticalEast_2 = new LineLoad[4];               
                for (int i = 0; i < 4; i++)
                {
                    WYCriticalEast_2[i] = LineLoad.Create(doc, startCriticalEast2, endCriticalEast2, XYZ.BasisZ, XYZ.Zero, lineLoadType, null);

                }
                double adjustFactorCriticalEast_2 = SetLoadExtension.GetHeightVariationCoefficientOfWindPressure(doc, framings[criticalEast][1], groundRoughness);

                //赋予荷载工况 
                WYCriticalEast_2[0].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYPlusLoadCase_0.Id);
                WYCriticalEast_2[1].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYPlusLoadCase_1.Id);
                WYCriticalEast_2[2].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYMinusLoadCase_0.Id);
                WYCriticalEast_2[3].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYMinusLoadCase_1.Id);
                //附加说明
                WYCriticalEast_2[0].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[criticalEast][1].Id.ToString());
                WYCriticalEast_2[1].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[criticalEast][1].Id.ToString());
                WYCriticalEast_2[2].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[criticalEast][1].Id.ToString());
                WYCriticalEast_2[3].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[criticalEast][1].Id.ToString());

                WYCriticalEast_2[0].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                WYCriticalEast_2[1].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                WYCriticalEast_2[2].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                WYCriticalEast_2[3].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                //3梁
                XYZ startCriticalEast3 = ((framings[criticalEast][2].Location as LocationCurve).Curve as Line).Origin;
                double lengthCriticalEast3 = ((framings[criticalEast][2].Location as LocationCurve).Curve as Line).Length;
                XYZ directionCriticalEast3 = ((framings[criticalEast][2].Location as LocationCurve).Curve as Line).Direction;
                XYZ endCriticalEast3 = new XYZ(startCriticalEast3.X + lengthCriticalEast3 * directionCriticalEast3.X / Math.Sqrt(directionCriticalEast3.X * directionCriticalEast3.X + directionCriticalEast3.Y * directionCriticalEast3.Y + directionCriticalEast3.Z * directionCriticalEast3.Z),
                                           startCriticalEast3.Y + lengthCriticalEast3 * directionCriticalEast3.Y / Math.Sqrt(directionCriticalEast3.X * directionCriticalEast3.X + directionCriticalWest3.Y * directionCriticalEast3.Y + directionCriticalEast3.Z * directionCriticalEast3.Z),
                                           startCriticalEast3.Z + lengthCriticalEast3 * directionCriticalEast3.Z / Math.Sqrt(directionCriticalEast3.X * directionCriticalEast3.X + directionCriticalWest3.Y * directionCriticalEast3.Y + directionCriticalEast3.Z * directionCriticalEast3.Z));
                LineLoad[] WYCriticalEast_3 = new LineLoad[4];
                for (int i = 0; i < 4; i++)
                {
                    WYCriticalEast_3[i] = LineLoad.Create(doc, startCriticalEast3, endCriticalEast3, XYZ.BasisZ, XYZ.Zero, lineLoadType, null);                   
                }
                double adjustFactorCriticalEast_3 = SetLoadExtension.GetHeightVariationCoefficientOfWindPressure(doc, framings[criticalEast][2], groundRoughness);

                //赋予荷载工况 
                WYCriticalEast_3[0].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYPlusLoadCase_0.Id);
                WYCriticalEast_3[1].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYPlusLoadCase_1.Id);
                WYCriticalEast_3[2].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYMinusLoadCase_0.Id);
                WYCriticalEast_3[3].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYMinusLoadCase_1.Id);
                //附加说明
                WYCriticalEast_3[0].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[criticalEast][2].Id.ToString());
                WYCriticalEast_3[1].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[criticalEast][2].Id.ToString());
                WYCriticalEast_3[2].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[criticalEast][2].Id.ToString());
                WYCriticalEast_3[3].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[criticalEast][2].Id.ToString());

                WYCriticalEast_3[0].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                WYCriticalEast_3[1].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                WYCriticalEast_3[2].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                WYCriticalEast_3[3].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                //4柱
                XYZ startCriticalEast4 = (framings[criticalEast][3].Location as LocationPoint).Point;
                double heightCriticalEast4 = (doc.GetElement(framings[criticalEast][3].get_Parameter(BuiltInParameter.SCHEDULE_TOP_LEVEL_PARAM).AsElementId()) as Level).Elevation;
                XYZ endCriticalEast4 = (framings[criticalEast][3].Location as LocationPoint).Point + new XYZ(0, 0, heightCriticalEast4);
                LineLoad[] WYCriticalEast_4 = new LineLoad[4];
                for (int i = 0; i < 4; i++)
                {
                    WYCriticalEast_4[i]= LineLoad.Create(doc, startCriticalEast4, endCriticalEast4, XYZ.BasisY, XYZ.Zero, lineLoadType, null);
                }
                double adjustFactorCriticalEast_4 = SetLoadExtension.GetHeightVariationCoefficientOfWindPressure(doc, framings[criticalEast][3], groundRoughness);

                //赋予荷载工况 
                WYCriticalEast_4[0].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYPlusLoadCase_0.Id);
                WYCriticalEast_4[1].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYPlusLoadCase_1.Id);
                WYCriticalEast_4[2].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYMinusLoadCase_0.Id);
                WYCriticalEast_4[3].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYMinusLoadCase_1.Id);
                //附加说明
                WYCriticalEast_4[0].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[criticalEast][3].Id.ToString());
                WYCriticalEast_4[1].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[criticalEast][3].Id.ToString());
                WYCriticalEast_4[2].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[criticalEast][3].Id.ToString());
                WYCriticalEast_4[3].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[criticalEast][3].Id.ToString());

                WYCriticalEast_4[0].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                WYCriticalEast_4[1].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                WYCriticalEast_4[2].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                WYCriticalEast_4[3].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");

                double tempVal4 = UnitExtension.ConvertToMillimeters((framings[criticalEast][0].Location as LocationPoint).Point.DistanceTo((framings[framings.Count - 1][0].Location as LocationPoint).Point));
                if (2 * a - tempVal4 < 0)//此时说明2a这条分界线在临界柱的右侧
                {
                    //更改1柱荷载大小
                    WYCriticalEast_1[0].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).Set((disCriticalEastXMinus / 2 + (tempVal4 - 2 * a)) * factor1_H_0 * basicWindPressure * adjustFactorCriticalEast_1 * adjustFactor + (disCriticalEastXPlus / 2 - (tempVal4 - 2 * a)) * factor1E_H_0 * basicWindPressure * adjustFactorCriticalEast_1 * adjustFactor);
                    WYCriticalEast_1[1].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).Set((disCriticalEastXMinus / 2 + (tempVal4 - 2 * a)) * factor1_H_1 * basicWindPressure * adjustFactorCriticalEast_1 * adjustFactor + (disCriticalEastXPlus / 2 - (tempVal4 - 2 * a)) * factor1E_H_1 * basicWindPressure * adjustFactorCriticalEast_1 * adjustFactor);
                    WYCriticalEast_1[2].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).Set((disCriticalEastXMinus / 2 + (tempVal4 - 2 * a)) * factor4_H_0 * basicWindPressure * adjustFactorCriticalEast_1 * adjustFactor + (disCriticalEastXPlus / 2 - (tempVal4 - 2 * a)) * factor4E_H_0 * basicWindPressure * adjustFactorCriticalEast_1 * adjustFactor);
                    WYCriticalEast_1[3].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).Set((disCriticalEastXMinus / 2 + (tempVal4 - 2 * a)) * factor4_H_1 * basicWindPressure * adjustFactorCriticalEast_1 * adjustFactor + (disCriticalEastXPlus / 2 - (tempVal4 - 2 * a)) * factor4E_H_1 * basicWindPressure * adjustFactorCriticalEast_1 * adjustFactor);
                    //更改2梁荷载大小
                    WYCriticalEast_2[0].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set((disCriticalEastXMinus / 2 + (tempVal4 - 2 * a)) * -factor2_H_0 * basicWindPressure * adjustFactorCriticalEast_2 * adjustFactor + (disCriticalEastXPlus / 2 - (tempVal4 - 2 * a)) * -factor2E_H_0 * basicWindPressure * adjustFactorCriticalEast_2 * adjustFactor);
                    WYCriticalEast_2[1].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set((disCriticalEastXMinus / 2 + (tempVal4 - 2 * a)) * -factor2_H_1 * basicWindPressure * adjustFactorCriticalEast_2 * adjustFactor + (disCriticalEastXPlus / 2 - (tempVal4 - 2 * a)) * -factor2E_H_1 * basicWindPressure * adjustFactorCriticalEast_2 * adjustFactor);
                    WYCriticalEast_2[2].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set((disCriticalEastXMinus / 2 + (tempVal4 - 2 * a)) * -factor3_H_0 * basicWindPressure * adjustFactorCriticalEast_2 * adjustFactor + (disCriticalEastXPlus / 2 - (tempVal4 - 2 * a)) * -factor3E_H_0 * basicWindPressure * adjustFactorCriticalEast_2 * adjustFactor);
                    WYCriticalEast_2[3].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set((disCriticalEastXMinus / 2 + (tempVal4 - 2 * a)) * -factor3_H_1 * basicWindPressure * adjustFactorCriticalEast_2 * adjustFactor + (disCriticalEastXPlus / 2 - (tempVal4 - 2 * a)) * -factor3E_H_1 * basicWindPressure * adjustFactorCriticalEast_2 * adjustFactor);
                    //更改3梁荷载大小
                    WYCriticalEast_3[0].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set((disCriticalEastXMinus / 2 + (tempVal4 - 2 * a)) * -factor3_H_0 * basicWindPressure * adjustFactorCriticalEast_3 * adjustFactor + (disCriticalEastXPlus / 2 - (tempVal4 - 2 * a)) * -factor3E_H_0 * basicWindPressure * adjustFactorCriticalEast_3 * adjustFactor);
                    WYCriticalEast_3[1].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set((disCriticalEastXMinus / 2 + (tempVal4 - 2 * a)) * -factor3_H_1 * basicWindPressure * adjustFactorCriticalEast_3 * adjustFactor + (disCriticalEastXPlus / 2 - (tempVal4 - 2 * a)) * -factor3E_H_1 * basicWindPressure * adjustFactorCriticalEast_3 * adjustFactor);
                    WYCriticalEast_3[2].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set((disCriticalEastXMinus / 2 + (tempVal4 - 2 * a)) * -factor2_H_0 * basicWindPressure * adjustFactorCriticalEast_3 * adjustFactor + (disCriticalEastXPlus / 2 - (tempVal4 - 2 * a)) * -factor2E_H_0 * basicWindPressure * adjustFactorCriticalEast_3 * adjustFactor);
                    WYCriticalEast_3[3].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set((disCriticalEastXMinus / 2 + (tempVal4 - 2 * a)) * -factor2_H_1 * basicWindPressure * adjustFactorCriticalEast_3 * adjustFactor + (disCriticalEastXPlus / 2 - (tempVal4 - 2 * a)) * -factor2E_H_1 * basicWindPressure * adjustFactorCriticalEast_3 * adjustFactor);
                    //更改4柱荷载大小
                    WYCriticalEast_4[0].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).Set((disCriticalEastXMinus / 2 + (tempVal4 - 2 * a)) * -factor4_H_0 * basicWindPressure * adjustFactorCriticalEast_4 * adjustFactor + (disCriticalEastXPlus / 2 - (tempVal4 - 2 * a)) * -factor4E_H_0 * basicWindPressure * adjustFactorCriticalEast_4 * adjustFactor);
                    WYCriticalEast_4[1].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).Set((disCriticalEastXMinus / 2 + (tempVal4 - 2 * a)) * -factor4_H_1 * basicWindPressure * adjustFactorCriticalEast_4 * adjustFactor + (disCriticalEastXPlus / 2 - (tempVal4 - 2 * a)) * -factor4E_H_1 * basicWindPressure * adjustFactorCriticalEast_4 * adjustFactor);
                    WYCriticalEast_4[2].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).Set((disCriticalEastXMinus / 2 + (tempVal4 - 2 * a)) * -factor1_H_0 * basicWindPressure * adjustFactorCriticalEast_4 * adjustFactor + (disCriticalEastXPlus / 2 - (tempVal4 - 2 * a)) * -factor1E_H_0 * basicWindPressure * adjustFactorCriticalEast_4 * adjustFactor);
                    WYCriticalEast_4[3].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).Set((disCriticalEastXMinus / 2 + (tempVal4 - 2 * a)) * -factor1_H_1 * basicWindPressure * adjustFactorCriticalEast_4 * adjustFactor + (disCriticalEastXPlus / 2 - (tempVal4 - 2 * a)) * -factor1E_H_1 * basicWindPressure * adjustFactorCriticalEast_4 * adjustFactor);
                }
                else//说明2a这条分界线在临界柱的左侧
                {
                    //更改1柱荷载大小
                    WYCriticalEast_1[0].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).Set((disCriticalEastXMinus / 2 - (2 * a - tempVal4)) * factor1_H_0 * basicWindPressure * adjustFactorCriticalEast_1 * adjustFactor + (disCriticalEastXPlus / 2 + (2 * a - tempVal4)) * factor1E_H_0 * basicWindPressure * adjustFactorCriticalEast_1 * adjustFactor);
                    WYCriticalEast_1[1].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).Set((disCriticalEastXMinus / 2 - (2 * a - tempVal4)) * factor1_H_1 * basicWindPressure * adjustFactorCriticalEast_1 * adjustFactor + (disCriticalEastXPlus / 2 + (2 * a - tempVal4)) * factor1E_H_1 * basicWindPressure * adjustFactorCriticalEast_1 * adjustFactor);
                    WYCriticalEast_1[2].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).Set((disCriticalEastXMinus / 2 - (2 * a - tempVal4)) * factor4_H_0 * basicWindPressure * adjustFactorCriticalEast_1 * adjustFactor + (disCriticalEastXPlus / 2 + (2 * a - tempVal4)) * factor4E_H_0 * basicWindPressure * adjustFactorCriticalEast_1 * adjustFactor);
                    WYCriticalEast_1[3].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).Set((disCriticalEastXMinus / 2 - (2 * a - tempVal4)) * factor4_H_1 * basicWindPressure * adjustFactorCriticalEast_1 * adjustFactor + (disCriticalEastXPlus / 2 + (2 * a - tempVal4)) * factor4E_H_1 * basicWindPressure * adjustFactorCriticalEast_1 * adjustFactor);
                    //更改2梁荷载大小
                    WYCriticalEast_2[0].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set((disCriticalEastXMinus / 2 - (2 * a - tempVal4)) * -factor2_H_0 * basicWindPressure * adjustFactorCriticalEast_2 * adjustFactor + (disCriticalEastXPlus / 2 + (2 * a - tempVal4)) * -factor2E_H_0 * basicWindPressure * adjustFactorCriticalEast_2 * adjustFactor);
                    WYCriticalEast_2[1].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set((disCriticalEastXMinus / 2 - (2 * a - tempVal4)) * -factor2_H_1 * basicWindPressure * adjustFactorCriticalEast_2 * adjustFactor + (disCriticalEastXPlus / 2 + (2 * a - tempVal4)) * -factor2E_H_1 * basicWindPressure * adjustFactorCriticalEast_2 * adjustFactor);
                    WYCriticalEast_2[2].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set((disCriticalEastXMinus / 2 - (2 * a - tempVal4)) * -factor3_H_0 * basicWindPressure * adjustFactorCriticalEast_2 * adjustFactor + (disCriticalEastXPlus / 2 + (2 * a - tempVal4)) * -factor3E_H_0 * basicWindPressure * adjustFactorCriticalEast_2 * adjustFactor);
                    WYCriticalEast_2[3].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set((disCriticalEastXMinus / 2 - (2 * a - tempVal4)) * -factor3_H_1 * basicWindPressure * adjustFactorCriticalEast_2 * adjustFactor + (disCriticalEastXPlus / 2 + (2 * a - tempVal4)) * -factor3E_H_1 * basicWindPressure * adjustFactorCriticalEast_2 * adjustFactor);
                    //更改3梁荷载大小
                    WYCriticalEast_3[0].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set((disCriticalEastXMinus / 2 - (2 * a - tempVal4)) * -factor3_H_0 * basicWindPressure * adjustFactorCriticalEast_3 * adjustFactor + (disCriticalEastXPlus / 2 + (2 * a - tempVal4)) * -factor3E_H_0 * basicWindPressure * adjustFactorCriticalEast_3 * adjustFactor);
                    WYCriticalEast_3[1].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set((disCriticalEastXMinus / 2 - (2 * a - tempVal4)) * -factor3_H_1 * basicWindPressure * adjustFactorCriticalEast_3 * adjustFactor + (disCriticalEastXPlus / 2 + (2 * a - tempVal4)) * -factor3E_H_1 * basicWindPressure * adjustFactorCriticalEast_3 * adjustFactor);
                    WYCriticalEast_3[2].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set((disCriticalEastXMinus / 2 - (2 * a - tempVal4)) * -factor2_H_0 * basicWindPressure * adjustFactorCriticalEast_3 * adjustFactor + (disCriticalEastXPlus / 2 + (2 * a - tempVal4)) * -factor2E_H_0 * basicWindPressure * adjustFactorCriticalEast_3 * adjustFactor);
                    WYCriticalEast_3[3].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set((disCriticalEastXMinus / 2 - (2 * a - tempVal4)) * -factor2_H_1 * basicWindPressure * adjustFactorCriticalEast_3 * adjustFactor + (disCriticalEastXPlus / 2 + (2 * a - tempVal4)) * -factor2E_H_1 * basicWindPressure * adjustFactorCriticalEast_3 * adjustFactor);
                    //更改4柱荷载大小
                    WYCriticalEast_4[0].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).Set((disCriticalEastXMinus / 2 - (2 * a - tempVal4)) * -factor4_H_0 * basicWindPressure * adjustFactorCriticalEast_4 * adjustFactor + (disCriticalEastXPlus / 2 + (2 * a - tempVal4)) * -factor4E_H_0 * basicWindPressure * adjustFactorCriticalEast_4 * adjustFactor);
                    WYCriticalEast_4[1].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).Set((disCriticalEastXMinus / 2 - (2 * a - tempVal4)) * -factor4_H_1 * basicWindPressure * adjustFactorCriticalEast_4 * adjustFactor + (disCriticalEastXPlus / 2 + (2 * a - tempVal4)) * -factor4E_H_1 * basicWindPressure * adjustFactorCriticalEast_4 * adjustFactor);
                    WYCriticalEast_4[2].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).Set((disCriticalEastXMinus / 2 - (2 * a - tempVal4)) * -factor1_H_0 * basicWindPressure * adjustFactorCriticalEast_4 * adjustFactor + (disCriticalEastXPlus / 2 + (2 * a - tempVal4)) * -factor1E_H_0 * basicWindPressure * adjustFactorCriticalEast_4 * adjustFactor);
                    WYCriticalEast_4[3].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).Set((disCriticalEastXMinus / 2 - (2 * a - tempVal4)) * -factor1_H_1 * basicWindPressure * adjustFactorCriticalEast_4 * adjustFactor + (disCriticalEastXPlus / 2 + (2 * a - tempVal4)) * -factor1E_H_1 * basicWindPressure * adjustFactorCriticalEast_4 * adjustFactor);

                }

                #endregion

                #region 中间跨
                for (int i = startMiddle; i <= endMiddle; i++)
                {
                    //1柱
                    //找X负向的最近柱并计算距离
                    double disMiddleXMinus = SetLoadExtension.CalculateNearestDistance(doc, framings[i][0], -XYZ.BasisX, referenceIntersectorColumn);
                    //找X正向的最近柱并计算距离
                    double disMiddleXPlus = SetLoadExtension.CalculateNearestDistance(doc, framings[i][0], XYZ.BasisX, referenceIntersectorColumn);
                    double disMiddle = disMiddleXPlus / 2 + disMiddleXMinus / 2;
                    XYZ startMiddle1 = (framings[i][0].Location as LocationPoint).Point;
                    double heightMiddle1 = (doc.GetElement(framings[i][0].get_Parameter(BuiltInParameter.SCHEDULE_TOP_LEVEL_PARAM).AsElementId()) as Level).Elevation;
                    XYZ endMiddle1 = (framings[i][0].Location as LocationPoint).Point + new XYZ(0, 0, heightMiddle1);
                    LineLoad[] WYMiddle_1 = new LineLoad[4];
                    for (int j = 0; j < 4; j++)
                    {
                        WYMiddle_1[j]= LineLoad.Create(doc, startMiddle1, endMiddle1, XYZ.BasisY, XYZ.Zero, lineLoadType, null);
                    }
                    double adjustFactorMiddle_1 = SetLoadExtension.GetHeightVariationCoefficientOfWindPressure(doc, framings[i][0], groundRoughness);
                    //更改荷载大小
                    WYMiddle_1[0].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).Set(disMiddle * factor1_H_0 * basicWindPressure * adjustFactorMiddle_1 * adjustFactor);
                    WYMiddle_1[1].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).Set(disMiddle * factor1_H_1 * basicWindPressure * adjustFactorMiddle_1 * adjustFactor);
                    WYMiddle_1[2].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).Set(disMiddle * factor4_H_0 * basicWindPressure * adjustFactorMiddle_1 * adjustFactor);
                    WYMiddle_1[3].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).Set(disMiddle * factor4_H_1 * basicWindPressure * adjustFactorMiddle_1 * adjustFactor);
                    //赋予荷载工况 
                    WYMiddle_1[0].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYPlusLoadCase_0.Id);
                    WYMiddle_1[1].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYPlusLoadCase_1.Id);
                    WYMiddle_1[2].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYMinusLoadCase_0.Id);
                    WYMiddle_1[3].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYMinusLoadCase_1.Id);
                    //附加说明
                    WYMiddle_1[0].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][0].Id.ToString());
                    WYMiddle_1[1].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][0].Id.ToString());
                    WYMiddle_1[2].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][0].Id.ToString());
                    WYMiddle_1[3].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][0].Id.ToString());

                    WYMiddle_1[0].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WYMiddle_1[1].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WYMiddle_1[2].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WYMiddle_1[3].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    //2梁
                    XYZ startMiddle2 = ((framings[i][1].Location as LocationCurve).Curve as Line).Origin;
                    double lengthMiddle2 = ((framings[i][1].Location as LocationCurve).Curve as Line).Length;
                    XYZ directionMiddle2 = ((framings[i][1].Location as LocationCurve).Curve as Line).Direction;
                    XYZ endMiddle2 = new XYZ(startMiddle2.X + lengthMiddle2 * directionMiddle2.X / Math.Sqrt(directionMiddle2.X * directionMiddle2.X + directionMiddle2.Y * directionMiddle2.Y + directionMiddle2.Z * directionMiddle2.Z),
                       startMiddle2.Y + lengthMiddle2 * directionMiddle2.Y / Math.Sqrt(directionMiddle2.X * directionMiddle2.X + directionMiddle2.Y * directionMiddle2.Y + directionMiddle2.Z * directionMiddle2.Z),
                       startMiddle2.Z + lengthMiddle2 * directionMiddle2.Z / Math.Sqrt(directionMiddle2.X * directionMiddle2.X + directionMiddle2.Y * directionMiddle2.Y + directionMiddle2.Z * directionMiddle2.Z));               
                    LineLoad[] WYMiddle_2 = new LineLoad[4];
                    for (int j = 0; j < 4; j++)
                    {
                        WYMiddle_2[j]= LineLoad.Create(doc, startMiddle2, endMiddle2, XYZ.BasisZ, XYZ.Zero, lineLoadType, null);
                    }
                    double adjustFactorMiddle_2 = SetLoadExtension.GetHeightVariationCoefficientOfWindPressure(doc, framings[i][1], groundRoughness);

                    //更改荷载大小
                    WYMiddle_2[0].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set(disMiddle * -factor2_H_0 * basicWindPressure * adjustFactorMiddle_2 * adjustFactor);
                    WYMiddle_2[1].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set(disMiddle * -factor2_H_1 * basicWindPressure * adjustFactorMiddle_2 * adjustFactor);
                    WYMiddle_2[2].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set(disMiddle * -factor3_H_0 * basicWindPressure * adjustFactorMiddle_2 * adjustFactor);
                    WYMiddle_2[3].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set(disMiddle * -factor3_H_1 * basicWindPressure * adjustFactorMiddle_2 * adjustFactor);

                    //赋予荷载工况 
                    WYMiddle_2[0].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYPlusLoadCase_0.Id);
                    WYMiddle_2[1].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYPlusLoadCase_1.Id);
                    WYMiddle_2[2].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYMinusLoadCase_0.Id);
                    WYMiddle_2[3].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYMinusLoadCase_1.Id);
                    //附加说明
                    WYMiddle_2[0].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][1].Id.ToString());
                    WYMiddle_2[1].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][1].Id.ToString());
                    WYMiddle_2[2].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][1].Id.ToString());
                    WYMiddle_2[3].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][1].Id.ToString());

                    WYMiddle_2[0].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WYMiddle_2[1].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WYMiddle_2[2].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WYMiddle_2[3].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    //3梁
                    XYZ startMiddle3 = ((framings[i][2].Location as LocationCurve).Curve as Line).Origin;
                    double lengthMiddle3 = ((framings[i][2].Location as LocationCurve).Curve as Line).Length;
                    XYZ directionMiddle3 = ((framings[i][2].Location as LocationCurve).Curve as Line).Direction;
                    XYZ endMiddle3 = new XYZ(startMiddle3.X + lengthMiddle3 * directionMiddle3.X / Math.Sqrt(directionMiddle3.X * directionMiddle3.X + directionMiddle3.Y * directionMiddle3.Y + directionMiddle3.Z * directionMiddle3.Z),
                                               startMiddle3.Y + lengthMiddle3 * directionMiddle3.Y / Math.Sqrt(directionMiddle3.X * directionMiddle3.X + directionMiddle3.Y * directionMiddle3.Y + directionMiddle3.Z * directionMiddle3.Z),
                                               startMiddle3.Z + lengthMiddle3 * directionMiddle3.Z / Math.Sqrt(directionMiddle3.X * directionMiddle3.X + directionMiddle3.Y * directionMiddle3.Y + directionMiddle3.Z * directionMiddle3.Z));
                  
                    LineLoad[] WYMiddle_3 = new LineLoad[4];
                    for (int j = 0; j < 4; j++)
                    {
                        WYMiddle_3[j] = LineLoad.Create(doc, startMiddle3, endMiddle3, XYZ.BasisZ, XYZ.Zero, lineLoadType, null);
                    }
                    double adjustFactorMiddle_3 = SetLoadExtension.GetHeightVariationCoefficientOfWindPressure(doc, framings[i][2], groundRoughness);

                    //更改荷载大小
                    WYMiddle_3[0].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set(disMiddle * -factor3_H_0 * basicWindPressure * adjustFactorMiddle_3 * adjustFactor);
                    WYMiddle_3[1].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set(disMiddle * -factor3_H_1 * basicWindPressure * adjustFactorMiddle_3 * adjustFactor);
                    WYMiddle_3[2].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set(disMiddle * -factor2_H_0 * basicWindPressure * adjustFactorMiddle_3 * adjustFactor);
                    WYMiddle_3[3].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set(disMiddle * -factor2_H_1 * basicWindPressure * adjustFactorMiddle_3 * adjustFactor);

                    //赋予荷载工况 
                    WYMiddle_3[0].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYPlusLoadCase_0.Id);
                    WYMiddle_3[1].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYPlusLoadCase_1.Id);
                    WYMiddle_3[2].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYMinusLoadCase_0.Id);
                    WYMiddle_3[3].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYMinusLoadCase_1.Id);
                    //附加说明
                    WYMiddle_3[0].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][2].Id.ToString());
                    WYMiddle_3[1].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][2].Id.ToString());
                    WYMiddle_3[2].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][2].Id.ToString());
                    WYMiddle_3[3].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][2].Id.ToString());

                    WYMiddle_3[0].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WYMiddle_3[1].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WYMiddle_3[2].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WYMiddle_3[3].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    //4柱
                    XYZ startMiddle4 = (framings[i][3].Location as LocationPoint).Point;
                    double heightMiddle4 = (doc.GetElement(framings[i][3].get_Parameter(BuiltInParameter.SCHEDULE_TOP_LEVEL_PARAM).AsElementId()) as Level).Elevation;
                    XYZ endMiddle4 = (framings[i][3].Location as LocationPoint).Point + new XYZ(0, 0, heightMiddle4);
                  
                    LineLoad[] WYMiddle_4 = new LineLoad[4];
                    for (int j = 0; j < 4; j++)
                    {
                        WYMiddle_4[j] = LineLoad.Create(doc, startMiddle4, endMiddle4, XYZ.BasisY, XYZ.Zero, lineLoadType, null);
                    }
                    double adjustFactorMiddle_4 = SetLoadExtension.GetHeightVariationCoefficientOfWindPressure(doc, framings[i][3], groundRoughness);

                    //更改荷载大小
                    WYMiddle_4[0].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).Set(disMiddle * -factor4_H_0 * basicWindPressure * adjustFactorMiddle_4 * adjustFactor);
                    WYMiddle_4[1].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).Set(disMiddle * -factor4_H_1 * basicWindPressure * adjustFactorMiddle_4 * adjustFactor);
                    WYMiddle_4[2].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).Set(disMiddle * -factor1_H_0 * basicWindPressure * adjustFactorMiddle_4 * adjustFactor);
                    WYMiddle_4[3].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).Set(disMiddle * -factor1_H_1 * basicWindPressure * adjustFactorMiddle_4 * adjustFactor);
                    //赋予荷载工况 
                    WYMiddle_4[0].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYPlusLoadCase_0.Id);
                    WYMiddle_4[1].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYPlusLoadCase_1.Id);
                    WYMiddle_4[2].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYMinusLoadCase_0.Id);
                    WYMiddle_4[3].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYMinusLoadCase_1.Id);
                    //附加说明
                    WYMiddle_4[0].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][3].Id.ToString());
                    WYMiddle_4[1].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][3].Id.ToString());
                    WYMiddle_4[2].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][3].Id.ToString());
                    WYMiddle_4[3].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][3].Id.ToString());

                    WYMiddle_4[0].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WYMiddle_4[1].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WYMiddle_4[2].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WYMiddle_4[3].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");

                }
                #endregion

                #region 边跨（西）
                for (int i = 0; i < startMiddle - 1; i++)
                {
                    //1柱
                    //找X负向的最近柱并计算距离                   
                    double disEdgeWestXMinus = SetLoadExtension.CalculateNearestDistance(doc, framings[i][0], -XYZ.BasisX, referenceIntersectorColumn);
                    //找X正向的最近柱并计算距离  
                    double disEdgeWestXPlus = SetLoadExtension.CalculateNearestDistance(doc, framings[i][0], XYZ.BasisX, referenceIntersectorColumn);
                    double disEdgeWest = disEdgeWestXPlus / 2 + disEdgeWestXMinus / 2;
                    XYZ startEdgeWest1 = (framings[i][0].Location as LocationPoint).Point;
                    double heightEdgeWest1 = (doc.GetElement(framings[i][0].get_Parameter(BuiltInParameter.SCHEDULE_TOP_LEVEL_PARAM).AsElementId()) as Level).Elevation;
                    XYZ endEdgeWest1 = (framings[i][0].Location as LocationPoint).Point + new XYZ(0, 0, heightEdgeWest1);
                    LineLoad[] WYEdgeWest_1 = new LineLoad[4];
                    for (int j = 0; j < 4; j++)
                    {
                        WYEdgeWest_1[j]= LineLoad.Create(doc, startEdgeWest1, endEdgeWest1, XYZ.BasisY, XYZ.Zero, lineLoadType, null);
                    }
                    double adjustFactorEdgeWest_1= SetLoadExtension.GetHeightVariationCoefficientOfWindPressure(doc, framings[i][0], groundRoughness);
                    //更改荷载大小
                    WYEdgeWest_1[0].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).Set(disEdgeWest * factor1E_H_0 * basicWindPressure * adjustFactorEdgeWest_1 * adjustFactor);
                    WYEdgeWest_1[1].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).Set(disEdgeWest * factor1E_H_1 * basicWindPressure * adjustFactorEdgeWest_1 * adjustFactor);
                    WYEdgeWest_1[2].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).Set(disEdgeWest * factor4E_H_0 * basicWindPressure * adjustFactorEdgeWest_1 * adjustFactor);
                    WYEdgeWest_1[3].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).Set(disEdgeWest * factor4E_H_1 * basicWindPressure * adjustFactorEdgeWest_1 * adjustFactor);
                    //赋予荷载工况 
                    WYEdgeWest_1[0].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYPlusLoadCase_0.Id);
                    WYEdgeWest_1[1].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYPlusLoadCase_1.Id);
                    WYEdgeWest_1[2].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYMinusLoadCase_0.Id);
                    WYEdgeWest_1[3].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYMinusLoadCase_1.Id);
                    //附加说明
                    WYEdgeWest_1[0].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][0].Id.ToString());
                    WYEdgeWest_1[1].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][0].Id.ToString());
                    WYEdgeWest_1[2].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][0].Id.ToString());
                    WYEdgeWest_1[3].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][0].Id.ToString());

                    WYEdgeWest_1[0].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WYEdgeWest_1[1].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WYEdgeWest_1[2].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WYEdgeWest_1[3].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    //2梁
                    XYZ startEdgeWest2 = ((framings[i][1].Location as LocationCurve).Curve as Line).Origin;
                    double lengthEdgeWest2 = ((framings[i][1].Location as LocationCurve).Curve as Line).Length;
                    XYZ directionEdgeWest2 = ((framings[i][1].Location as LocationCurve).Curve as Line).Direction;
                    XYZ endEdgeWest2 = new XYZ(startEdgeWest2.X + lengthEdgeWest2 * directionEdgeWest2.X / Math.Sqrt(directionEdgeWest2.X * directionEdgeWest2.X + directionEdgeWest2.Y * directionEdgeWest2.Y + directionEdgeWest2.Z * directionEdgeWest2.Z),
                       startEdgeWest2.Y + lengthEdgeWest2 * directionEdgeWest2.Y / Math.Sqrt(directionEdgeWest2.X * directionEdgeWest2.X + directionEdgeWest2.Y * directionEdgeWest2.Y + directionEdgeWest2.Z * directionEdgeWest2.Z),
                       startEdgeWest2.Z + lengthEdgeWest2 * directionEdgeWest2.Z / Math.Sqrt(directionEdgeWest2.X * directionEdgeWest2.X + directionEdgeWest2.Y * directionEdgeWest2.Y + directionEdgeWest2.Z * directionEdgeWest2.Z));
                    LineLoad[] WYEdgeWest_2 = new LineLoad[4];

                    for (int j = 0; j < 4; j++)
                    {
                        WYEdgeWest_2[j]= LineLoad.Create(doc, startEdgeWest2, endEdgeWest2, XYZ.BasisZ, XYZ.Zero, lineLoadType, null);
                    }
                    double adjustFactorEdgeWest_2 = SetLoadExtension.GetHeightVariationCoefficientOfWindPressure(doc, framings[i][1], groundRoughness);

                    //更改荷载大小
                    WYEdgeWest_2[0].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set(disEdgeWest * -factor2E_H_0 * basicWindPressure * adjustFactorEdgeWest_2 * adjustFactor);
                    WYEdgeWest_2[1].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set(disEdgeWest * -factor2E_H_1 * basicWindPressure * adjustFactorEdgeWest_2 * adjustFactor);
                    WYEdgeWest_2[2].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set(disEdgeWest * -factor3E_H_0 * basicWindPressure * adjustFactorEdgeWest_2 * adjustFactor);
                    WYEdgeWest_2[3].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set(disEdgeWest * -factor3E_H_1 * basicWindPressure * adjustFactorEdgeWest_2 * adjustFactor);
                    //赋予荷载工况 
                    WYEdgeWest_2[0].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYPlusLoadCase_0.Id);
                    WYEdgeWest_2[1].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYPlusLoadCase_1.Id);
                    WYEdgeWest_2[2].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYMinusLoadCase_0.Id);
                    WYEdgeWest_2[3].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYMinusLoadCase_1.Id);
                    //附加说明
                    WYEdgeWest_2[0].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][1].Id.ToString());
                    WYEdgeWest_2[1].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][1].Id.ToString());
                    WYEdgeWest_2[2].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][1].Id.ToString());
                    WYEdgeWest_2[3].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][1].Id.ToString());

                    WYEdgeWest_2[0].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WYEdgeWest_2[1].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WYEdgeWest_2[2].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WYEdgeWest_2[3].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    //3梁
                    XYZ startEdgeWest3 = ((framings[i][2].Location as LocationCurve).Curve as Line).Origin;
                    double lengthEdgeWest3 = ((framings[i][2].Location as LocationCurve).Curve as Line).Length;
                    XYZ directionEdgeWest3 = ((framings[i][2].Location as LocationCurve).Curve as Line).Direction;
                    XYZ endEdgeWest3 = new XYZ(startEdgeWest3.X + lengthEdgeWest3 * directionEdgeWest3.X / Math.Sqrt(directionEdgeWest3.X * directionEdgeWest3.X + directionEdgeWest3.Y * directionEdgeWest3.Y + directionEdgeWest3.Z * directionEdgeWest3.Z),
                                               startEdgeWest3.Y + lengthEdgeWest3 * directionEdgeWest3.Y / Math.Sqrt(directionEdgeWest3.X * directionEdgeWest3.X + directionEdgeWest3.Y * directionEdgeWest3.Y + directionEdgeWest3.Z * directionEdgeWest3.Z),
                                               startEdgeWest3.Z + lengthEdgeWest3 * directionEdgeWest3.Z / Math.Sqrt(directionEdgeWest3.X * directionEdgeWest3.X + directionEdgeWest3.Y * directionEdgeWest3.Y + directionEdgeWest3.Z * directionEdgeWest3.Z));
                    LineLoad[] WYEdgeWest_3 = new LineLoad[4];
                    for (int j = 0; j < 4; j++)
                    {
                        WYEdgeWest_3[j] = LineLoad.Create(doc, startEdgeWest3, endEdgeWest3, XYZ.BasisZ, XYZ.Zero, lineLoadType, null);
                    }
                    double adjustFactorEdgeWest_3 = SetLoadExtension.GetHeightVariationCoefficientOfWindPressure(doc, framings[i][2], groundRoughness);

                    //更改荷载大小
                    WYEdgeWest_3[0].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set(disEdgeWest * -factor3E_H_0 * basicWindPressure * adjustFactorEdgeWest_3 * adjustFactor);
                    WYEdgeWest_3[1].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set(disEdgeWest * -factor3E_H_1 * basicWindPressure * adjustFactorEdgeWest_3 * adjustFactor);
                    WYEdgeWest_3[2].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set(disEdgeWest * -factor2E_H_0 * basicWindPressure * adjustFactorEdgeWest_3 * adjustFactor);
                    WYEdgeWest_3[3].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set(disEdgeWest * -factor2E_H_1 * basicWindPressure * adjustFactorEdgeWest_3 * adjustFactor);
                    //赋予荷载工况 
                    WYEdgeWest_3[0].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYPlusLoadCase_0.Id);
                    WYEdgeWest_3[1].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYPlusLoadCase_1.Id);
                    WYEdgeWest_3[2].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYMinusLoadCase_0.Id);
                    WYEdgeWest_3[3].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYMinusLoadCase_1.Id);
                    //附加说明
                    WYEdgeWest_3[0].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][2].Id.ToString());
                    WYEdgeWest_3[1].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][2].Id.ToString());
                    WYEdgeWest_3[2].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][2].Id.ToString());
                    WYEdgeWest_3[3].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][2].Id.ToString());

                    WYEdgeWest_3[0].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WYEdgeWest_3[1].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WYEdgeWest_3[2].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WYEdgeWest_3[3].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    //4柱
                    XYZ startEdgeWest4 = (framings[i][3].Location as LocationPoint).Point;
                    double heightEdgeWest4 = (doc.GetElement(framings[i][3].get_Parameter(BuiltInParameter.SCHEDULE_TOP_LEVEL_PARAM).AsElementId()) as Level).Elevation;
                    XYZ endEdgeWest4 = (framings[i][3].Location as LocationPoint).Point + new XYZ(0, 0, heightEdgeWest4);
                    LineLoad[] WYEdgeWest_4 = new LineLoad[4];
                    for (int j = 0; j < 4; j++)
                    {
                        WYEdgeWest_4[j] = LineLoad.Create(doc, startEdgeWest4, endEdgeWest4, XYZ.BasisY, XYZ.Zero, lineLoadType, null);
                    }
                    double adjustFactorEdgeWest_4 = SetLoadExtension.GetHeightVariationCoefficientOfWindPressure(doc, framings[i][3], groundRoughness);

                    //更改荷载大小
                    WYEdgeWest_4[0].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).Set(disEdgeWest * -factor4E_H_0 * basicWindPressure * adjustFactorEdgeWest_4 * adjustFactor);
                    WYEdgeWest_4[1].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).Set(disEdgeWest * -factor4E_H_1 * basicWindPressure * adjustFactorEdgeWest_4 * adjustFactor);
                    WYEdgeWest_4[2].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).Set(disEdgeWest * -factor1E_H_0 * basicWindPressure * adjustFactorEdgeWest_4 * adjustFactor);
                    WYEdgeWest_4[3].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).Set(disEdgeWest * -factor1E_H_1 * basicWindPressure * adjustFactorEdgeWest_4 * adjustFactor);
                    //赋予荷载工况 
                    WYEdgeWest_4[0].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYPlusLoadCase_0.Id);
                    WYEdgeWest_4[1].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYPlusLoadCase_1.Id);
                    WYEdgeWest_4[2].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYMinusLoadCase_0.Id);
                    WYEdgeWest_4[3].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYMinusLoadCase_1.Id);
                    //附加说明
                    WYEdgeWest_4[0].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][3].Id.ToString());
                    WYEdgeWest_4[1].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][3].Id.ToString());
                    WYEdgeWest_4[2].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][3].Id.ToString());
                    WYEdgeWest_4[3].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][3].Id.ToString());

                    WYEdgeWest_4[0].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WYEdgeWest_4[1].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WYEdgeWest_4[2].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WYEdgeWest_4[3].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                }
                #endregion

                #region 边跨（东）
                for (int i = endMiddle + 2; i < framings.Count; i++)
                {
                    //1柱
                    //找X负向的最近柱并计算距离
                    double disEdgeEastXMinus = SetLoadExtension.CalculateNearestDistance(doc, framings[i][0], -XYZ.BasisX, referenceIntersectorColumn);
                    //找X正向的最近柱并计算距离
                    double disEdgeEastXPlus = SetLoadExtension.CalculateNearestDistance(doc, framings[i][0], XYZ.BasisX, referenceIntersectorColumn);
                    double disEdgeEast = disEdgeEastXPlus / 2 + disEdgeEastXMinus / 2;
                    XYZ startEdgeEast1 = (framings[i][0].Location as LocationPoint).Point;
                    double heightEdgeEast1 = (doc.GetElement(framings[i][0].get_Parameter(BuiltInParameter.SCHEDULE_TOP_LEVEL_PARAM).AsElementId()) as Level).Elevation;
                    XYZ endEdgeEast1 = (framings[i][0].Location as LocationPoint).Point + new XYZ(0, 0, heightEdgeEast1);
                    LineLoad[] WYEdgeEast_1 = new LineLoad[4];
                    for (int j = 0; j < 4; j++)
                    {
                        WYEdgeEast_1[j]= LineLoad.Create(doc, startEdgeEast1, endEdgeEast1, XYZ.BasisY, XYZ.Zero, lineLoadType, null);
                    }
                    double adjustFactorEdgeEast_1 = SetLoadExtension.GetHeightVariationCoefficientOfWindPressure(doc, framings[i][0], groundRoughness);

                    //更改荷载大小
                    WYEdgeEast_1[0].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).Set(disEdgeEast * factor1E_H_0 * basicWindPressure * adjustFactorEdgeEast_1 * adjustFactor);
                    WYEdgeEast_1[1].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).Set(disEdgeEast * factor1E_H_1 * basicWindPressure * adjustFactorEdgeEast_1 * adjustFactor);
                    WYEdgeEast_1[2].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).Set(disEdgeEast * factor4E_H_0 * basicWindPressure * adjustFactorEdgeEast_1 * adjustFactor);
                    WYEdgeEast_1[3].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).Set(disEdgeEast * factor4E_H_1 * basicWindPressure * adjustFactorEdgeEast_1 * adjustFactor);
                    //赋予荷载工况 
                    WYEdgeEast_1[0].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYPlusLoadCase_0.Id);
                    WYEdgeEast_1[1].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYPlusLoadCase_1.Id);
                    WYEdgeEast_1[2].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYMinusLoadCase_0.Id);
                    WYEdgeEast_1[3].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYMinusLoadCase_1.Id);
                    //附加说明
                    WYEdgeEast_1[0].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][0].Id.ToString());
                    WYEdgeEast_1[1].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][0].Id.ToString());
                    WYEdgeEast_1[2].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][0].Id.ToString());
                    WYEdgeEast_1[3].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][0].Id.ToString());

                    WYEdgeEast_1[0].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WYEdgeEast_1[1].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WYEdgeEast_1[2].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WYEdgeEast_1[3].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    //2梁
                    XYZ startEdgeEast2 = ((framings[i][1].Location as LocationCurve).Curve as Line).Origin;
                    double lengthEdgeEast2 = ((framings[i][1].Location as LocationCurve).Curve as Line).Length;
                    XYZ directionEdgeEast2 = ((framings[i][1].Location as LocationCurve).Curve as Line).Direction;
                    XYZ endEdgeEast2 = new XYZ(startEdgeEast2.X + lengthEdgeEast2 * directionEdgeEast2.X / Math.Sqrt(directionEdgeEast2.X * directionEdgeEast2.X + directionEdgeEast2.Y * directionEdgeEast2.Y + directionEdgeEast2.Z * directionEdgeEast2.Z),
                       startEdgeEast2.Y + lengthEdgeEast2 * directionEdgeEast2.Y / Math.Sqrt(directionEdgeEast2.X * directionEdgeEast2.X + directionEdgeEast2.Y * directionEdgeEast2.Y + directionEdgeEast2.Z * directionEdgeEast2.Z),
                       startEdgeEast2.Z + lengthEdgeEast2 * directionEdgeEast2.Z / Math.Sqrt(directionEdgeEast2.X * directionEdgeEast2.X + directionEdgeEast2.Y * directionEdgeEast2.Y + directionEdgeEast2.Z * directionEdgeEast2.Z));
                    LineLoad[] WYEdgeEast_2 = new LineLoad[4];
                    for (int j = 0; j < 4; j++)
                    {
                        WYEdgeEast_2[j]= LineLoad.Create(doc, startEdgeEast2, endEdgeEast2, XYZ.BasisZ, XYZ.Zero, lineLoadType, null);
                    }
                    double adjustFactorEdgeEast_2 = SetLoadExtension.GetHeightVariationCoefficientOfWindPressure(doc, framings[i][1], groundRoughness);

                    //更改荷载大小
                    WYEdgeEast_2[0].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set(disEdgeEast * -factor2E_H_0 * basicWindPressure * adjustFactorEdgeEast_2 * adjustFactor);
                    WYEdgeEast_2[1].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set(disEdgeEast * -factor2E_H_1 * basicWindPressure * adjustFactorEdgeEast_2 * adjustFactor);
                    WYEdgeEast_2[2].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set(disEdgeEast * -factor3E_H_0 * basicWindPressure * adjustFactorEdgeEast_2 * adjustFactor);
                    WYEdgeEast_2[3].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set(disEdgeEast * -factor3E_H_1 * basicWindPressure * adjustFactorEdgeEast_2 * adjustFactor);
                    //赋予荷载工况 
                    WYEdgeEast_2[0].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYPlusLoadCase_0.Id);
                    WYEdgeEast_2[1].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYPlusLoadCase_1.Id);
                    WYEdgeEast_2[2].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYMinusLoadCase_0.Id);
                    WYEdgeEast_2[3].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYMinusLoadCase_1.Id);
                    //附加说明
                    WYEdgeEast_2[0].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][1].Id.ToString());
                    WYEdgeEast_2[1].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][1].Id.ToString());
                    WYEdgeEast_2[2].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][1].Id.ToString());
                    WYEdgeEast_2[3].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][1].Id.ToString());


                    WYEdgeEast_2[0].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WYEdgeEast_2[1].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WYEdgeEast_2[2].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WYEdgeEast_2[3].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    //3梁
                    XYZ startEdgeEast3 = ((framings[i][2].Location as LocationCurve).Curve as Line).Origin;
                    double lengthEdgeEast3 = ((framings[i][2].Location as LocationCurve).Curve as Line).Length;
                    XYZ directionEdgeEast3 = ((framings[i][2].Location as LocationCurve).Curve as Line).Direction;
                    XYZ endEdgeEast3 = new XYZ(startEdgeEast3.X + lengthEdgeEast3 * directionEdgeEast3.X / Math.Sqrt(directionEdgeEast3.X * directionEdgeEast3.X + directionEdgeEast3.Y * directionEdgeEast3.Y + directionEdgeEast3.Z * directionEdgeEast3.Z),
                                               startEdgeEast3.Y + lengthEdgeEast3 * directionEdgeEast3.Y / Math.Sqrt(directionEdgeEast3.X * directionEdgeEast3.X + directionEdgeEast3.Y * directionEdgeEast3.Y + directionEdgeEast3.Z * directionEdgeEast3.Z),
                                               startEdgeEast3.Z + lengthEdgeEast3 * directionEdgeEast3.Z / Math.Sqrt(directionEdgeEast3.X * directionEdgeEast3.X + directionEdgeEast3.Y * directionEdgeEast3.Y + directionEdgeEast3.Z * directionEdgeEast3.Z));
                    LineLoad[] WYEdgeEast_3 = new LineLoad[4];
                    for (int j = 0; j < 4; j++)
                    {
                        WYEdgeEast_3[j] = LineLoad.Create(doc, startEdgeEast3, endEdgeEast3, XYZ.BasisZ, XYZ.Zero, lineLoadType, null);
                    }
                    double adjustFactorEdgeEast_3 = SetLoadExtension.GetHeightVariationCoefficientOfWindPressure(doc, framings[i][2], groundRoughness);

                    //更改荷载大小
                    WYEdgeEast_3[0].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set(disEdgeEast * -factor3E_H_0 * basicWindPressure * adjustFactorEdgeEast_3 * adjustFactor);
                    WYEdgeEast_3[1].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set(disEdgeEast * -factor3E_H_1 * basicWindPressure * adjustFactorEdgeEast_3 * adjustFactor);
                    WYEdgeEast_3[2].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set(disEdgeEast * -factor2E_H_0 * basicWindPressure * adjustFactorEdgeEast_3 * adjustFactor);
                    WYEdgeEast_3[3].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set(disEdgeEast * -factor2E_H_1 * basicWindPressure * adjustFactorEdgeEast_3 * adjustFactor);
                    //赋予荷载工况 
                    WYEdgeEast_3[0].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYPlusLoadCase_0.Id);
                    WYEdgeEast_3[1].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYPlusLoadCase_1.Id);
                    WYEdgeEast_3[2].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYMinusLoadCase_0.Id);
                    WYEdgeEast_3[3].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYMinusLoadCase_1.Id);
                    //附加说明
                    WYEdgeEast_3[0].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][2].Id.ToString());
                    WYEdgeEast_3[1].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][2].Id.ToString());
                    WYEdgeEast_3[2].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][2].Id.ToString());
                    WYEdgeEast_3[3].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][2].Id.ToString());

                    WYEdgeEast_3[0].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WYEdgeEast_3[1].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WYEdgeEast_3[2].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WYEdgeEast_3[3].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    //4柱
                    XYZ startEdgeEast4 = (framings[i][3].Location as LocationPoint).Point;
                    double heightEdgeEast4 = (doc.GetElement(framings[i][3].get_Parameter(BuiltInParameter.SCHEDULE_TOP_LEVEL_PARAM).AsElementId()) as Level).Elevation;
                    XYZ endEdgeEast4 = (framings[i][3].Location as LocationPoint).Point + new XYZ(0, 0, heightEdgeEast4);
                    LineLoad[] WYEdgeEast_4 = new LineLoad[4];
                    for (int j = 0; j < 4; j++)
                    {
                        WYEdgeEast_4[j] = LineLoad.Create(doc, startEdgeEast4, endEdgeEast4, XYZ.BasisY, XYZ.Zero, lineLoadType, null);
                    }
                    double adjustFactorEdgeEast_4 = SetLoadExtension.GetHeightVariationCoefficientOfWindPressure(doc, framings[i][3], groundRoughness);

                    //更改荷载大小
                    WYEdgeEast_4[0].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).Set(disEdgeEast * -factor4E_H_0 * basicWindPressure * adjustFactorEdgeEast_4 * adjustFactor);
                    WYEdgeEast_4[1].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).Set(disEdgeEast * -factor4E_H_1 * basicWindPressure * adjustFactorEdgeEast_4 * adjustFactor);
                    WYEdgeEast_4[2].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).Set(disEdgeEast * -factor1E_H_0 * basicWindPressure * adjustFactorEdgeEast_4 * adjustFactor);
                    WYEdgeEast_4[3].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).Set(disEdgeEast * -factor1E_H_1 * basicWindPressure * adjustFactorEdgeEast_4 * adjustFactor);
                    //赋予荷载工况 
                    WYEdgeEast_4[0].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYPlusLoadCase_0.Id);
                    WYEdgeEast_4[1].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYPlusLoadCase_1.Id);
                    WYEdgeEast_4[2].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYMinusLoadCase_0.Id);
                    WYEdgeEast_4[3].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYMinusLoadCase_1.Id);
                    //附加说明
                    WYEdgeEast_4[0].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][3].Id.ToString());
                    WYEdgeEast_4[1].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][3].Id.ToString());
                    WYEdgeEast_4[2].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][3].Id.ToString());
                    WYEdgeEast_4[3].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][3].Id.ToString());

                    WYEdgeEast_4[0].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WYEdgeEast_4[1].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WYEdgeEast_4[2].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WYEdgeEast_4[3].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                }
                #endregion


                #endregion

                #region WY工况下山墙处各柱风荷载
                //WY+ 0.63
                List<FamilyInstance> westColumns = new List<FamilyInstance>();
                List<FamilyInstance> eastColumns = new List<FamilyInstance>();

                for (int i = 0; i < allColumns.Count; i++)
                {
                    BoundingBoxXYZ boxWestColumns = allColumns[i].get_BoundingBox(view3D);
                    XYZ centerWestColumns = boxWestColumns.Min.Add(boxWestColumns.Max).Multiply(0.5);
                    ReferenceIntersector referenceIntersectorWestColumns = new ReferenceIntersector(allColumnsIds, FindReferenceTarget.Element, view3D);
                    ReferenceWithContext referenceWithContextWestColumnsXMinus = referenceIntersectorWestColumns.FindNearest(centerWestColumns, -XYZ.BasisX);
                    if (referenceWithContextWestColumnsXMinus == null)
                    {
                        westColumns.Add(allColumns[i]);
                    }
                    ReferenceWithContext referenceWithContextWestColumnsXPlus = referenceIntersectorWestColumns.FindNearest(centerWestColumns, XYZ.BasisX);
                    if (referenceWithContextWestColumnsXPlus == null)
                    {
                        eastColumns.Add(allColumns[i]);
                    }
                }
                ReferenceIntersector referenceIntersectorAllColumns = new ReferenceIntersector(allColumnsIds, FindReferenceTarget.Element, view3D);
                for (int i = 0; i < westColumns.Count; i++)
                {
                    BoundingBoxXYZ boxWest = westColumns[i].get_BoundingBox(view3D);
                    XYZ centerWest = boxWest.Min.Add(boxWest.Max).Multiply(0.5);                
                    //找y负向的最近柱并计算距离
                    double disWestYMinus = SetLoadExtension.CalculateNearestDistance(doc, westColumns[i], -XYZ.BasisY, referenceIntersectorAllColumns);
                    //找y正向的最近柱并计算距离
                    double disWestYPlus = SetLoadExtension.CalculateNearestDistance(doc, westColumns[i], XYZ.BasisY, referenceIntersectorAllColumns);
                    double disWest = disWestYMinus / 2 + disWestYPlus / 2;
                    XYZ startWest = (westColumns[i].Location as LocationPoint).Point;
                    double heightWest = (doc.GetElement(westColumns[i].get_Parameter(BuiltInParameter.SCHEDULE_TOP_LEVEL_PARAM).AsElementId()) as Level).Elevation;
                    double offset = westColumns[i].get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM).AsDouble();
                    XYZ endWest = (westColumns[i].Location as LocationPoint).Point + new XYZ(0, 0, heightWest + offset);
                    LineLoad[] WYWest = new LineLoad[4];
                    for (int j = 0; j < 4; j++)
                    {
                        WYWest[j]= LineLoad.Create(doc, startWest, endWest, XYZ.BasisY, XYZ.Zero, lineLoadType, null);
                    }
                    double adjustFactorWest= SetLoadExtension.GetHeightVariationCoefficientOfWindPressure(doc, westColumns[i], groundRoughness);
                    //更改荷载大小
                    WYWest[0].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).Set(disWest * factor5And6_H_0 * basicWindPressure * adjustFactorWest * adjustFactor);
                    WYWest[1].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).Set(disWest * factor5And6_H_1 * basicWindPressure * adjustFactorWest * adjustFactor);
                    WYWest[2].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).Set(disWest * factor5And6_H_0 * basicWindPressure * adjustFactorWest * adjustFactor);
                    WYWest[3].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).Set(disWest * factor5And6_H_1 * basicWindPressure * adjustFactorWest * adjustFactor);
                    //赋予荷载工况 
                    WYWest[0].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYPlusLoadCase_0.Id);
                    WYWest[1].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYPlusLoadCase_1.Id);
                    WYWest[2].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYMinusLoadCase_0.Id);
                    WYWest[3].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYMinusLoadCase_1.Id);
                    //附加说明
                    WYWest[0].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + westColumns[i].Id.ToString());
                    WYWest[1].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + westColumns[i].Id.ToString());
                    WYWest[2].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + westColumns[i].Id.ToString());
                    WYWest[3].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + westColumns[i].Id.ToString());

                    WYWest[0].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WYWest[1].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WYWest[2].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WYWest[3].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");

                }
                for (int i = 0; i < eastColumns.Count; i++)
                {

                    //找y负向的最近柱并计算距离
                    double disEastYMinus = SetLoadExtension.CalculateNearestDistance(doc, eastColumns[i], -XYZ.BasisY, referenceIntersectorAllColumns);
                    //找y正向的最近柱并计算距离
                    double disEastYPlus = SetLoadExtension.CalculateNearestDistance(doc, eastColumns[i], XYZ.BasisY, referenceIntersectorAllColumns);
                    double disEast = disEastYMinus / 2 + disEastYPlus / 2;
                    XYZ startEast = (eastColumns[i].Location as LocationPoint).Point;
                    double heightEast = (doc.GetElement(eastColumns[i].get_Parameter(BuiltInParameter.SCHEDULE_TOP_LEVEL_PARAM).AsElementId()) as Level).Elevation;
                    double offset = eastColumns[i].get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM).AsDouble();
                    XYZ endEast = (eastColumns[i].Location as LocationPoint).Point + new XYZ(0, 0, heightEast + offset);
                    LineLoad[] WYEast = new LineLoad[4];
                    for (int j = 0; j < 4; j++)
                    {
                        WYEast[j]= LineLoad.Create(doc, startEast, endEast, XYZ.BasisY, XYZ.Zero, lineLoadType, null);
                    }
                    double adjustFactortEast = SetLoadExtension.GetHeightVariationCoefficientOfWindPressure(doc, eastColumns[i], groundRoughness);

                    //更改荷载大小
                    WYEast[0].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).Set(disEast * -factor5And6_H_0 * basicWindPressure * adjustFactortEast * adjustFactor);
                    WYEast[1].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).Set(disEast * -factor5And6_H_1 * basicWindPressure * adjustFactortEast * adjustFactor);
                    WYEast[2].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).Set(disEast * -factor5And6_H_0 * basicWindPressure * adjustFactortEast * adjustFactor);
                    WYEast[3].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).Set(disEast * -factor5And6_H_1 * basicWindPressure * adjustFactortEast * adjustFactor);
                    //赋予荷载工况 
                    WYEast[0].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYPlusLoadCase_0.Id);
                    WYEast[1].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYPlusLoadCase_1.Id);
                    WYEast[2].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYMinusLoadCase_0.Id);
                    WYEast[3].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windYMinusLoadCase_1.Id);
                    //附加说明
                    WYEast[0].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + eastColumns[i].Id.ToString());
                    WYEast[1].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + eastColumns[i].Id.ToString());
                    WYEast[2].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + eastColumns[i].Id.ToString());
                    WYEast[3].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + eastColumns[i].Id.ToString());


                    WYEast[0].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WYEast[1].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WYEast[2].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WYEast[3].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                }
                #endregion



                //WX+ 0.43 -1.25 -0.71 -0.61          0.22  -0.87 -0.55 -0.47    -0.63
                #region WX工况侧墙各柱风荷载

                for (int i = 0; i < framings.Count; i++)
                {
                    
                    //找X负向的最近柱并计算距离
                    double disXMinus = SetLoadExtension.CalculateNearestDistance(doc, framings[i][0], -XYZ.BasisX, referenceIntersectorColumn);
                    //找X正向的最近柱并计算距离
                    double disXPlus = SetLoadExtension.CalculateNearestDistance(doc, framings[i][0], XYZ.BasisX, referenceIntersectorColumn);
                    double disColumn = disXPlus / 2 + disXMinus / 2;                   
                    //1柱
                    XYZ start1 = (framings[i][0].Location as LocationPoint).Point;
                    double height1 = (doc.GetElement(framings[i][0].get_Parameter(BuiltInParameter.SCHEDULE_TOP_LEVEL_PARAM).AsElementId()) as Level).Elevation;
                    XYZ end1 = (framings[i][0].Location as LocationPoint).Point + new XYZ(0, 0, height1);
                    LineLoad[] WXSouth_1 = new LineLoad[4];
                    for (int j = 0; j < 4; j++)
                    {
                        WXSouth_1[j]= LineLoad.Create(doc, start1, end1, XYZ.BasisY, XYZ.Zero, lineLoadType, null);
                    }
                    double adjustFactorSouth_1= SetLoadExtension.GetHeightVariationCoefficientOfWindPressure(doc, framings[i][0], groundRoughness);
                    //更改荷载大小
                    WXSouth_1[0].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).Set(disColumn * factor5And6_L_0 * basicWindPressure * adjustFactorSouth_1 * adjustFactor);
                    WXSouth_1[1].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).Set(disColumn * factor5And6_L_1 * basicWindPressure * adjustFactorSouth_1 * adjustFactor);
                    WXSouth_1[2].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).Set(disColumn * factor5And6_L_0 * basicWindPressure * adjustFactorSouth_1 * adjustFactor);
                    WXSouth_1[3].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).Set(disColumn * factor5And6_L_1 * basicWindPressure * adjustFactorSouth_1 * adjustFactor);
                    //赋予荷载工况 
                    WXSouth_1[0].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXPlusLoadCase_0.Id);
                    WXSouth_1[1].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXPlusLoadCase_1.Id);
                    WXSouth_1[2].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXMinusLoadCase_0.Id);
                    WXSouth_1[3].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXMinusLoadCase_1.Id);
                    //附加说明
                    WXSouth_1[0].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][0].Id.ToString());
                    WXSouth_1[1].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][0].Id.ToString());
                    WXSouth_1[2].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][0].Id.ToString());
                    WXSouth_1[3].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][0].Id.ToString());

                    WXSouth_1[0].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WXSouth_1[1].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WXSouth_1[2].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WXSouth_1[3].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");

                    //4柱
                    XYZ start4 = (framings[i][3].Location as LocationPoint).Point;
                    double height4 = (doc.GetElement(framings[i][3].get_Parameter(BuiltInParameter.SCHEDULE_TOP_LEVEL_PARAM).AsElementId()) as Level).Elevation;
                    XYZ end4 = (framings[i][3].Location as LocationPoint).Point + new XYZ(0, 0, height4);
                    LineLoad[] WXSouth_4 = new LineLoad[4];
                    for (int j = 0; j < 4; j++)
                    {
                        WXSouth_4[j]= LineLoad.Create(doc, start4, end4, XYZ.BasisY, XYZ.Zero, lineLoadType, null);
                    }
                    double adjustFactorSouth_4= SetLoadExtension.GetHeightVariationCoefficientOfWindPressure(doc, framings[i][3], groundRoughness);
                    //更改荷载大小
                    WXSouth_4[0].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).Set(disColumn * -factor5And6_L_0 * basicWindPressure * adjustFactorSouth_4 * adjustFactor);
                    WXSouth_4[1].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).Set(disColumn * -factor5And6_L_1 * basicWindPressure * adjustFactorSouth_4 * adjustFactor);
                    WXSouth_4[2].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).Set(disColumn * -factor5And6_L_0 * basicWindPressure * adjustFactorSouth_4 * adjustFactor);
                    WXSouth_4[3].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).Set(disColumn * -factor5And6_L_1 * basicWindPressure * adjustFactorSouth_4 * adjustFactor);
                    //赋予荷载工况 
                    WXSouth_4[0].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXPlusLoadCase_0.Id);
                    WXSouth_4[1].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXPlusLoadCase_1.Id);
                    WXSouth_4[2].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXMinusLoadCase_0.Id);
                    WXSouth_4[3].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXMinusLoadCase_1.Id);
                    //附加说明
                    WXSouth_4[0].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][3].Id.ToString());
                    WXSouth_4[1].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][3].Id.ToString());
                    WXSouth_4[2].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][3].Id.ToString());
                    WXSouth_4[3].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][3].Id.ToString());

                    WXSouth_4[0].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WXSouth_4[1].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WXSouth_4[2].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WXSouth_4[3].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                }



                #endregion



                int criticalMiddle = recordL2West;
                double tempVal5 = UnitExtension.ConvertToMillimeters((framings[recordL2West-1][0].Location as LocationPoint).Point.DistanceTo((framings[0][0].Location as LocationPoint).Point));
                double disCritical = UnitExtension.ConvertToMillimeters((framings[recordL2West - 1][0].Location as LocationPoint).Point.DistanceTo((framings[recordL2West][0].Location as LocationPoint).Point));
                if (xLength.Max() / 2 - tempVal5 < disCritical / 2)//此时说明L/2这条分界线靠近recordL2West-1侧，故第recordL2West-1跨为临界跨
                {
                    criticalMiddle = recordL2West - 1;
                }

                #region 临界跨梁
                double tempVal6 = UnitExtension.ConvertToMillimeters((framings[criticalMiddle][0].Location as LocationPoint).Point.DistanceTo((framings[0][0].Location as LocationPoint).Point));
                //找X负向的最近柱并计算距离     
                double disCriticalMiddleXMinus = SetLoadExtension.CalculateNearestDistance(doc, framings[criticalMiddle][0], -XYZ.BasisX, referenceIntersectorColumn);
                //找X正向的最近柱并计算距离
                double disCriticalMiddleXPlus = SetLoadExtension.CalculateNearestDistance(doc, framings[criticalMiddle][0], XYZ.BasisX, referenceIntersectorColumn);
   
                if (UnitExtension.ConvertToMillimeters(xLength.Max() / 2) - tempVal6 > 0)//此时说明L/2在临界跨右侧
                {
                    //2梁
                    XYZ start2 = ((framings[criticalMiddle][1].Location as LocationCurve).Curve as Line).Origin;
                    double length2 = ((framings[criticalMiddle][1].Location as LocationCurve).Curve as Line).Length;
                    XYZ direction2 = ((framings[criticalMiddle][1].Location as LocationCurve).Curve as Line).Direction;
                    XYZ end2 = new XYZ(start2.X + length2 * direction2.X / Math.Sqrt(direction2.X * direction2.X + direction2.Y * direction2.Y + direction2.Z * direction2.Z),
                       start2.Y + length2 * direction2.Y / Math.Sqrt(direction2.X * direction2.X + direction2.Y * direction2.Y + direction2.Z * direction2.Z),
                       start2.Z + length2 * direction2.Z / Math.Sqrt(direction2.X * direction2.X + direction2.Y * direction2.Y + direction2.Z * direction2.Z));

                    XYZ middle2 = start2 + (end2 - start2) * a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2);
                    LineLoad[] WXCriticalMiddle_2_1 = new LineLoad[4];
                    for (int i = 0; i < 4; i++)
                    {
                        WXCriticalMiddle_2_1[i]= LineLoad.Create(doc, start2, middle2, XYZ.BasisZ, XYZ.Zero, lineLoadType, null);
                    }
                    LineLoad[] WXCriticalMiddle_2_2 = new LineLoad[4];
                    for (int i = 0; i < 4; i++)
                    {
                        WXCriticalMiddle_2_2[i] = LineLoad.Create(doc, middle2, end2, XYZ.BasisZ, XYZ.Zero, lineLoadType, null);
                    }
                    double adjustFactorCritical_2= SetLoadExtension.GetHeightVariationCoefficientOfWindPressure(doc, framings[criticalMiddle][1], groundRoughness);
                    //更改荷载大小
                    WXCriticalMiddle_2_1[0].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set((UnitExtension.ConvertToMillimeters(xLength.Max() / 2) - tempVal6 + disCriticalMiddleXMinus / 2) * -factor2E_L_0 * basicWindPressure * adjustFactorCritical_2 * adjustFactor
                        + (disCriticalMiddleXPlus / 2 - UnitExtension.ConvertToMillimeters(xLength.Max() / 2) + tempVal6) * -factor3E_L_0 * basicWindPressure * adjustFactorCritical_2 * adjustFactor);
                    WXCriticalMiddle_2_1[1].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set((UnitExtension.ConvertToMillimeters(xLength.Max() / 2) - tempVal6 + disCriticalMiddleXMinus / 2) * -factor2E_L_1 * basicWindPressure * adjustFactorCritical_2 * adjustFactor
                        + (disCriticalMiddleXPlus / 2 - UnitExtension.ConvertToMillimeters(xLength.Max() / 2) + tempVal6) * -factor3E_L_1 * basicWindPressure * adjustFactorCritical_2 * adjustFactor);
                    WXCriticalMiddle_2_1[2].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set((UnitExtension.ConvertToMillimeters(xLength.Max() / 2) - tempVal6 + disCriticalMiddleXMinus / 2) * -factor3E_L_0 * basicWindPressure * adjustFactorCritical_2 * adjustFactor
                        + (disCriticalMiddleXPlus / 2 - UnitExtension.ConvertToMillimeters(xLength.Max() / 2) + tempVal6) * -factor2E_L_0 * basicWindPressure * adjustFactorCritical_2 * adjustFactor);
                    WXCriticalMiddle_2_1[3].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set((UnitExtension.ConvertToMillimeters(xLength.Max() / 2) - tempVal6 + disCriticalMiddleXMinus / 2) * -factor3E_L_1 * basicWindPressure * adjustFactorCritical_2 * adjustFactor
                        + (disCriticalMiddleXPlus / 2 - UnitExtension.ConvertToMillimeters(xLength.Max() / 2) + tempVal6) * -factor2E_L_1 * basicWindPressure * adjustFactorCritical_2 * adjustFactor);

                    WXCriticalMiddle_2_2[0].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set((UnitExtension.ConvertToMillimeters(xLength.Max() / 2) - tempVal6 + disCriticalMiddleXMinus / 2) * -factor2_L_0 * basicWindPressure * adjustFactorCritical_2 * adjustFactor
                        + (disCriticalMiddleXPlus / 2 - UnitExtension.ConvertToMillimeters(xLength.Max() / 2) + tempVal6) * -factor3_L_0 * basicWindPressure * adjustFactorCritical_2 * adjustFactor);
                    WXCriticalMiddle_2_2[1].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set((UnitExtension.ConvertToMillimeters(xLength.Max() / 2) - tempVal6 + disCriticalMiddleXMinus / 2) * -factor2_L_1 * basicWindPressure * adjustFactorCritical_2 * adjustFactor
                        + (disCriticalMiddleXPlus / 2 - UnitExtension.ConvertToMillimeters(xLength.Max() / 2) + tempVal6) * -factor3_L_1 * basicWindPressure * adjustFactorCritical_2 * adjustFactor);
                    WXCriticalMiddle_2_2[2].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set((UnitExtension.ConvertToMillimeters(xLength.Max() / 2) - tempVal6 + disCriticalMiddleXMinus / 2) * -factor3_L_0 * basicWindPressure * adjustFactorCritical_2 * adjustFactor
                        + (disCriticalMiddleXPlus / 2 - UnitExtension.ConvertToMillimeters(xLength.Max() / 2) + tempVal6) * -factor2_L_0 * basicWindPressure * adjustFactorCritical_2 * adjustFactor);
                    WXCriticalMiddle_2_2[3].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set((UnitExtension.ConvertToMillimeters(xLength.Max() / 2) - tempVal6 + disCriticalMiddleXMinus / 2) * -factor3_L_1 * basicWindPressure * adjustFactorCritical_2 * adjustFactor
                        + (disCriticalMiddleXPlus / 2 - UnitExtension.ConvertToMillimeters(xLength.Max() / 2) + tempVal6) * -factor2_L_1 * basicWindPressure * adjustFactorCritical_2 * adjustFactor);
                    //赋予荷载工况 
                    WXCriticalMiddle_2_1[0].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXPlusLoadCase_0.Id);
                    WXCriticalMiddle_2_1[1].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXPlusLoadCase_1.Id);
                    WXCriticalMiddle_2_1[2].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXMinusLoadCase_0.Id);
                    WXCriticalMiddle_2_1[3].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXMinusLoadCase_1.Id);
                    
                    WXCriticalMiddle_2_2[0].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXPlusLoadCase_0.Id);
                    WXCriticalMiddle_2_2[1].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXPlusLoadCase_1.Id);
                    WXCriticalMiddle_2_2[2].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXMinusLoadCase_0.Id);
                    WXCriticalMiddle_2_2[3].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXMinusLoadCase_1.Id);

                    //附加说明
                    WXCriticalMiddle_2_1[0].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[criticalMiddle][1].Id.ToString());
                    WXCriticalMiddle_2_1[1].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[criticalMiddle][1].Id.ToString());
                    WXCriticalMiddle_2_1[2].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[criticalMiddle][1].Id.ToString());
                    WXCriticalMiddle_2_1[3].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[criticalMiddle][1].Id.ToString());


                    WXCriticalMiddle_2_1[0].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set($"Scope:(0,{(a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2)).ToString()})");
                    WXCriticalMiddle_2_1[1].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set($"Scope:(0,{(a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2)).ToString()})");
                    WXCriticalMiddle_2_1[2].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set($"Scope:(0,{(a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2)).ToString()})");
                    WXCriticalMiddle_2_1[3].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set($"Scope:(0,{(a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2)).ToString()})");

                    WXCriticalMiddle_2_2[0].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[criticalMiddle][1].Id.ToString());
                    WXCriticalMiddle_2_2[1].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[criticalMiddle][1].Id.ToString());
                    WXCriticalMiddle_2_2[2].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[criticalMiddle][1].Id.ToString());
                    WXCriticalMiddle_2_2[3].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[criticalMiddle][1].Id.ToString());

                    WXCriticalMiddle_2_2[0].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set($"Scope:({(a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2)).ToString()},1)");
                    WXCriticalMiddle_2_2[1].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set($"Scope:({(a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2)).ToString()},1)");
                    WXCriticalMiddle_2_2[2].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set($"Scope:({(a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2)).ToString()},1)");
                    WXCriticalMiddle_2_2[3].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set($"Scope:({(a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2)).ToString()},1)");


                    //3梁
                    XYZ start3 = ((framings[criticalMiddle][2].Location as LocationCurve).Curve as Line).Origin;
                    double length3 = ((framings[criticalMiddle][2].Location as LocationCurve).Curve as Line).Length;
                    XYZ direction3 = ((framings[criticalMiddle][2].Location as LocationCurve).Curve as Line).Direction;
                    XYZ end3 = new XYZ(start3.X + length3 * direction3.X / Math.Sqrt(direction3.X * direction3.X + direction3.Y * direction3.Y + direction3.Z * direction3.Z),
                                               start3.Y + length3 * direction3.Y / Math.Sqrt(direction3.X * direction3.X + direction3.Y * direction3.Y + direction3.Z * direction3.Z),
                                               start3.Z + length3 * direction3.Z / Math.Sqrt(direction3.X * direction3.X + direction3.Y * direction3.Y + direction3.Z * direction3.Z));
                    XYZ middle3 = start3 + (end3 - start3) * a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2);
                    LineLoad[] WXCriticalMiddle_3_1 = new LineLoad[4];
                    for (int i = 0; i < 4; i++)
                    {
                        WXCriticalMiddle_3_1[i]= LineLoad.Create(doc, start3, middle3, XYZ.BasisZ, XYZ.Zero, lineLoadType, null);
                    }
                    LineLoad[] WXCriticalMiddle_3_2=new LineLoad[4];
                    for (int i = 0; i < 4; i++)
                    {

                        WXCriticalMiddle_3_2[i]= LineLoad.Create(doc, middle3, end3, XYZ.BasisZ, XYZ.Zero, lineLoadType, null);
                    }
                    double adjustFactorCritical_3 = SetLoadExtension.GetHeightVariationCoefficientOfWindPressure(doc, framings[criticalMiddle][2], groundRoughness);

                    //更改荷载大小
                    WXCriticalMiddle_3_1[0].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set((UnitExtension.ConvertToMillimeters(xLength.Max() / 2) - tempVal6 + disCriticalMiddleXMinus / 2) * -factor2E_L_0 * basicWindPressure * adjustFactorCritical_3 * adjustFactor
                        + (disCriticalMiddleXPlus / 2 - UnitExtension.ConvertToMillimeters(xLength.Max() / 2) + tempVal6) * -factor3E_L_0 * basicWindPressure * adjustFactorCritical_3 * adjustFactor);
                    WXCriticalMiddle_3_1[1].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set((UnitExtension.ConvertToMillimeters(xLength.Max() / 2) - tempVal6 + disCriticalMiddleXMinus / 2) * -factor2E_L_1 * basicWindPressure * adjustFactorCritical_3 * adjustFactor
                       + (disCriticalMiddleXPlus / 2 - UnitExtension.ConvertToMillimeters(xLength.Max() / 2) + tempVal6) * -factor3E_L_1 * basicWindPressure * adjustFactorCritical_3 * adjustFactor);
                    WXCriticalMiddle_3_1[2].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set((UnitExtension.ConvertToMillimeters(xLength.Max() / 2) - tempVal6 + disCriticalMiddleXMinus / 2) * -factor3E_L_0 * basicWindPressure * adjustFactorCritical_3 * adjustFactor
                        + (disCriticalMiddleXPlus / 2 - UnitExtension.ConvertToMillimeters(xLength.Max() / 2) + tempVal6) * -factor2E_L_0 * basicWindPressure * adjustFactorCritical_3 * adjustFactor);
                    WXCriticalMiddle_3_1[3].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set((UnitExtension.ConvertToMillimeters(xLength.Max() / 2) - tempVal6 + disCriticalMiddleXMinus / 2) * -factor3E_L_1 * basicWindPressure * adjustFactorCritical_3 * adjustFactor
                        + (disCriticalMiddleXPlus / 2 - UnitExtension.ConvertToMillimeters(xLength.Max() / 2) + tempVal6) * -factor2E_L_1 * basicWindPressure * adjustFactorCritical_3 * adjustFactor);

                    WXCriticalMiddle_3_2[0].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set((UnitExtension.ConvertToMillimeters(xLength.Max() / 2) - tempVal6 + disCriticalMiddleXMinus / 2) * -factor2_L_0 * basicWindPressure * adjustFactorCritical_3 * adjustFactor
                        + (disCriticalMiddleXPlus / 2 - UnitExtension.ConvertToMillimeters(xLength.Max() / 2) + tempVal6) * -factor3_L_0 * basicWindPressure * adjustFactorCritical_3 * adjustFactor);
                    WXCriticalMiddle_3_2[1].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set((UnitExtension.ConvertToMillimeters(xLength.Max() / 2) - tempVal6 + disCriticalMiddleXMinus / 2) * -factor2_L_1 * basicWindPressure * adjustFactorCritical_3 * adjustFactor
                        + (disCriticalMiddleXPlus / 2 - UnitExtension.ConvertToMillimeters(xLength.Max() / 2) + tempVal6) * -factor3_L_1 * basicWindPressure * adjustFactorCritical_3 * adjustFactor); 
                    WXCriticalMiddle_3_2[2].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set((UnitExtension.ConvertToMillimeters(xLength.Max() / 2) - tempVal6 + disCriticalMiddleXMinus / 2) * -factor3_L_0 * basicWindPressure * adjustFactorCritical_3 * adjustFactor
                        + (disCriticalMiddleXPlus / 2 - UnitExtension.ConvertToMillimeters(xLength.Max() / 2) + tempVal6) * -factor2_L_0 * basicWindPressure * adjustFactorCritical_3 * adjustFactor); 
                    WXCriticalMiddle_3_2[3].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set((UnitExtension.ConvertToMillimeters(xLength.Max() / 2) - tempVal6 + disCriticalMiddleXMinus / 2) * -factor3_L_1 * basicWindPressure * adjustFactorCritical_3 * adjustFactor
                        + (disCriticalMiddleXPlus / 2 - UnitExtension.ConvertToMillimeters(xLength.Max() / 2) + tempVal6) * -factor2_L_1 * basicWindPressure * adjustFactorCritical_3 * adjustFactor);

                    //赋予荷载工况 
                    WXCriticalMiddle_3_1[0].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXPlusLoadCase_0.Id);
                    WXCriticalMiddle_3_1[1].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXPlusLoadCase_1.Id);
                    WXCriticalMiddle_3_1[2].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXMinusLoadCase_0.Id);
                    WXCriticalMiddle_3_1[3].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXMinusLoadCase_1.Id);

                    WXCriticalMiddle_3_2[0].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXPlusLoadCase_0.Id);
                    WXCriticalMiddle_3_2[1].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXPlusLoadCase_1.Id);
                    WXCriticalMiddle_3_2[2].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXMinusLoadCase_0.Id);
                    WXCriticalMiddle_3_2[3].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXMinusLoadCase_1.Id);
                    //附加说明
                    WXCriticalMiddle_3_1[0].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[criticalMiddle][2].Id.ToString());
                    WXCriticalMiddle_3_1[1].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[criticalMiddle][2].Id.ToString());
                    WXCriticalMiddle_3_1[2].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[criticalMiddle][2].Id.ToString());
                    WXCriticalMiddle_3_1[3].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[criticalMiddle][2].Id.ToString());

                    WXCriticalMiddle_3_1[0].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set($"Scope:(0,{(a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2)).ToString()})");
                    WXCriticalMiddle_3_1[1].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set($"Scope:(0,{(a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2)).ToString()})");
                    WXCriticalMiddle_3_1[2].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set($"Scope:(0,{(a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2)).ToString()})");
                    WXCriticalMiddle_3_1[3].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set($"Scope:(0,{(a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2)).ToString()})");

                    WXCriticalMiddle_3_2[0].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[criticalMiddle][2].Id.ToString());
                    WXCriticalMiddle_3_2[1].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[criticalMiddle][2].Id.ToString());
                    WXCriticalMiddle_3_2[2].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[criticalMiddle][2].Id.ToString());
                    WXCriticalMiddle_3_2[3].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[criticalMiddle][2].Id.ToString());

                    WXCriticalMiddle_3_2[0].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set($"Scope:({(a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2)).ToString()},1)");
                    WXCriticalMiddle_3_2[1].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set($"Scope:({(a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2)).ToString()},1)");
                    WXCriticalMiddle_3_2[2].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set($"Scope:({(a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2)).ToString()},1)");
                    WXCriticalMiddle_3_2[3].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set($"Scope:({(a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2)).ToString()},1)");
                }
                else//说明L/2在临界跨左侧
                {
                    //2梁
                    XYZ start2 = ((framings[criticalMiddle][1].Location as LocationCurve).Curve as Line).Origin;
                    double length2 = ((framings[criticalMiddle][1].Location as LocationCurve).Curve as Line).Length;
                    XYZ direction2 = ((framings[criticalMiddle][1].Location as LocationCurve).Curve as Line).Direction;
                    XYZ end2 = new XYZ(start2.X + length2 * direction2.X / Math.Sqrt(direction2.X * direction2.X + direction2.Y * direction2.Y + direction2.Z * direction2.Z),
                       start2.Y + length2 * direction2.Y / Math.Sqrt(direction2.X * direction2.X + direction2.Y * direction2.Y + direction2.Z * direction2.Z),
                       start2.Z + length2 * direction2.Z / Math.Sqrt(direction2.X * direction2.X + direction2.Y * direction2.Y + direction2.Z * direction2.Z));

                    XYZ middle2 = start2 + (end2 - start2) * a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2);
                    LineLoad[] WXCriticalMiddle_2_1 = new LineLoad[4];
                    for (int i = 0; i < 4; i++)
                    {
                        WXCriticalMiddle_2_1[i] = LineLoad.Create(doc, start2, middle2, XYZ.BasisZ, XYZ.Zero, lineLoadType, null);
                    }
                    LineLoad[] WXCriticalMiddle_2_2 = new LineLoad[4];
                    for (int i = 0; i < 4; i++)
                    {
                        WXCriticalMiddle_2_2[i] = LineLoad.Create(doc, middle2, end2, XYZ.BasisZ, XYZ.Zero, lineLoadType, null);
                    }
                    double adjustFactorCritical_2 = SetLoadExtension.GetHeightVariationCoefficientOfWindPressure(doc, framings[criticalMiddle][1], groundRoughness);
                    //更改荷载大小
                    WXCriticalMiddle_2_1[0].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set((disCriticalMiddleXMinus / 2 - tempVal6 + UnitExtension.ConvertToMillimeters(xLength.Max() / 2)) * -factor2E_L_0 * basicWindPressure * adjustFactorCritical_2 * adjustFactor
                        + (disCriticalMiddleXPlus / 2 + tempVal6 - UnitExtension.ConvertToMillimeters(xLength.Max() / 2)) * -factor3E_L_0 * basicWindPressure * adjustFactorCritical_2 * adjustFactor);
                    WXCriticalMiddle_2_1[1].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set((disCriticalMiddleXMinus / 2 - tempVal6 + UnitExtension.ConvertToMillimeters(xLength.Max() / 2)) * -factor2E_L_1 * basicWindPressure * adjustFactorCritical_2 * adjustFactor
                        + (disCriticalMiddleXPlus / 2 + tempVal6 - UnitExtension.ConvertToMillimeters(xLength.Max() / 2)) * -factor3E_L_1 * basicWindPressure * adjustFactorCritical_2 * adjustFactor); 
                    WXCriticalMiddle_2_1[2].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set((disCriticalMiddleXMinus / 2 - tempVal6 + UnitExtension.ConvertToMillimeters(xLength.Max() / 2)) * -factor3E_L_0 * basicWindPressure * adjustFactorCritical_2 * adjustFactor
                        + (disCriticalMiddleXPlus / 2 + tempVal6 - UnitExtension.ConvertToMillimeters(xLength.Max() / 2)) * -factor2E_L_0 * basicWindPressure * adjustFactorCritical_2 * adjustFactor);
                    WXCriticalMiddle_2_1[3].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set((disCriticalMiddleXMinus / 2 - tempVal6 + UnitExtension.ConvertToMillimeters(xLength.Max() / 2)) * -factor3E_L_1 * basicWindPressure * adjustFactorCritical_2 * adjustFactor
                        + (disCriticalMiddleXPlus / 2 + tempVal6 - UnitExtension.ConvertToMillimeters(xLength.Max() / 2)) * -factor2E_L_1 * basicWindPressure * adjustFactorCritical_2 * adjustFactor);
                    WXCriticalMiddle_2_2[0].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set((disCriticalMiddleXMinus / 2 - tempVal6 + UnitExtension.ConvertToMillimeters(xLength.Max() / 2)) * -factor2_L_0 * basicWindPressure * adjustFactorCritical_2 * adjustFactor
                        + (disCriticalMiddleXPlus / 2 + tempVal6 - UnitExtension.ConvertToMillimeters(xLength.Max() / 2)) * -factor3_L_0 * basicWindPressure * adjustFactorCritical_2 * adjustFactor); 
                    WXCriticalMiddle_2_2[1].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set((disCriticalMiddleXMinus / 2 - tempVal6 + UnitExtension.ConvertToMillimeters(xLength.Max() / 2)) * -factor2_L_1 * basicWindPressure * adjustFactorCritical_2 * adjustFactor
                        + (disCriticalMiddleXPlus / 2 + tempVal6 - UnitExtension.ConvertToMillimeters(xLength.Max() / 2)) * -factor3_L_1 * basicWindPressure * adjustFactorCritical_2 * adjustFactor); 
                    WXCriticalMiddle_2_2[2].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set((disCriticalMiddleXMinus / 2 - tempVal6 + UnitExtension.ConvertToMillimeters(xLength.Max() / 2)) * -factor3_L_0 * basicWindPressure * adjustFactorCritical_2 * adjustFactor
                        + (disCriticalMiddleXPlus / 2 + tempVal6 - UnitExtension.ConvertToMillimeters(xLength.Max() / 2)) * -factor2_L_0 * basicWindPressure * adjustFactorCritical_2 * adjustFactor); 
                    WXCriticalMiddle_2_2[3].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set((disCriticalMiddleXMinus / 2 - tempVal6 + UnitExtension.ConvertToMillimeters(xLength.Max() / 2)) * -factor3_L_1 * basicWindPressure * adjustFactorCritical_2 * adjustFactor
                        + (disCriticalMiddleXPlus / 2 + tempVal6 - UnitExtension.ConvertToMillimeters(xLength.Max() / 2)) * -factor2_L_1 * basicWindPressure * adjustFactorCritical_2 * adjustFactor);
                    //赋予荷载工况 
                    WXCriticalMiddle_2_1[0].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXPlusLoadCase_0.Id);
                    WXCriticalMiddle_2_1[1].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXPlusLoadCase_1.Id);
                    WXCriticalMiddle_2_1[2].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXMinusLoadCase_0.Id);
                    WXCriticalMiddle_2_1[3].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXMinusLoadCase_1.Id);

                    WXCriticalMiddle_2_2[0].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXPlusLoadCase_0.Id);
                    WXCriticalMiddle_2_2[1].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXPlusLoadCase_1.Id);
                    WXCriticalMiddle_2_2[2].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXMinusLoadCase_0.Id);
                    WXCriticalMiddle_2_2[3].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXMinusLoadCase_1.Id);
                    //附加说明
                    WXCriticalMiddle_2_1[0].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[criticalMiddle][1].Id.ToString());
                    WXCriticalMiddle_2_1[1].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[criticalMiddle][1].Id.ToString());
                    WXCriticalMiddle_2_1[2].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[criticalMiddle][1].Id.ToString());
                    WXCriticalMiddle_2_1[3].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[criticalMiddle][1].Id.ToString());

                    WXCriticalMiddle_2_1[0].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set($"Scope:(0,{(a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2)).ToString()})");
                    WXCriticalMiddle_2_1[1].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set($"Scope:(0,{(a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2)).ToString()})");
                    WXCriticalMiddle_2_1[2].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set($"Scope:(0,{(a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2)).ToString()})");
                    WXCriticalMiddle_2_1[3].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set($"Scope:(0,{(a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2)).ToString()})");

                    WXCriticalMiddle_2_2[0].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[criticalMiddle][1].Id.ToString());
                    WXCriticalMiddle_2_2[1].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[criticalMiddle][1].Id.ToString());
                    WXCriticalMiddle_2_2[2].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[criticalMiddle][1].Id.ToString());
                    WXCriticalMiddle_2_2[3].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[criticalMiddle][1].Id.ToString());

                    WXCriticalMiddle_2_2[0].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set($"Scope:({(a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2)).ToString()},1)");
                    WXCriticalMiddle_2_2[1].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set($"Scope:({(a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2)).ToString()},1)");
                    WXCriticalMiddle_2_2[2].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set($"Scope:({(a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2)).ToString()},1)");
                    WXCriticalMiddle_2_2[3].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set($"Scope:({(a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2)).ToString()},1)");

                    //3梁
                    XYZ start3 = ((framings[criticalMiddle][2].Location as LocationCurve).Curve as Line).Origin;
                    double length3 = ((framings[criticalMiddle][2].Location as LocationCurve).Curve as Line).Length;
                    XYZ direction3 = ((framings[criticalMiddle][2].Location as LocationCurve).Curve as Line).Direction;
                    XYZ end3 = new XYZ(start3.X + length3 * direction3.X / Math.Sqrt(direction3.X * direction3.X + direction3.Y * direction3.Y + direction3.Z * direction3.Z),
                                               start3.Y + length3 * direction3.Y / Math.Sqrt(direction3.X * direction3.X + direction3.Y * direction3.Y + direction3.Z * direction3.Z),
                                               start3.Z + length3 * direction3.Z / Math.Sqrt(direction3.X * direction3.X + direction3.Y * direction3.Y + direction3.Z * direction3.Z));
                    XYZ middle3 = start3 + (end3 - start3) * a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2);
                    LineLoad[] WXCriticalMiddle_3_1 = new LineLoad[4];
                    for (int i = 0; i < 4; i++)
                    {
                        WXCriticalMiddle_3_1[i]= LineLoad.Create(doc, start3, end3, XYZ.BasisZ, XYZ.Zero, lineLoadType, null);
                    }
                    LineLoad[] WXCriticalMiddle_3_2 = new LineLoad[4];
                    for (int i = 0; i < 4; i++)
                    {
                        WXCriticalMiddle_3_2[i]= LineLoad.Create(doc, start3, end3, XYZ.BasisZ, XYZ.Zero, lineLoadType, null);
                    }
                    double adjustFactorCritical_3 = SetLoadExtension.GetHeightVariationCoefficientOfWindPressure(doc, framings[criticalMiddle][2], groundRoughness);
                    //更改荷载大小
                    WXCriticalMiddle_3_1[0].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set((disCriticalMiddleXMinus / 2 - tempVal6 + UnitExtension.ConvertToMillimeters(xLength.Max() / 2)) * -factor2E_L_0 * basicWindPressure * adjustFactorCritical_3 * adjustFactor
                        + (disCriticalMiddleXPlus / 2 + tempVal6 - UnitExtension.ConvertToMillimeters(xLength.Max() / 2)) * -factor3E_L_0 * basicWindPressure * adjustFactorCritical_3 * adjustFactor);
                    WXCriticalMiddle_3_1[1].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set((disCriticalMiddleXMinus / 2 - tempVal6 + UnitExtension.ConvertToMillimeters(xLength.Max() / 2)) * -factor2E_L_1 * basicWindPressure * adjustFactorCritical_3 * adjustFactor
                        + (disCriticalMiddleXPlus / 2 + tempVal6 - UnitExtension.ConvertToMillimeters(xLength.Max() / 2)) * -factor3E_L_1 * basicWindPressure * adjustFactorCritical_3 * adjustFactor); 
                    WXCriticalMiddle_3_1[2].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set((disCriticalMiddleXMinus / 2 - tempVal6 + UnitExtension.ConvertToMillimeters(xLength.Max() / 2)) * -factor3E_L_0 * basicWindPressure * adjustFactorCritical_3 * adjustFactor
                        + (disCriticalMiddleXPlus / 2 + tempVal6 - UnitExtension.ConvertToMillimeters(xLength.Max() / 2)) * -factor2E_L_0 * basicWindPressure * adjustFactorCritical_3 * adjustFactor); 
                    WXCriticalMiddle_3_1[3].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set((disCriticalMiddleXMinus / 2 - tempVal6 + UnitExtension.ConvertToMillimeters(xLength.Max() / 2)) * -factor3E_L_1 * basicWindPressure * adjustFactorCritical_3 * adjustFactor
                        + (disCriticalMiddleXPlus / 2 + tempVal6 - UnitExtension.ConvertToMillimeters(xLength.Max() / 2)) * -factor2E_L_1 * basicWindPressure * adjustFactorCritical_3 * adjustFactor);
                    WXCriticalMiddle_3_2[0].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set((disCriticalMiddleXMinus / 2 - tempVal6 + UnitExtension.ConvertToMillimeters(xLength.Max() / 2)) * -factor2_L_0 * basicWindPressure * adjustFactorCritical_3 * adjustFactor
                        + (disCriticalMiddleXPlus / 2 + tempVal6 - UnitExtension.ConvertToMillimeters(xLength.Max() / 2)) * -factor3_L_0 * basicWindPressure * adjustFactorCritical_3 * adjustFactor); 
                    WXCriticalMiddle_3_2[1].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set((disCriticalMiddleXMinus / 2 - tempVal6 + UnitExtension.ConvertToMillimeters(xLength.Max() / 2)) * -factor2_L_1 * basicWindPressure * adjustFactorCritical_3 * adjustFactor
                        + (disCriticalMiddleXPlus / 2 + tempVal6 - UnitExtension.ConvertToMillimeters(xLength.Max() / 2)) * -factor3_L_1 * basicWindPressure * adjustFactorCritical_3 * adjustFactor); 
                    WXCriticalMiddle_3_2[2].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set((disCriticalMiddleXMinus / 2 - tempVal6 + UnitExtension.ConvertToMillimeters(xLength.Max() / 2)) * -factor3_L_0 * basicWindPressure * adjustFactorCritical_3 * adjustFactor
                        + (disCriticalMiddleXPlus / 2 + tempVal6 - UnitExtension.ConvertToMillimeters(xLength.Max() / 2)) * -factor2_L_0 * basicWindPressure * adjustFactorCritical_3 * adjustFactor); 
                    WXCriticalMiddle_3_2[3].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set((disCriticalMiddleXMinus / 2 - tempVal6 + UnitExtension.ConvertToMillimeters(xLength.Max() / 2)) * -factor3_L_1 * basicWindPressure * adjustFactorCritical_3 * adjustFactor
                        + (disCriticalMiddleXPlus / 2 + tempVal6 - UnitExtension.ConvertToMillimeters(xLength.Max() / 2)) * -factor2_L_1 * basicWindPressure * adjustFactorCritical_3 * adjustFactor);
                    //赋予荷载工况 
                    WXCriticalMiddle_3_1[0].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXPlusLoadCase_0.Id);
                    WXCriticalMiddle_3_1[1].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXPlusLoadCase_1.Id);
                    WXCriticalMiddle_3_1[2].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXMinusLoadCase_0.Id);
                    WXCriticalMiddle_3_1[3].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXMinusLoadCase_1.Id);

                    WXCriticalMiddle_3_2[0].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXPlusLoadCase_0.Id);
                    WXCriticalMiddle_3_2[1].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXPlusLoadCase_1.Id);
                    WXCriticalMiddle_3_2[2].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXMinusLoadCase_0.Id);
                    WXCriticalMiddle_3_2[3].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXMinusLoadCase_1.Id);
                    //附加说明
                    WXCriticalMiddle_3_1[0].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[criticalMiddle][2].Id.ToString());
                    WXCriticalMiddle_3_1[1].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[criticalMiddle][2].Id.ToString());
                    WXCriticalMiddle_3_1[2].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[criticalMiddle][2].Id.ToString());
                    WXCriticalMiddle_3_1[3].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[criticalMiddle][2].Id.ToString());

                    WXCriticalMiddle_3_1[0].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set($"Scope:(0,{(a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2)).ToString()})");
                    WXCriticalMiddle_3_1[1].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set($"Scope:(0,{(a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2)).ToString()})");
                    WXCriticalMiddle_3_1[2].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set($"Scope:(0,{(a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2)).ToString()})");
                    WXCriticalMiddle_3_1[3].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set($"Scope:(0,{(a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2)).ToString()})");


                    WXCriticalMiddle_3_2[0].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[criticalMiddle][2].Id.ToString());
                    WXCriticalMiddle_3_2[1].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[criticalMiddle][2].Id.ToString());
                    WXCriticalMiddle_3_2[2].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[criticalMiddle][2].Id.ToString());
                    WXCriticalMiddle_3_2[3].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[criticalMiddle][2].Id.ToString());


                    WXCriticalMiddle_3_2[0].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set($"Scope:({(a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2)).ToString()},1)");
                    WXCriticalMiddle_3_2[1].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set($"Scope:({(a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2)).ToString()},1)");
                    WXCriticalMiddle_3_2[2].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set($"Scope:({(a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2)).ToString()},1)");
                    WXCriticalMiddle_3_2[3].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set($"Scope:({(a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2)).ToString()},1)");


                }

                #endregion


                #region 西侧非临界跨梁

                for (int i = 0; i <= criticalMiddle-1; i++)
                {
                    //找X负向的最近柱并计算距离           
                    double disXMinus = SetLoadExtension.CalculateNearestDistance(doc, framings[i][0], -XYZ.BasisX, referenceIntersectorColumn);
                    //找X正向的最近柱并计算距离
                    double disXPlus = SetLoadExtension.CalculateNearestDistance(doc, framings[i][0], XYZ.BasisX, referenceIntersectorColumn);
                    double disColumn = disXPlus / 2 + disXMinus / 2;

                    //2梁
                    XYZ start2 = ((framings[i][1].Location as LocationCurve).Curve as Line).Origin;
                    double length2 = ((framings[i][1].Location as LocationCurve).Curve as Line).Length;
                    XYZ direction2 = ((framings[i][1].Location as LocationCurve).Curve as Line).Direction;                  
                    XYZ end2 = new XYZ(start2.X + length2 * direction2.X / Math.Sqrt(direction2.X * direction2.X + direction2.Y * direction2.Y + direction2.Z * direction2.Z),
                       start2.Y + length2 * direction2.Y / Math.Sqrt(direction2.X * direction2.X + direction2.Y * direction2.Y + direction2.Z * direction2.Z),
                       start2.Z + length2 * direction2.Z / Math.Sqrt(direction2.X * direction2.X + direction2.Y * direction2.Y + direction2.Z * direction2.Z));

                    XYZ middle2 = start2 + (end2 - start2) * a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2);

                    LineLoad[] WXWest_2_1 = new LineLoad[4];
                    for (int j = 0; j < 4; j++)
                    {
                        WXWest_2_1[j]= LineLoad.Create(doc, start2, middle2, XYZ.BasisZ, XYZ.Zero, lineLoadType, null);
                    }
                    LineLoad[] WXWest_2_2 = new LineLoad[4];
                    for (int j = 0; j < 4; j++)
                    {
                        WXWest_2_2[j] = LineLoad.Create(doc, middle2, end2, XYZ.BasisZ, XYZ.Zero, lineLoadType, null);
                    }
                    double adjustFactorWest_2 = SetLoadExtension.GetHeightVariationCoefficientOfWindPressure(doc, framings[i][1], groundRoughness);
                    //更改荷载大小
                    WXWest_2_1[0].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set(disColumn * -factor2E_L_0 * basicWindPressure * adjustFactorWest_2 * adjustFactor);
                    WXWest_2_1[1].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set(disColumn * -factor2E_L_1 * basicWindPressure * adjustFactorWest_2 * adjustFactor);
                    WXWest_2_1[2].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set(disColumn * -factor3E_L_0 * basicWindPressure * adjustFactorWest_2 * adjustFactor);
                    WXWest_2_1[3].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set(disColumn * -factor3E_L_1 * basicWindPressure * adjustFactorWest_2 * adjustFactor);

                    WXWest_2_2[0].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set(disColumn * -factor2_L_0 * basicWindPressure * adjustFactorWest_2 * adjustFactor);
                    WXWest_2_2[1].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set(disColumn * -factor2_L_1 * basicWindPressure * adjustFactorWest_2 * adjustFactor);
                    WXWest_2_2[2].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set(disColumn * -factor3_L_0 * basicWindPressure * adjustFactorWest_2 * adjustFactor);
                    WXWest_2_2[3].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set(disColumn * -factor3_L_1 * basicWindPressure * adjustFactorWest_2 * adjustFactor);

                    //赋予荷载工况 
                    WXWest_2_1[0].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXPlusLoadCase_0.Id);
                    WXWest_2_1[1].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXPlusLoadCase_1.Id);
                    WXWest_2_1[2].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXMinusLoadCase_0.Id);
                    WXWest_2_1[3].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXMinusLoadCase_1.Id);

                    WXWest_2_2[0].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXPlusLoadCase_0.Id);
                    WXWest_2_2[1].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXPlusLoadCase_1.Id);
                    WXWest_2_2[2].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXMinusLoadCase_0.Id);
                    WXWest_2_2[3].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXMinusLoadCase_1.Id);

                    //附加说明
                    WXWest_2_1[0].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][1].Id.ToString());
                    WXWest_2_1[1].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][1].Id.ToString());
                    WXWest_2_1[2].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][1].Id.ToString());
                    WXWest_2_1[3].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][1].Id.ToString());

                    WXWest_2_1[0].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set($"Scope:(0,{(a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2)).ToString()})");
                    WXWest_2_1[1].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set($"Scope:(0,{(a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2)).ToString()})");
                    WXWest_2_1[2].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set($"Scope:(0,{(a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2)).ToString()})");
                    WXWest_2_1[3].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set($"Scope:(0,{(a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2)).ToString()})");


                    WXWest_2_2[0].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][1].Id.ToString());
                    WXWest_2_2[1].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][1].Id.ToString());
                    WXWest_2_2[2].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][1].Id.ToString());
                    WXWest_2_2[3].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][1].Id.ToString());

                    WXWest_2_2[0].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set($"Scope:({(a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2)).ToString()},1)");
                    WXWest_2_2[1].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set($"Scope:({(a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2)).ToString()},1)");
                    WXWest_2_2[2].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set($"Scope:({(a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2)).ToString()},1)");
                    WXWest_2_2[3].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set($"Scope:({(a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2)).ToString()},1)");

                    //3梁
                    XYZ start3 = ((framings[i][2].Location as LocationCurve).Curve as Line).Origin;
                    double length3 = ((framings[i][2].Location as LocationCurve).Curve as Line).Length;
                    XYZ direction3 = ((framings[i][2].Location as LocationCurve).Curve as Line).Direction;
                    XYZ end3 = new XYZ(start3.X + length3 * direction3.X / Math.Sqrt(direction3.X * direction3.X + direction3.Y * direction3.Y + direction3.Z * direction3.Z),
                                               start3.Y + length3 * direction3.Y / Math.Sqrt(direction3.X * direction3.X + direction3.Y * direction3.Y + direction3.Z * direction3.Z),
                                               start3.Z + length3 * direction3.Z / Math.Sqrt(direction3.X * direction3.X + direction3.Y * direction3.Y + direction3.Z * direction3.Z));
                    XYZ middle3 = start3 + (end3 - start3) * a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2);

                    LineLoad[] WXWest_3_1 = new LineLoad[4];
                    for (int j = 0; j < 4; j++)
                    {
                        WXWest_3_1[j]= LineLoad.Create(doc, start3, middle3, XYZ.BasisZ, XYZ.Zero, lineLoadType, null);
                    }
                    LineLoad[] WXWest_3_2 = new LineLoad[4];
                    for (int j = 0; j < 4; j++)
                    {
                        WXWest_3_2[j]= LineLoad.Create(doc, middle3, end3, XYZ.BasisZ, XYZ.Zero, lineLoadType, null);
                    }
                    double adjustFactorWest_3 = SetLoadExtension.GetHeightVariationCoefficientOfWindPressure(doc, framings[i][2], groundRoughness);

                    //更改荷载大小
                    WXWest_3_1[0].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set(disColumn * -factor2E_L_0 * basicWindPressure * adjustFactorWest_3 * adjustFactor);
                    WXWest_3_1[1].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set(disColumn * -factor2E_L_1 * basicWindPressure * adjustFactorWest_3 * adjustFactor);
                    WXWest_3_1[2].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set(disColumn * -factor3E_L_0 * basicWindPressure * adjustFactorWest_3 * adjustFactor);
                    WXWest_3_1[3].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set(disColumn * -factor3E_L_1 * basicWindPressure * adjustFactorWest_3 * adjustFactor);

                    WXWest_3_2[0].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set(disColumn * -factor2_L_0 * basicWindPressure * adjustFactorWest_3 * adjustFactor);
                    WXWest_3_2[1].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set(disColumn * -factor2_L_1 * basicWindPressure * adjustFactorWest_3 * adjustFactor);
                    WXWest_3_2[2].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set(disColumn * -factor3_L_0 * basicWindPressure * adjustFactorWest_3 * adjustFactor);
                    WXWest_3_2[3].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set(disColumn * -factor3_L_1 * basicWindPressure * adjustFactorWest_3 * adjustFactor);
                    //赋予荷载工况 
                    WXWest_3_1[0].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXPlusLoadCase_0.Id);
                    WXWest_3_1[1].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXPlusLoadCase_1.Id);
                    WXWest_3_1[2].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXMinusLoadCase_0.Id);
                    WXWest_3_1[3].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXMinusLoadCase_1.Id);

                    WXWest_3_2[0].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXPlusLoadCase_0.Id);
                    WXWest_3_2[1].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXPlusLoadCase_1.Id);
                    WXWest_3_2[2].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXMinusLoadCase_0.Id);
                    WXWest_3_2[3].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXMinusLoadCase_1.Id);
                    //附加说明
                    WXWest_3_1[0].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][2].Id.ToString());
                    WXWest_3_1[1].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][2].Id.ToString());
                    WXWest_3_1[2].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][2].Id.ToString());
                    WXWest_3_1[3].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][2].Id.ToString());

                    WXWest_3_1[0].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set($"Scope:(0,{(a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2)).ToString()})");
                    WXWest_3_1[1].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set($"Scope:(0,{(a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2)).ToString()})");
                    WXWest_3_1[2].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set($"Scope:(0,{(a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2)).ToString()})");
                    WXWest_3_1[3].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set($"Scope:(0,{(a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2)).ToString()})");

                    WXWest_3_2[0].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][2].Id.ToString());
                    WXWest_3_2[1].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][2].Id.ToString());
                    WXWest_3_2[2].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][2].Id.ToString());
                    WXWest_3_2[3].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][2].Id.ToString());

                    WXWest_3_2[0].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set($"Scope:({(a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2)).ToString()},1)");
                    WXWest_3_2[1].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set($"Scope:({(a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2)).ToString()},1)");
                    WXWest_3_2[2].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set($"Scope:({(a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2)).ToString()},1)");
                    WXWest_3_2[3].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set($"Scope:({(a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2)).ToString()},1)");

                }

                #endregion


                #region 东侧非临界跨梁
                for (int i = criticalMiddle+1; i < framings.Count; i++)
                {

                    //找X负向的最近柱并计算距离
                    double disXMinus = SetLoadExtension.CalculateNearestDistance(doc, framings[i][0], -XYZ.BasisX, referenceIntersectorColumn);
                    //找X正向的最近柱并计算距离
                    double disXPlus = SetLoadExtension.CalculateNearestDistance(doc, framings[i][0], XYZ.BasisX, referenceIntersectorColumn);
                    double disColumn = disXPlus / 2 + disXMinus / 2;

                    //2梁
                    XYZ start2 = ((framings[i][1].Location as LocationCurve).Curve as Line).Origin;
                    double length2 = ((framings[i][1].Location as LocationCurve).Curve as Line).Length;
                    XYZ direction2 = ((framings[i][1].Location as LocationCurve).Curve as Line).Direction;
                    XYZ end2 = new XYZ(start2.X + length2 * direction2.X / Math.Sqrt(direction2.X * direction2.X + direction2.Y * direction2.Y + direction2.Z * direction2.Z),
                       start2.Y + length2 * direction2.Y / Math.Sqrt(direction2.X * direction2.X + direction2.Y * direction2.Y + direction2.Z * direction2.Z),
                       start2.Z + length2 * direction2.Z / Math.Sqrt(direction2.X * direction2.X + direction2.Y * direction2.Y + direction2.Z * direction2.Z));

                    XYZ middle2 = start2 + (end2 - start2) * a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2);
                    LineLoad[] WXEast_2_1 = new LineLoad[4];
                    for (int j = 0; j < 4; j++)
                    {
                        WXEast_2_1[j]= LineLoad.Create(doc, start2, middle2, XYZ.BasisZ, XYZ.Zero, lineLoadType, null);
                    }
                    LineLoad[] WXEast_2_2 = new LineLoad[4];
                    for (int j = 0; j < 4; j++)
                    {
                        WXEast_2_2[j]= LineLoad.Create(doc, middle2, end2, XYZ.BasisZ, XYZ.Zero, lineLoadType, null);
                    }
                    double adjustFactorEast_2 = SetLoadExtension.GetHeightVariationCoefficientOfWindPressure(doc, framings[i][1],groundRoughness);
                    //更改荷载大小
                    WXEast_2_1[0].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set(disColumn * -factor3E_L_0 * basicWindPressure * adjustFactorEast_2 * adjustFactor);
                    WXEast_2_1[1].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set(disColumn * -factor3E_L_1 * basicWindPressure * adjustFactorEast_2 * adjustFactor);
                    WXEast_2_1[2].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set(disColumn * -factor2E_L_0 * basicWindPressure * adjustFactorEast_2 * adjustFactor);
                    WXEast_2_1[3].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set(disColumn * -factor2E_L_1 * basicWindPressure * adjustFactorEast_2 * adjustFactor);
                    WXEast_2_2[0].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set(disColumn * -factor3_L_0 * basicWindPressure * adjustFactorEast_2 * adjustFactor);
                    WXEast_2_2[1].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set(disColumn * -factor3_L_1 * basicWindPressure * adjustFactorEast_2 * adjustFactor);
                    WXEast_2_2[2].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set(disColumn * -factor2_L_0 * basicWindPressure * adjustFactorEast_2 * adjustFactor);
                    WXEast_2_2[3].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set(disColumn * -factor2_L_1 * basicWindPressure * adjustFactorEast_2 * adjustFactor);

                    //赋予荷载工况 
                    WXEast_2_1[0].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXPlusLoadCase_0.Id);
                    WXEast_2_1[1].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXPlusLoadCase_1.Id);
                    WXEast_2_1[2].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXMinusLoadCase_0.Id);
                    WXEast_2_1[3].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXMinusLoadCase_1.Id);
                    WXEast_2_2[0].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXPlusLoadCase_0.Id);
                    WXEast_2_2[1].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXPlusLoadCase_1.Id);
                    WXEast_2_2[2].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXMinusLoadCase_0.Id);
                    WXEast_2_2[3].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXMinusLoadCase_1.Id);
                    //附加说明
                    WXEast_2_1[0].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][1].Id.ToString());
                    WXEast_2_1[1].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][1].Id.ToString());
                    WXEast_2_1[2].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][1].Id.ToString());
                    WXEast_2_1[3].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][1].Id.ToString());

                    WXEast_2_1[0].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set($"Scope:(0,{(a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2)).ToString()})");
                    WXEast_2_1[1].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set($"Scope:(0,{(a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2)).ToString()})");
                    WXEast_2_1[2].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set($"Scope:(0,{(a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2)).ToString()})");
                    WXEast_2_1[3].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set($"Scope:(0,{(a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2)).ToString()})");


                    WXEast_2_2[0].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][1].Id.ToString());
                    WXEast_2_2[1].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][1].Id.ToString());
                    WXEast_2_2[2].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][1].Id.ToString());
                    WXEast_2_2[3].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][1].Id.ToString());


                    WXEast_2_2[0].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set($"Scope:({(a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2)).ToString()},1)");
                    WXEast_2_2[1].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set($"Scope:({(a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2)).ToString()},1)");
                    WXEast_2_2[2].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set($"Scope:({(a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2)).ToString()},1)");
                    WXEast_2_2[3].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set($"Scope:({(a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2)).ToString()},1)");


                    //3梁
                    XYZ start3 = ((framings[i][2].Location as LocationCurve).Curve as Line).Origin;
                    double length3 = ((framings[i][2].Location as LocationCurve).Curve as Line).Length;
                    XYZ direction3 = ((framings[i][2].Location as LocationCurve).Curve as Line).Direction;
                    XYZ end3 = new XYZ(start3.X + length3 * direction3.X / Math.Sqrt(direction3.X * direction3.X + direction3.Y * direction3.Y + direction3.Z * direction3.Z),
                                               start3.Y + length3 * direction3.Y / Math.Sqrt(direction3.X * direction3.X + direction3.Y * direction3.Y + direction3.Z * direction3.Z),
                                               start3.Z + length3 * direction3.Z / Math.Sqrt(direction3.X * direction3.X + direction3.Y * direction3.Y + direction3.Z * direction3.Z));
                    XYZ middle3 = start3 + (end3 - start3) * a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2);
                    LineLoad[] WXEast_3_1 = new LineLoad[4];
                    for (int j = 0; j < 4; j++)
                    {
                        WXEast_3_1[j]= LineLoad.Create(doc, start3, middle3, XYZ.BasisZ, XYZ.Zero, lineLoadType, null);
                    }
                    LineLoad[] WXEast_3_2 = new LineLoad[4];
                    for (int j = 0; j < 4; j++)
                    {
                        WXEast_3_2[j]= LineLoad.Create(doc, middle3, end3, XYZ.BasisZ, XYZ.Zero, lineLoadType, null);
                    }
                    double adjustFactorEast_3 = SetLoadExtension.GetHeightVariationCoefficientOfWindPressure(doc, framings[i][2], groundRoughness);

                    //更改荷载大小
                    WXEast_3_1[0].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set(disColumn * -factor3E_L_0 * basicWindPressure * adjustFactorEast_3 * adjustFactor);
                    WXEast_3_1[1].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set(disColumn * -factor3E_L_1 * basicWindPressure * adjustFactorEast_3 * adjustFactor);
                    WXEast_3_1[2].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set(disColumn * -factor2E_L_0 * basicWindPressure * adjustFactorEast_3 * adjustFactor);
                    WXEast_3_1[3].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set(disColumn * -factor2E_L_1 * basicWindPressure * adjustFactorEast_3 * adjustFactor);

                    WXEast_3_2[0].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set(disColumn * -factor3_L_0 * basicWindPressure * adjustFactorEast_3 * adjustFactor);
                    WXEast_3_2[1].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set(disColumn * -factor3_L_1 * basicWindPressure * adjustFactorEast_3 * adjustFactor);
                    WXEast_3_2[2].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set(disColumn * -factor2_L_0 * basicWindPressure * adjustFactorEast_3 * adjustFactor);
                    WXEast_3_2[3].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).Set(disColumn * -factor2_L_1 * basicWindPressure * adjustFactorEast_3 * adjustFactor);
                    //赋予荷载工况 
                    WXEast_3_1[0].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXPlusLoadCase_0.Id);
                    WXEast_3_1[1].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXPlusLoadCase_1.Id);
                    WXEast_3_1[2].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXMinusLoadCase_0.Id);
                    WXEast_3_1[3].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXMinusLoadCase_1.Id);

                    WXEast_3_2[0].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXPlusLoadCase_0.Id);
                    WXEast_3_2[1].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXPlusLoadCase_1.Id);
                    WXEast_3_2[2].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXMinusLoadCase_0.Id);
                    WXEast_3_2[3].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXMinusLoadCase_1.Id);
                    //附加说明
                    WXEast_3_1[0].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][2].Id.ToString());
                    WXEast_3_1[1].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][2].Id.ToString());
                    WXEast_3_1[2].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][2].Id.ToString());
                    WXEast_3_1[3].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][2].Id.ToString());

                    WXEast_3_1[0].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set($"Scope:(0,{(a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2)).ToString()})");
                    WXEast_3_1[1].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set($"Scope:(0,{(a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2)).ToString()})");
                    WXEast_3_1[2].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set($"Scope:(0,{(a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2)).ToString()})");
                    WXEast_3_1[3].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set($"Scope:(0,{(a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2)).ToString()})");

                    WXEast_3_2[0].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][2].Id.ToString());
                    WXEast_3_2[1].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][2].Id.ToString());
                    WXEast_3_2[2].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][2].Id.ToString());
                    WXEast_3_2[3].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + framings[i][2].Id.ToString());

                    WXEast_3_2[0].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set($"Scope:({(a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2)).ToString()},1)");
                    WXEast_3_2[1].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set($"Scope:({(a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2)).ToString()},1)");
                    WXEast_3_2[2].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set($"Scope:({(a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2)).ToString()},1)");
                    WXEast_3_2[3].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set($"Scope:({(a / UnitExtension.ConvertToMillimeters(yLength.Max() / 2)).ToString()},1)");


                }
                #endregion


                #region WX工况下山墙处各柱风荷载
                List<FamilyInstance> orderedWestColumns = westColumns.OrderBy(t => (t.Location as LocationPoint).Point.Y).ToList(); 
                List<FamilyInstance> orderedEastColumns = eastColumns.OrderBy(t => (t.Location as LocationPoint).Point.Y).ToList();

                int recordaF=0;
                for (int i = 0; i < orderedWestColumns.Count; i++)
                {
                    double dis = UnitExtension.ConvertToMillimeters((orderedWestColumns[0].Location as LocationPoint).Point.DistanceTo((orderedWestColumns[i].Location as LocationPoint).Point));
                    if (dis >  a)
                    {
                        recordaF = i;
                        break;
                    }
                }
                int recordaB = 0;
                for (int i = orderedWestColumns.Count-1; i >0 ; i--)
                {
                    double dis = UnitExtension.ConvertToMillimeters((orderedWestColumns[orderedWestColumns.Count-1].Location as LocationPoint).Point.DistanceTo((orderedWestColumns[i].Location as LocationPoint).Point));
                    if (dis >  a)
                    {
                        recordaB = i;
                        break;
                    }
                }
                
                int startMiddleY=recordaF;
                int endMiddleY=recordaB;

                //通过a所在位置划分中间跨和边跨，在startMiddle和endMiddle之间为中间跨，0和startMiddle之间为左边跨，endMiddle和framings.count-1之间为右边跨
                //找Y负向的最近柱并计算距离
                double disRecordYMinus_South = SetLoadExtension.CalculateNearestDistance(doc, orderedWestColumns[recordaF],-XYZ.BasisY,referenceIntersectorAllColumns);
                double tempVal7 = UnitExtension.ConvertToMillimeters((orderedWestColumns[recordaF].Location as LocationPoint).Point.DistanceTo((orderedWestColumns[0].Location as LocationPoint).Point));
                if (tempVal7 -  a <= disRecordYMinus_South / 2)
                {
                    startMiddleY = recordaF + 1;
                }

                //找x正向的最近柱并计算距离             
                double disRecordaYPlus_North= SetLoadExtension.CalculateNearestDistance(doc, orderedWestColumns[recordaB], -XYZ.BasisY, referenceIntersectorAllColumns);
                double tempVal8 = UnitExtension.ConvertToMillimeters((framings[recordaB][0].Location as LocationPoint).Point.DistanceTo((orderedWestColumns[orderedWestColumns.Count-1].Location as LocationPoint).Point));
                if (tempVal8 -  a <= disRecordaYPlus_North / 2)
                {
                    endMiddleY = recordaB - 1;
                }

                #region 中间跨 （西）
                for (int i = startMiddleY; i <=endMiddleY ; i++)
                {

                    //找y负向的最近柱并计算距离    
                    double disMiddleYMinus = SetLoadExtension.CalculateNearestDistance(doc, orderedWestColumns[i], -XYZ.BasisY, referenceIntersectorAllColumns);
                    //找y正向的最近柱并计算距离
                    double disMiddleYPlus = SetLoadExtension.CalculateNearestDistance(doc, orderedWestColumns[i], XYZ.BasisY, referenceIntersectorAllColumns);
                    double disMiddle = disMiddleYMinus / 2 + disMiddleYPlus / 2;
                    XYZ start = (orderedWestColumns[i].Location as LocationPoint).Point;
                    double height = (doc.GetElement(orderedWestColumns[i].get_Parameter(BuiltInParameter.SCHEDULE_TOP_LEVEL_PARAM).AsElementId()) as Level).Elevation;
                    double offset = orderedWestColumns[i].get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM).AsDouble();
                    XYZ end = (orderedWestColumns[i].Location as LocationPoint).Point + new XYZ(0, 0, height + offset);

                    LineLoad[] WXWestMiddle = new LineLoad[4];
                    for (int j = 0; j < 4; j++)
                    {
                        WXWestMiddle[j]= LineLoad.Create(doc, start, end, XYZ.BasisX, XYZ.Zero, lineLoadType, null);
                    }
                    double adjustFactorWestMiddle = SetLoadExtension.GetHeightVariationCoefficientOfWindPressure(doc, orderedWestColumns[i], groundRoughness);
                    //更改荷载大小
                    WXWestMiddle[0].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).Set(disMiddle * factor1_L_0 * basicWindPressure * adjustFactorWestMiddle * adjustFactor);
                    WXWestMiddle[1].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).Set(disMiddle * factor1_L_1 * basicWindPressure * adjustFactorWestMiddle * adjustFactor);
                    WXWestMiddle[2].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).Set(disMiddle * factor4_L_0 * basicWindPressure * adjustFactorWestMiddle * adjustFactor);
                    WXWestMiddle[3].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).Set(disMiddle * factor4_L_1 * basicWindPressure * adjustFactorWestMiddle * adjustFactor);
                    //赋予荷载工况 
                    WXWestMiddle[0].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXPlusLoadCase_0.Id);
                    WXWestMiddle[1].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXPlusLoadCase_1.Id);
                    WXWestMiddle[2].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXMinusLoadCase_0.Id);
                    WXWestMiddle[3].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXMinusLoadCase_1.Id);
                    //附加说明
                    WXWestMiddle[0].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + orderedWestColumns[i].Id.ToString());
                    WXWestMiddle[1].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + orderedWestColumns[i].Id.ToString());
                    WXWestMiddle[2].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + orderedWestColumns[i].Id.ToString());
                    WXWestMiddle[3].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + orderedWestColumns[i].Id.ToString());


                    WXWestMiddle[0].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WXWestMiddle[1].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WXWestMiddle[2].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WXWestMiddle[3].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                }
                #endregion

                #region 中间跨（东）
                for (int i = startMiddleY; i <= endMiddleY; i++)
                {
                    //找y负向的最近柱并计算距离
                    double disMiddleYMinus = SetLoadExtension.CalculateNearestDistance(doc, orderedEastColumns[i], -XYZ.BasisY, referenceIntersectorAllColumns);
                    //找y正向的最近柱并计算距离
                    double disMiddleYPlus = SetLoadExtension.CalculateNearestDistance(doc, orderedEastColumns[i], XYZ.BasisY, referenceIntersectorAllColumns);
                    double disMiddle = disMiddleYMinus / 2 + disMiddleYPlus / 2;
                    XYZ start = (orderedEastColumns[i].Location as LocationPoint).Point;
                    double height = (doc.GetElement(orderedEastColumns[i].get_Parameter(BuiltInParameter.SCHEDULE_TOP_LEVEL_PARAM).AsElementId()) as Level).Elevation;
                    double offset = orderedEastColumns[i].get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM).AsDouble();
                    XYZ end = (orderedEastColumns[i].Location as LocationPoint).Point + new XYZ(0, 0, height + offset);
                    LineLoad[] WXEastMiddle = new LineLoad[4];
                    for (int j = 0; j < 4; j++)
                    {
                        WXEastMiddle[j] = LineLoad.Create(doc, start, end, XYZ.BasisX, XYZ.Zero, lineLoadType, null);
                    }
                    double adjustFactorEastMiddle = SetLoadExtension.GetHeightVariationCoefficientOfWindPressure(doc, orderedEastColumns[i], groundRoughness);
                    //更改荷载大小
                    WXEastMiddle[0].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).Set(disMiddle * -factor4_L_0 * basicWindPressure * adjustFactorEastMiddle * adjustFactor);
                    WXEastMiddle[1].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).Set(disMiddle * -factor4_L_1 * basicWindPressure * adjustFactorEastMiddle * adjustFactor);
                    WXEastMiddle[2].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).Set(disMiddle * -factor1_L_0 * basicWindPressure * adjustFactorEastMiddle * adjustFactor);
                    WXEastMiddle[3].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).Set(disMiddle * -factor1_L_1 * basicWindPressure * adjustFactorEastMiddle * adjustFactor);
                    //赋予荷载工况 
                    WXEastMiddle[0].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXPlusLoadCase_0.Id);
                    WXEastMiddle[1].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXPlusLoadCase_1.Id);
                    WXEastMiddle[2].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXMinusLoadCase_0.Id);
                    WXEastMiddle[3].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXMinusLoadCase_1.Id);
                    //附加说明
                    WXEastMiddle[0].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + orderedEastColumns[i].Id.ToString());
                    WXEastMiddle[1].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + orderedEastColumns[i].Id.ToString());
                    WXEastMiddle[2].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + orderedEastColumns[i].Id.ToString());
                    WXEastMiddle[3].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + orderedEastColumns[i].Id.ToString());

                    WXEastMiddle[0].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WXEastMiddle[1].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WXEastMiddle[2].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WXEastMiddle[3].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                }
                #endregion

                #region 边跨 (西)（前）
                for (int i = 0; i < startMiddleY-1; i++)
                {
                    //找y负向的最近柱并计算距离
                    double disEdgeYMinus = SetLoadExtension.CalculateNearestDistance(doc, orderedWestColumns[i], -XYZ.BasisY, referenceIntersectorAllColumns);
                    //找y正向的最近柱并计算距离
                    double disEdgeYPlus = SetLoadExtension.CalculateNearestDistance(doc, orderedWestColumns[i], XYZ.BasisY, referenceIntersectorAllColumns);
                    double disEdge = disEdgeYMinus / 2 + disEdgeYPlus / 2;
                    XYZ start = (orderedWestColumns[i].Location as LocationPoint).Point;
                    double height = (doc.GetElement(orderedWestColumns[i].get_Parameter(BuiltInParameter.SCHEDULE_TOP_LEVEL_PARAM).AsElementId()) as Level).Elevation;
                    double offset = orderedWestColumns[i].get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM).AsDouble();
                    XYZ end = (orderedWestColumns[i].Location as LocationPoint).Point + new XYZ(0, 0, height + offset);

                    LineLoad[] WXWestEdge_South = new LineLoad[4];
                    for (int j = 0; j < 4; j++)
                    {
                        WXWestEdge_South[j]= LineLoad.Create(doc, start, end, XYZ.BasisX, XYZ.Zero, lineLoadType, null);
                    }
                    double adjustFactorWestEdge_South = SetLoadExtension.GetHeightVariationCoefficientOfWindPressure(doc, orderedWestColumns[i], groundRoughness);
                    //更改荷载大小
                    WXWestEdge_South[0].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).Set(disEdge * factor1E_L_0 * basicWindPressure * adjustFactorWestEdge_South * adjustFactor);
                    WXWestEdge_South[1].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).Set(disEdge * factor1E_L_1 * basicWindPressure * adjustFactorWestEdge_South * adjustFactor);
                    WXWestEdge_South[2].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).Set(disEdge * factor4E_L_0 * basicWindPressure * adjustFactorWestEdge_South * adjustFactor);
                    WXWestEdge_South[3].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).Set(disEdge * factor4E_L_1 * basicWindPressure * adjustFactorWestEdge_South * adjustFactor);
                    //赋予荷载工况 
                    WXWestEdge_South[0].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXPlusLoadCase_0.Id);
                    WXWestEdge_South[1].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXPlusLoadCase_1.Id);
                    WXWestEdge_South[2].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXMinusLoadCase_0.Id);
                    WXWestEdge_South[3].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXMinusLoadCase_1.Id);
                    //附加说明
                    WXWestEdge_South[0].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + orderedWestColumns[i].Id.ToString());
                    WXWestEdge_South[1].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + orderedWestColumns[i].Id.ToString());
                    WXWestEdge_South[2].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + orderedWestColumns[i].Id.ToString());
                    WXWestEdge_South[3].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + orderedWestColumns[i].Id.ToString());

                    WXWestEdge_South[0].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WXWestEdge_South[1].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WXWestEdge_South[2].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WXWestEdge_South[3].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                }

                #endregion

                #region 边跨（西）（后）
                for (int i = endMiddleY+2; i < orderedWestColumns.Count; i++)
                {
                    //找y负向的最近柱并计算距离
                    double disEdgeYMinus = SetLoadExtension.CalculateNearestDistance(doc, orderedWestColumns[i], -XYZ.BasisY, referenceIntersectorAllColumns);
                    //找y正向的最近柱并计算距离
                    double disEdgeYPlus = SetLoadExtension.CalculateNearestDistance(doc, orderedWestColumns[i], XYZ.BasisY, referenceIntersectorAllColumns);
                    double disEdge = disEdgeYMinus / 2 + disEdgeYPlus / 2;
                    XYZ start = (orderedWestColumns[i].Location as LocationPoint).Point;
                    double height = (doc.GetElement(orderedWestColumns[i].get_Parameter(BuiltInParameter.SCHEDULE_TOP_LEVEL_PARAM).AsElementId()) as Level).Elevation;
                    double offset = orderedWestColumns[i].get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM).AsDouble();
                    XYZ end = (orderedWestColumns[i].Location as LocationPoint).Point + new XYZ(0, 0, height + offset);
                    LineLoad[] WXWestEdge_North = new LineLoad[4];
                    for (int j = 0; j < 4; j++)
                    {
                        WXWestEdge_North[i]= LineLoad.Create(doc, start, end, XYZ.BasisX, XYZ.Zero, lineLoadType, null);
                    }
                    double adjustFactorWestEdge_North = SetLoadExtension.GetHeightVariationCoefficientOfWindPressure(doc, orderedWestColumns[i], groundRoughness);

                    //更改荷载大小
                    WXWestEdge_North[0].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).Set(disEdge * factor1E_L_0 * basicWindPressure * adjustFactorWestEdge_North * adjustFactor);
                    WXWestEdge_North[1].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).Set(disEdge * factor1E_L_1 * basicWindPressure * adjustFactorWestEdge_North * adjustFactor);
                    WXWestEdge_North[2].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).Set(disEdge * factor4E_L_0 * basicWindPressure * adjustFactorWestEdge_North * adjustFactor);
                    WXWestEdge_North[3].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).Set(disEdge * factor4E_L_1 * basicWindPressure * adjustFactorWestEdge_North * adjustFactor);
                    //赋予荷载工况 
                    WXWestEdge_North[0].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXPlusLoadCase_0.Id);
                    WXWestEdge_North[0].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXPlusLoadCase_1.Id);
                    WXWestEdge_North[0].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXMinusLoadCase_0.Id);
                    WXWestEdge_North[0].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXMinusLoadCase_1.Id);
                    //附加说明
                    WXWestEdge_North[0].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + orderedWestColumns[i].Id.ToString());
                    WXWestEdge_North[1].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + orderedWestColumns[i].Id.ToString());
                    WXWestEdge_North[2].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + orderedWestColumns[i].Id.ToString());
                    WXWestEdge_North[3].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + orderedWestColumns[i].Id.ToString());

                    WXWestEdge_North[0].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WXWestEdge_North[1].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WXWestEdge_North[2].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WXWestEdge_North[3].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                }
                #endregion

                #region 边跨（东）（前）
                for (int i = 0; i < startMiddleY - 1; i++)
                {
                    //找y负向的最近柱并计算距离
                    double disEdgeYMinus = SetLoadExtension.CalculateNearestDistance(doc, orderedEastColumns[i],-XYZ.BasisY,referenceIntersectorAllColumns);
                    //找y正向的最近柱并计算距离
                    double disEdgeYPlus = SetLoadExtension.CalculateNearestDistance(doc, orderedEastColumns[i], XYZ.BasisY, referenceIntersectorAllColumns);
                    double disEdge = disEdgeYMinus / 2 + disEdgeYPlus / 2;
                    XYZ start = (orderedEastColumns[i].Location as LocationPoint).Point;
                    double height = (doc.GetElement(orderedEastColumns[i].get_Parameter(BuiltInParameter.SCHEDULE_TOP_LEVEL_PARAM).AsElementId()) as Level).Elevation;
                    double offset = orderedEastColumns[i].get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM).AsDouble();
                    XYZ end = (orderedEastColumns[i].Location as LocationPoint).Point + new XYZ(0, 0, height + offset);
                    LineLoad[] WXEastEdge_South = new LineLoad[4];
                    for (int j = 0; j < 4; j++)
                    {
                        WXEastEdge_South[i]= LineLoad.Create(doc, start, end, XYZ.BasisX, XYZ.Zero, lineLoadType, null);
                    }
                    double adjustFactorEastEdge_South = SetLoadExtension.GetHeightVariationCoefficientOfWindPressure(doc, orderedEastColumns[i], groundRoughness);

                    //更改荷载大小
                    WXEastEdge_South[0].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).Set(disEdge * -factor4E_L_0 * basicWindPressure * adjustFactorEastEdge_South * adjustFactor);
                    WXEastEdge_South[1].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).Set(disEdge * -factor4E_L_1 * basicWindPressure * adjustFactorEastEdge_South * adjustFactor);
                    WXEastEdge_South[2].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).Set(disEdge * -factor1E_L_0 * basicWindPressure * adjustFactorEastEdge_South * adjustFactor);
                    WXEastEdge_South[3].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).Set(disEdge * -factor1E_L_1 * basicWindPressure * adjustFactorEastEdge_South * adjustFactor);
                    //赋予荷载工况 
                    WXEastEdge_South[0].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXPlusLoadCase_0.Id);
                    WXEastEdge_South[1].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXPlusLoadCase_1.Id);
                    WXEastEdge_South[2].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXMinusLoadCase_0.Id);
                    WXEastEdge_South[3].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXMinusLoadCase_1.Id);
                    //附加说明
                    WXEastEdge_South[0].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + orderedEastColumns[i].Id.ToString());
                    WXEastEdge_South[1].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + orderedEastColumns[i].Id.ToString());
                    WXEastEdge_South[2].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + orderedEastColumns[i].Id.ToString());
                    WXEastEdge_South[3].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + orderedEastColumns[i].Id.ToString());

                    WXEastEdge_South[0].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WXEastEdge_South[1].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WXEastEdge_South[2].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WXEastEdge_South[3].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                }
                #endregion

                #region 边跨（东）（后）
                for (int i = endMiddleY + 2; i < orderedWestColumns.Count; i++)
                {
                    //找y负向的最近柱并计算距离
                    double disEdgeYMinus = SetLoadExtension.CalculateNearestDistance(doc, orderedWestColumns[i], -XYZ.BasisY, referenceIntersectorAllColumns);
                    //找y正向的最近柱并计算距离
                    double disEdgeYPlus= SetLoadExtension.CalculateNearestDistance(doc, orderedWestColumns[i], XYZ.BasisY, referenceIntersectorAllColumns);
                    double disEdge = disEdgeYMinus / 2 + disEdgeYPlus / 2;
                    XYZ start = (orderedEastColumns[i].Location as LocationPoint).Point;
                    double height = (doc.GetElement(orderedEastColumns[i].get_Parameter(BuiltInParameter.SCHEDULE_TOP_LEVEL_PARAM).AsElementId()) as Level).Elevation;
                    double offset = orderedEastColumns[i].get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM).AsDouble();
                    XYZ end = (orderedEastColumns[i].Location as LocationPoint).Point + new XYZ(0, 0, height + offset);
                    LineLoad[] WXEastEdge_North = new LineLoad[4];
                    for (int j = 0; j < 4; j++)
                    {
                        WXEastEdge_North[j]= LineLoad.Create(doc, start, end, XYZ.BasisX, XYZ.Zero, lineLoadType, null);
                    }

                    double adjustFactorEastEdge_North = SetLoadExtension.GetHeightVariationCoefficientOfWindPressure(doc, orderedEastColumns[i], groundRoughness);

                    //更改荷载大小
                    WXEastEdge_North[0].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).Set(disEdge * -factor4E_L_0 * basicWindPressure * adjustFactorEastEdge_North * adjustFactor);
                    WXEastEdge_North[1].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).Set(disEdge * -factor4E_L_1 * basicWindPressure * adjustFactorEastEdge_North * adjustFactor);
                    WXEastEdge_North[2].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).Set(disEdge * -factor1E_L_0 * basicWindPressure * adjustFactorEastEdge_North * adjustFactor);
                    WXEastEdge_North[3].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).Set(disEdge * -factor1E_L_1 * basicWindPressure * adjustFactorEastEdge_North * adjustFactor);
                    //赋予荷载工况 
                    WXEastEdge_North[0].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXPlusLoadCase_0.Id);
                    WXEastEdge_North[1].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXPlusLoadCase_1.Id);
                    WXEastEdge_North[2].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXMinusLoadCase_0.Id);
                    WXEastEdge_North[3].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXMinusLoadCase_1.Id);
                    //附加说明
                    WXEastEdge_North[0].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + orderedEastColumns[i].Id.ToString());
                    WXEastEdge_North[1].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + orderedEastColumns[i].Id.ToString());
                    WXEastEdge_North[2].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + orderedEastColumns[i].Id.ToString());
                    WXEastEdge_North[3].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + orderedEastColumns[i].Id.ToString());

                    WXEastEdge_North[0].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WXEastEdge_North[1].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WXEastEdge_North[2].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                    WXEastEdge_North[3].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");

                }
                #endregion

                #region 临界柱 (西)（前）
                int criticalSouth_West = startMiddleY - 1;
                //找Y负向的最近柱并计算距离 
                double disCriticalSouthYMinus_West = SetLoadExtension.CalculateNearestDistance(doc, orderedWestColumns[criticalSouth_West], -XYZ.BasisY, referenceIntersectorAllColumns);
                //找Y正向的最近柱并计算距离
                double disCriticalSouthYPlus_West = SetLoadExtension.CalculateNearestDistance(doc, orderedWestColumns[criticalSouth_West], XYZ.BasisY, referenceIntersectorAllColumns);

                double disCriticalSouth_West = disCriticalSouthYPlus_West / 2 + disCriticalSouthYMinus_West / 2;
                XYZ startCriticalSouth = (orderedWestColumns[criticalSouth_West].Location as LocationPoint).Point;
                double heightCriticalSouth = (doc.GetElement(orderedWestColumns[criticalSouth_West].get_Parameter(BuiltInParameter.SCHEDULE_TOP_LEVEL_PARAM).AsElementId()) as Level).Elevation;
                double offsetCriticalSouth = orderedWestColumns[criticalSouth_West].get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM).AsDouble();
                XYZ endCriticalSouth = (orderedWestColumns[criticalSouth_West].Location as LocationPoint).Point + new XYZ(0, 0, heightCriticalSouth + offsetCriticalSouth);
                LineLoad[] WXWestCritical_South = new LineLoad[4];
                for (int j = 0; j < 4; j++)
                {
                    WXWestCritical_South[j] = LineLoad.Create(doc, startCriticalSouth, endCriticalSouth, XYZ.BasisX, XYZ.Zero, lineLoadType, null);
                }
                double adjustFactorWestCritical_South = SetLoadExtension.GetHeightVariationCoefficientOfWindPressure(doc, orderedWestColumns[criticalSouth_West], groundRoughness);

                //赋予荷载工况 
                WXWestCritical_South[0].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXPlusLoadCase_0.Id);
                WXWestCritical_South[1].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXPlusLoadCase_1.Id);
                WXWestCritical_South[2].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXMinusLoadCase_0.Id);
                WXWestCritical_South[3].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXMinusLoadCase_1.Id);
                //附加说明
                WXWestCritical_South[0].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + orderedWestColumns[criticalSouth_West].Id.ToString());
                WXWestCritical_South[1].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + orderedWestColumns[criticalSouth_West].Id.ToString());
                WXWestCritical_South[2].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + orderedWestColumns[criticalSouth_West].Id.ToString());
                WXWestCritical_South[3].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + orderedWestColumns[criticalSouth_West].Id.ToString());


                WXWestCritical_South[0].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                WXWestCritical_South[1].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                WXWestCritical_South[2].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                WXWestCritical_South[3].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");

                double tempVal9 = UnitExtension.ConvertToMillimeters((orderedWestColumns[criticalSouth_West].Location as LocationPoint).Point.DistanceTo((orderedWestColumns[0].Location as LocationPoint).Point));
                if ( a - tempVal9 < 0)//此时说明a这条分界线在临界柱的南侧
                {

                    //更改荷载大小
                    WXWestCritical_South[0].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).Set((disCriticalSouthYMinus_West / 2 - (tempVal9 -  a)) * factor1E_L_0 * basicWindPressure * adjustFactorWestCritical_South * adjustFactor + ((tempVal9 -  a) + disCriticalSouthYPlus_West / 2) * factor1_L_0 * basicWindPressure * adjustFactorWestCritical_South * adjustFactor);
                    WXWestCritical_South[1].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).Set((disCriticalSouthYMinus_West / 2 - (tempVal9 -  a)) * factor1E_L_1 * basicWindPressure * adjustFactorWestCritical_South * adjustFactor + ((tempVal9 -  a) + disCriticalSouthYPlus_West / 2) * factor1_L_1 * basicWindPressure * adjustFactorWestCritical_South * adjustFactor);
                    WXWestCritical_South[2].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).Set((disCriticalSouthYMinus_West / 2 - (tempVal9 -  a)) * factor4E_L_0 * basicWindPressure * adjustFactorWestCritical_South * adjustFactor + ((tempVal9 -  a) + disCriticalSouthYPlus_West / 2) * factor4_L_0 * basicWindPressure * adjustFactorWestCritical_South * adjustFactor);
                    WXWestCritical_South[3].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).Set((disCriticalSouthYMinus_West / 2 - (tempVal9 -  a)) * factor4E_L_1 * basicWindPressure * adjustFactorWestCritical_South * adjustFactor + ((tempVal9 -  a) + disCriticalSouthYPlus_West / 2) * factor4_L_1 * basicWindPressure * adjustFactorWestCritical_South * adjustFactor);
                }
                else//说明a这条分界线在临界柱的北侧
                {
                    //更改荷载大小
                    WXWestCritical_South[0].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).Set((disCriticalSouthYMinus_West / 2 +  a - tempVal9) * factor1E_L_0 * basicWindPressure * adjustFactorWestCritical_South * adjustFactor + (disCriticalSouthYPlus_West / 2 - ( a - tempVal9)) * factor1_L_0 * basicWindPressure * adjustFactorWestCritical_South * adjustFactor);
                    WXWestCritical_South[1].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).Set((disCriticalSouthYMinus_West / 2 +  a - tempVal9) * factor1E_L_1 * basicWindPressure * adjustFactorWestCritical_South * adjustFactor + (disCriticalSouthYPlus_West / 2 - ( a - tempVal9)) * factor1_L_1 * basicWindPressure * adjustFactorWestCritical_South * adjustFactor);
                    WXWestCritical_South[2].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).Set((disCriticalSouthYMinus_West / 2 +  a - tempVal9) * factor4E_L_0 * basicWindPressure * adjustFactorWestCritical_South * adjustFactor + (disCriticalSouthYPlus_West / 2 - ( a - tempVal9)) * factor4_L_0 * basicWindPressure * adjustFactorWestCritical_South * adjustFactor);
                    WXWestCritical_South[3].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).Set((disCriticalSouthYMinus_West / 2 +  a - tempVal9) * factor4E_L_1 * basicWindPressure * adjustFactorWestCritical_South * adjustFactor + (disCriticalSouthYPlus_West / 2 - ( a - tempVal9)) * factor4_L_1 * basicWindPressure * adjustFactorWestCritical_South * adjustFactor);

                }
                #endregion

                #region 临界柱（西）（后）
                int criticalNorth_West = endMiddleY + 1;
                //找y负向的最近柱并计算距离          
                double disCriticalNorthYMinus_West = SetLoadExtension.CalculateNearestDistance(doc, orderedWestColumns[criticalNorth_West], -XYZ.BasisY, referenceIntersectorAllColumns);
                //找y正向的最近柱并计算距离
                double disCriticalNorthYPlus_West = SetLoadExtension.CalculateNearestDistance(doc, orderedWestColumns[criticalNorth_West], XYZ.BasisY, referenceIntersectorAllColumns);
                double disCriticalNorth_West = disCriticalNorthYPlus_West / 2 + disCriticalNorthYMinus_West / 2;
                XYZ startCriticalNorth = (orderedWestColumns[criticalNorth_West].Location as LocationPoint).Point;
                double heightCriticalNorth = (doc.GetElement(orderedWestColumns[criticalNorth_West].get_Parameter(BuiltInParameter.SCHEDULE_TOP_LEVEL_PARAM).AsElementId()) as Level).Elevation;
                double offsetCriticalNorth = orderedWestColumns[criticalNorth_West].get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM).AsDouble();
                XYZ endCriticalNorth = (orderedWestColumns[criticalNorth_West].Location as LocationPoint).Point + new XYZ(0, 0, heightCriticalNorth + offsetCriticalNorth);
                LineLoad[] WXWestCritical_North = new LineLoad[4];
                for (int j = 0; j < 4; j++)
                {
                    WXWestCritical_North[j] = LineLoad.Create(doc, startCriticalNorth, endCriticalNorth, XYZ.BasisX, XYZ.Zero, lineLoadType, null);
                }
                double adjustFactorWestCritical_North = SetLoadExtension.GetHeightVariationCoefficientOfWindPressure(doc, orderedWestColumns[criticalNorth_West], groundRoughness);

                //赋予荷载工况 
                WXWestCritical_North[0].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXPlusLoadCase_0.Id);
                WXWestCritical_North[1].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXPlusLoadCase_1.Id);
                WXWestCritical_North[2].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXMinusLoadCase_0.Id);
                WXWestCritical_North[3].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXMinusLoadCase_1.Id);
                //附加说明
                WXWestCritical_North[0].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + orderedWestColumns[criticalNorth_West].Id.ToString());
                WXWestCritical_North[1].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + orderedWestColumns[criticalNorth_West].Id.ToString());
                WXWestCritical_North[2].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + orderedWestColumns[criticalNorth_West].Id.ToString());
                WXWestCritical_North[3].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + orderedWestColumns[criticalNorth_West].Id.ToString());


                WXWestCritical_North[0].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                WXWestCritical_North[1].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                WXWestCritical_North[2].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                WXWestCritical_North[3].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");

                double tempVal10 = UnitExtension.ConvertToMillimeters((orderedWestColumns[criticalNorth_West].Location as LocationPoint).Point.DistanceTo((orderedWestColumns[orderedWestColumns.Count-1].Location as LocationPoint).Point));
                if (a - tempVal10 < 0)//此时说明a这条分界线在临界柱的北侧
                {

                    //更改荷载大小
                    WXWestCritical_North[0].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).Set((disCriticalNorthYMinus_West / 2 + (tempVal10 -  a)) * factor1_L_0 * basicWindPressure * adjustFactorWestCritical_North * adjustFactor + (disCriticalNorthYPlus_West / 2 - (tempVal10 -  a)) * factor1E_L_0 * basicWindPressure * adjustFactorWestCritical_North * adjustFactor);
                    WXWestCritical_North[1].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).Set((disCriticalNorthYMinus_West / 2 + (tempVal10 -  a)) * factor1_L_1 * basicWindPressure * adjustFactorWestCritical_North * adjustFactor + (disCriticalNorthYPlus_West / 2 - (tempVal10 -  a)) * factor1E_L_1 * basicWindPressure * adjustFactorWestCritical_North * adjustFactor);
                    WXWestCritical_North[2].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).Set((disCriticalNorthYMinus_West / 2 + (tempVal10 -  a)) * factor4_L_0 * basicWindPressure * adjustFactorWestCritical_North * adjustFactor + (disCriticalNorthYPlus_West / 2 - (tempVal10 -  a)) * factor4E_L_0 * basicWindPressure * adjustFactorWestCritical_North * adjustFactor);
                    WXWestCritical_North[3].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).Set((disCriticalNorthYMinus_West / 2 + (tempVal10 -  a)) * factor4_L_1 * basicWindPressure * adjustFactorWestCritical_North * adjustFactor + (disCriticalNorthYPlus_West / 2 - (tempVal10 -  a)) * factor4E_L_1 * basicWindPressure * adjustFactorWestCritical_North * adjustFactor);
                }
                else//说明a这条分界线在临界柱的南侧
                {
                    //更改荷载大小
                    WXWestCritical_North[0].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).Set((disCriticalNorthYPlus_West / 2 + a - tempVal10) * factor1E_L_0 * basicWindPressure * adjustFactorWestCritical_North * adjustFactor + (disCriticalNorthYMinus_West/ 2 - (a - tempVal10)) * factor1_L_0 * basicWindPressure * adjustFactorWestCritical_North * adjustFactor);
                    WXWestCritical_North[1].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).Set((disCriticalNorthYPlus_West / 2 + a - tempVal10) * factor1E_L_1 * basicWindPressure * adjustFactorWestCritical_North * adjustFactor + (disCriticalNorthYMinus_West/ 2 - (a - tempVal10)) * factor1_L_1 * basicWindPressure * adjustFactorWestCritical_North * adjustFactor);
                    WXWestCritical_North[2].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).Set((disCriticalNorthYPlus_West / 2 + a - tempVal10) * factor4E_L_0 * basicWindPressure * adjustFactorWestCritical_North * adjustFactor + (disCriticalNorthYMinus_West/ 2 - (a - tempVal10)) * factor4_L_0 * basicWindPressure * adjustFactorWestCritical_North * adjustFactor);
                    WXWestCritical_North[3].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).Set((disCriticalNorthYPlus_West / 2 + a - tempVal10) * factor4E_L_1 * basicWindPressure * adjustFactorWestCritical_North * adjustFactor + (disCriticalNorthYMinus_West/ 2 - (a - tempVal10)) * factor4_L_1 * basicWindPressure * adjustFactorWestCritical_North * adjustFactor);

                }
                #endregion

                #region 临界柱 (东)（前）
                int criticalSouth_East = startMiddleY - 1;
                //找y负向的最近柱并计算距离            
                double disCriticalSouthYMinus_East= SetLoadExtension.CalculateNearestDistance(doc, orderedEastColumns[criticalSouth_East], -XYZ.BasisY, referenceIntersectorAllColumns);
                //找y正向的最近柱并计算距离
                double disCriticalSouthYPlus_East = SetLoadExtension.CalculateNearestDistance(doc, orderedEastColumns[criticalSouth_East], XYZ.BasisY, referenceIntersectorAllColumns);
                double disCriticalSouth_East = disCriticalSouthYPlus_East / 2 + disCriticalSouthYMinus_East / 2;
                XYZ startCriticalSouth1 = (orderedEastColumns[criticalSouth_East].Location as LocationPoint).Point;
                double heightCriticalSouth1 = (doc.GetElement(orderedEastColumns[criticalSouth_East].get_Parameter(BuiltInParameter.SCHEDULE_TOP_LEVEL_PARAM).AsElementId()) as Level).Elevation;
                double offsetCriticalSouth1 = orderedEastColumns[criticalSouth_East].get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM).AsDouble();
                XYZ endCriticalSouth1 = (orderedEastColumns[criticalSouth_East].Location as LocationPoint).Point + new XYZ(0, 0, heightCriticalSouth1 + offsetCriticalSouth1);
                LineLoad[] WXEastCritical_South = new LineLoad[4];
                for (int j = 0; j < 4; j++)
                {
                    WXEastCritical_South[j]= LineLoad.Create(doc, startCriticalSouth1, endCriticalSouth1, XYZ.BasisX, XYZ.Zero, lineLoadType, null);
                }
                double adjustFactorEastCritical_South = SetLoadExtension.GetHeightVariationCoefficientOfWindPressure(doc, orderedEastColumns[criticalSouth_East], groundRoughness);

                //赋予荷载工况 
                WXEastCritical_South[0].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXPlusLoadCase_0.Id);
                WXEastCritical_South[1].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXPlusLoadCase_1.Id);
                WXEastCritical_South[2].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXMinusLoadCase_0.Id);
                WXEastCritical_South[3].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXMinusLoadCase_1.Id);
                //附加说明
                WXEastCritical_South[0].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + orderedEastColumns[criticalSouth_East].Id.ToString());
                WXEastCritical_South[1].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + orderedEastColumns[criticalSouth_East].Id.ToString());
                WXEastCritical_South[2].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + orderedEastColumns[criticalSouth_East].Id.ToString());
                WXEastCritical_South[3].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + orderedEastColumns[criticalSouth_East].Id.ToString());

                WXEastCritical_South[0].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                WXEastCritical_South[1].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                WXEastCritical_South[2].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                WXEastCritical_South[3].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                double tempVal11 = UnitExtension.ConvertToMillimeters((orderedEastColumns[criticalSouth_East].Location as LocationPoint).Point.DistanceTo((orderedEastColumns[0].Location as LocationPoint).Point));
                if (a - tempVal11 < 0)//此时说明a这条分界线在临界柱的南侧
                {

                    //更改荷载大小
                    WXEastCritical_South[0].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).Set((disCriticalSouthYMinus_East / 2 - (tempVal11 - a)) * -factor4E_L_0 * basicWindPressure * adjustFactorEastCritical_South * adjustFactor + ((tempVal11 - a) + disCriticalSouthYPlus_East / 2) * -factor4_L_0 * basicWindPressure * adjustFactorEastCritical_South * adjustFactor);
                    WXEastCritical_South[1].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).Set((disCriticalSouthYMinus_East / 2 - (tempVal11 - a)) * -factor4E_L_1 * basicWindPressure * adjustFactorEastCritical_South * adjustFactor + ((tempVal11 - a) + disCriticalSouthYPlus_East / 2) * -factor4_L_1 * basicWindPressure * adjustFactorEastCritical_South * adjustFactor);
                    WXEastCritical_South[2].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).Set((disCriticalSouthYMinus_East / 2 - (tempVal11 - a)) * -factor1E_L_0 * basicWindPressure * adjustFactorEastCritical_South * adjustFactor + ((tempVal11 - a) + disCriticalSouthYPlus_East / 2) * -factor1_L_0 * basicWindPressure * adjustFactorEastCritical_South * adjustFactor);
                    WXEastCritical_South[3].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).Set((disCriticalSouthYMinus_East / 2 - (tempVal11 - a)) * -factor1E_L_1 * basicWindPressure * adjustFactorEastCritical_South * adjustFactor + ((tempVal11 - a) + disCriticalSouthYPlus_East / 2) * -factor1_L_1 * basicWindPressure * adjustFactorEastCritical_South * adjustFactor);
                }
                else//说明a这条分界线在临界柱的北侧
                {
                    //更改荷载大小
                    WXEastCritical_South[0].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).Set((disCriticalSouthYMinus_East / 2 + a - tempVal11) * -factor4E_L_0 * basicWindPressure * adjustFactorEastCritical_South * adjustFactor + (disCriticalSouthYPlus_East / 2 - (a - tempVal11)) * -factor4_L_0 * basicWindPressure * adjustFactorEastCritical_South * adjustFactor);
                    WXEastCritical_South[1].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).Set((disCriticalSouthYMinus_East / 2 + a - tempVal11) * -factor4E_L_1 * basicWindPressure * adjustFactorEastCritical_South * adjustFactor + (disCriticalSouthYPlus_East / 2 - (a - tempVal11)) * -factor4_L_1 * basicWindPressure * adjustFactorEastCritical_South * adjustFactor);
                    WXEastCritical_South[2].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).Set((disCriticalSouthYMinus_East / 2 + a - tempVal11) * -factor1E_L_0 * basicWindPressure * adjustFactorEastCritical_South * adjustFactor + (disCriticalSouthYPlus_East / 2 - (a - tempVal11)) * -factor1_L_0 * basicWindPressure * adjustFactorEastCritical_South * adjustFactor);
                    WXEastCritical_South[3].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).Set((disCriticalSouthYMinus_East / 2 + a - tempVal11) * -factor1E_L_1 * basicWindPressure * adjustFactorEastCritical_South * adjustFactor + (disCriticalSouthYPlus_East / 2 - (a - tempVal11)) * -factor1_L_1 * basicWindPressure * adjustFactorEastCritical_South * adjustFactor);

                }
                #endregion

                #region 临界柱（东）（后）
                int criticalNorth_East = endMiddleY + 1;
                //找y负向的最近柱并计算距离
                double disCriticalNorthYMinus_East = SetLoadExtension.CalculateNearestDistance(doc, orderedEastColumns[criticalNorth_East], -XYZ.BasisY, referenceIntersectorAllColumns);
                //找y正向的最近柱并计算距离
                double disCriticalNorthYPlus_East = SetLoadExtension.CalculateNearestDistance(doc, orderedEastColumns[criticalNorth_East], XYZ.BasisY, referenceIntersectorAllColumns);
                double disCriticalNorth1 =disCriticalNorthYPlus_East / 2 + disCriticalNorthYMinus_East / 2;
                XYZ startCriticalNorth1 = (orderedEastColumns[criticalNorth_East].Location as LocationPoint).Point;
                double heightCriticalNorth1 = (doc.GetElement(orderedEastColumns[criticalNorth_East].get_Parameter(BuiltInParameter.SCHEDULE_TOP_LEVEL_PARAM).AsElementId()) as Level).Elevation;
                double offsetCriticalNorth1 = orderedEastColumns[criticalNorth_East].get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_OFFSET_PARAM).AsDouble();
                XYZ endCriticalNorth1 = (orderedEastColumns[criticalNorth_East].Location as LocationPoint).Point + new XYZ(0, 0, heightCriticalNorth1 + offsetCriticalNorth1);

                LineLoad[] WXEastCritical_North = new LineLoad[4];
                for (int j = 0; j < 4; j++)
                {
                    WXEastCritical_North[j] = LineLoad.Create(doc, startCriticalNorth1, endCriticalNorth1, XYZ.BasisX, XYZ.Zero, lineLoadType, null);
                }
                double adjustFactorEastCritical_North = SetLoadExtension.GetHeightVariationCoefficientOfWindPressure(doc, orderedEastColumns[criticalNorth_East], groundRoughness);

                //赋予荷载工况 
                WXEastCritical_North[0].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXPlusLoadCase_0.Id);
                WXEastCritical_North[1].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXPlusLoadCase_1.Id);
                WXEastCritical_North[2].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXMinusLoadCase_0.Id);
                WXEastCritical_North[3].get_Parameter(BuiltInParameter.LOAD_CASE_ID).Set(windXMinusLoadCase_1.Id);
                //附加说明
                WXEastCritical_North[0].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + orderedEastColumns[criticalNorth_East].Id.ToString());
                WXEastCritical_North[1].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + orderedEastColumns[criticalNorth_East].Id.ToString());
                WXEastCritical_North[2].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + orderedEastColumns[criticalNorth_East].Id.ToString());
                WXEastCritical_North[3].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).Set("ParentId:" + orderedEastColumns[criticalNorth_East].Id.ToString());

                WXEastCritical_North[0].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                WXEastCritical_North[1].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                WXEastCritical_North[2].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");
                WXEastCritical_North[3].get_Parameter(BuiltInParameter.LOAD_COMMENTS).Set("Scope:(0,1)");

                double tempVal12 = UnitExtension.ConvertToMillimeters((orderedEastColumns[criticalNorth_East].Location as LocationPoint).Point.DistanceTo((orderedEastColumns[orderedEastColumns.Count - 1].Location as LocationPoint).Point));
                if (a - tempVal12 < 0)//此时说明a这条分界线在临界柱的北侧
                {

                    //更改荷载大小
                    WXEastCritical_North[0].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).Set((disCriticalNorthYMinus_East / 2 + (tempVal12 - a)) * -factor4_L_0 * basicWindPressure * adjustFactorEastCritical_North * adjustFactor + (disCriticalNorthYPlus_East / 2 - (tempVal12 - a)) * -factor4E_L_0 * basicWindPressure * adjustFactorEastCritical_North * adjustFactor);
                    WXEastCritical_North[1].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).Set((disCriticalNorthYMinus_East / 2 + (tempVal12 - a)) * -factor4_L_1 * basicWindPressure * adjustFactorEastCritical_North * adjustFactor + (disCriticalNorthYPlus_East / 2 - (tempVal12 - a)) * -factor4E_L_1 * basicWindPressure * adjustFactorEastCritical_North * adjustFactor);
                    WXEastCritical_North[2].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).Set((disCriticalNorthYMinus_East / 2 + (tempVal12 - a)) * -factor1_L_0 * basicWindPressure * adjustFactorEastCritical_North * adjustFactor + (disCriticalNorthYPlus_East / 2 - (tempVal12 - a)) * -factor1E_L_0 * basicWindPressure * adjustFactorEastCritical_North * adjustFactor);
                    WXEastCritical_North[3].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).Set((disCriticalNorthYMinus_East / 2 + (tempVal12 - a)) * -factor1_L_1 * basicWindPressure * adjustFactorEastCritical_North * adjustFactor + (disCriticalNorthYPlus_East / 2 - (tempVal12 - a)) * -factor1E_L_1 * basicWindPressure * adjustFactorEastCritical_North * adjustFactor);
                }
                else//说明a这条分界线在临界柱的南侧
                {
                    //更改荷载大小
                    WXEastCritical_North[0].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).Set((disCriticalNorthYPlus_East / 2 + a - tempVal12) * -factor4E_L_0 * basicWindPressure * adjustFactorEastCritical_North * adjustFactor + (disCriticalNorthYMinus_East / 2 - (a - tempVal12)) * -factor4_L_0 * basicWindPressure * adjustFactorEastCritical_North * adjustFactor);
                    WXEastCritical_North[1].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).Set((disCriticalNorthYPlus_East / 2 + a - tempVal12) * -factor4E_L_0 * basicWindPressure * adjustFactorEastCritical_North * adjustFactor + (disCriticalNorthYMinus_East / 2 - (a - tempVal12)) * -factor4_L_1 * basicWindPressure * adjustFactorEastCritical_North * adjustFactor);
                    WXEastCritical_North[2].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).Set((disCriticalNorthYPlus_East / 2 + a - tempVal12) * -factor1E_L_0 * basicWindPressure * adjustFactorEastCritical_North * adjustFactor + (disCriticalNorthYMinus_East / 2 - (a - tempVal12)) * -factor1_L_0 * basicWindPressure * adjustFactorEastCritical_North * adjustFactor);
                    WXEastCritical_North[3].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).Set((disCriticalNorthYPlus_East / 2 + a - tempVal12) * -factor1E_L_0 * basicWindPressure * adjustFactorEastCritical_North * adjustFactor + (disCriticalNorthYMinus_East / 2 - (a - tempVal12)) * -factor1_L_1 * basicWindPressure * adjustFactorEastCritical_North * adjustFactor);

                }
                #endregion

                #endregion



                ts.Commit();




            }

            #endregion
            TextDialog textDialog = new TextDialog("荷载施加成功");
            textDialog.ShowDialog();

            #region 将荷载在除了“分析模型”以外的视图上进行隐藏
            using (Transaction ts=new Transaction(doc,"隐藏荷载"))
            {
                ts.Start();
                FilteredElementCollector viewFilter = new FilteredElementCollector(doc).OfClass(typeof(View));
                List<View> views = viewFilter.Cast<View>().Where(t => t.Name != "分析模型").ToList();
                FilteredElementCollector lineLoadFilter = new FilteredElementCollector(doc).OfClass(typeof(LineLoad));
                List<LineLoad> lineLoads = lineLoadFilter.Cast<LineLoad>().ToList();
                List<ElementId> lineLoadIds = new List<ElementId>();
                for (int i = 0; i < lineLoads.Count; i++)
                {
                    lineLoadIds.Add(lineLoads[i].Id);
                }

                for (int i = 0; i < views.Count; i++)
                {
                    views[i].HideElements(lineLoadIds);
                }
                ts.Commit();
            }
           
            #endregion
        }
    }
}
