using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using GalaSoft.MvvmLight.Messaging;
using HandyControl.Controls;
using HandyControl.Properties.Langs;
using SAP2000v1;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using Test.Common;
using Test.Extensions;

namespace Test.UserControls
{
    /// <summary>
    /// SAP2000AnalysisView.xaml 的交互逻辑
    /// </summary>
    public partial class SAP2000AnalysisView :System.Windows.Controls.UserControl
    {
        private static SAP2000AnalysisView _instance;
        public UIDocument uidoc { get; set; }

        private SAP2000AnalysisView()
        {
            InitializeComponent();
        }
        public static SAP2000AnalysisView GetInstance()
        {
            if (_instance == null)
            {
                _instance = new SAP2000AnalysisView();
            }
            return _instance;
        }
        public void Initial(UIDocument uidoc)
        {
            this.uidoc = uidoc;
            FoldPath.Text = @"C:\Users\15389\Desktop\Dic";
            FoldName.Text = "Test";
            Desc.Items.Add("周期信息");
            Desc.Items.Add("单工况内力信息");
            Desc.Items.Add("基本组合内力信息");
            Desc.SelectedIndex = 0;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            Messenger.Default.Send<bool>(true, Token.LoadSAP2000Anlysis);
            await Task.Run(SAP2000Analysis);
        }

        private void SAP2000Analysis()
        {
            {             
                int ret = 0;
                cHelper Helper = new Helper();
                cOAPI sapObject = Helper.CreateObjectProgID("CSI.SAP2000.API.SapObject");
                //cOAPI mySapObject = myHelper.CreateObject(@"C:\Program Files\Computers and Structures\SAP2000 23\SAP2000.exe");
                //sap2000 oapi中方法的返回值均为int，若方法执行成功返回值为0，不成功则为非0数。故定义ret，保存方法是否执行成功。
                ret = sapObject.ApplicationStart(eUnits.kN_m_C);//打开sap2000
                cSapModel sapModel = sapObject.SapModel;//通过object拿到model
                ret = sapModel.File.NewBlank(); //在SAP2000中的文件下拉列表中选择新模型并选择模板，这里选择三维框架空模板


                #region 荷载模式
                //荷载模式 添加Dead,Live,WX+(+i),WX+(-i),WX-(+i),WX-(-i),WY+(+i),WY+(-i),WY-(+i),WY-(-i)
                ret = sapModel.LoadPatterns.ChangeName("DEAD", "Dead");
                ret = sapModel.LoadPatterns.Add("Live", eLoadPatternType.Live);
                ret = sapModel.LoadPatterns.Add("WX+(+i)", eLoadPatternType.Wind);
                ret = sapModel.LoadPatterns.Add("WX+(-i)", eLoadPatternType.Wind);
                ret = sapModel.LoadPatterns.Add("WX-(+i)", eLoadPatternType.Wind);
                ret = sapModel.LoadPatterns.Add("WX-(-i)", eLoadPatternType.Wind);
                ret = sapModel.LoadPatterns.Add("WY+(+i)", eLoadPatternType.Wind);
                ret = sapModel.LoadPatterns.Add("WY+(-i)", eLoadPatternType.Wind);
                ret = sapModel.LoadPatterns.Add("WY-(+i)", eLoadPatternType.Wind);
                ret = sapModel.LoadPatterns.Add("WY-(-i)", eLoadPatternType.Wind);

                #endregion

                #region 生成刚架柱同时生成各刚架柱上的荷载
                //刚架柱生成
                string sqlForColumnInfo = "select * from ColumnInfo";
                DBHelper.PrepareSql(sqlForColumnInfo);
                DataTable dtForColumnInfo = DBHelper.ExecQuery();
                for (int i = 0; i < dtForColumnInfo.Rows.Count; i++)
                {
                    int Id = Convert.ToInt32(dtForColumnInfo.Rows[i]["Id"]);
                    string frameSection = SAP2000Extension.GetSection(sapModel, dtForColumnInfo.Rows[i]["SectionShape"].ToString(), dtForColumnInfo.Rows[i]["sectionDimension"].ToString(), dtForColumnInfo.Rows[i]["material"].ToString());
                    double x1 = Convert.ToDouble(dtForColumnInfo.Rows[i]["startX"]) / 1000;
                    double y1 = Convert.ToDouble(dtForColumnInfo.Rows[i]["startY"]) / 1000;
                    double z1 = Convert.ToDouble(dtForColumnInfo.Rows[i]["startZ"]) / 1000;
                    double x2 = Convert.ToDouble(dtForColumnInfo.Rows[i]["endX"]) / 1000;
                    double y2 = Convert.ToDouble(dtForColumnInfo.Rows[i]["endY"]) / 1000;
                    double z2 = Convert.ToDouble(dtForColumnInfo.Rows[i]["endZ"]) / 1000;
                    string elementName = null;
                    //创建对象
                    ret = SAP2000Extension.CreateElement(sapModel, x1, y1, z1, x2, y2, z2, frameSection, ref elementName);
                    //针对主刚架需绕主轴旋转
                    ret = sapModel.FrameObj.SetLocalAxes(elementName, 90);
                    //设置荷载
                    ret = SAP2000Extension.SetLoad(sapModel, elementName, Id);
                    //设置与地连接方式
                    string point1 = null;
                    string point2 = null;
                    sapModel.FrameObj.GetPoints(elementName, ref point1, ref point2);
                    bool[] values = new bool[6] { true, true, true, false, false, false };
                    sapModel.PointObj.SetRestraint(point1, ref values);
                    //创建Revit ElementId与SAP 2000 Frame Text的对应关系
                    string sqlForColumnRelationship = "insert into Revit_SAP2000_Element_Relationship(Revit_Element_Id,SAP2000_Element_Id) " +
                                                     "values(@Revit_Element_Id,@SAP2000_Element_Id)";
                    DBHelper.PrepareSql(sqlForColumnRelationship);
                    DBHelper.SetParameter("Revit_Element_Id", Id);
                    DBHelper.SetParameter("SAP2000_Element_Id", elementName);
                    DBHelper.ExecNonQuery();
                }
                #endregion

                #region 生成抗风柱同时生成抗风柱的荷载
                //抗风柱生成

                string sqlForWindResistantColumnInfo = "select * from WindResistantColumnInfo";
                DBHelper.PrepareSql(sqlForWindResistantColumnInfo);
                DataTable dtForWindResistantColumn = DBHelper.ExecQuery();
                for (int i = 0; i < dtForWindResistantColumn.Rows.Count; i++)
                {
                    int Id = Convert.ToInt32(dtForWindResistantColumn.Rows[i]["Id"]);
                    string frameSection = SAP2000Extension.GetSection(sapModel, dtForWindResistantColumn.Rows[i]["SectionShape"].ToString(), dtForWindResistantColumn.Rows[i]["sectionDimension"].ToString(), dtForWindResistantColumn.Rows[i]["material"].ToString());
                    double x1 = Convert.ToDouble(dtForWindResistantColumn.Rows[i]["startX"]) / 1000;
                    double y1 = Convert.ToDouble(dtForWindResistantColumn.Rows[i]["startY"]) / 1000;
                    double z1 = Convert.ToDouble(dtForWindResistantColumn.Rows[i]["startZ"]) / 1000;
                    double x2 = Convert.ToDouble(dtForWindResistantColumn.Rows[i]["endX"]) / 1000;
                    double y2 = Convert.ToDouble(dtForWindResistantColumn.Rows[i]["endY"]) / 1000;
                    double z2 = Convert.ToDouble(dtForWindResistantColumn.Rows[i]["endZ"]) / 1000;
                    string elementName = null;
                    ret = SAP2000Extension.CreateElement(sapModel, x1, y1, z1, x2, y2, z2, frameSection, ref elementName);
                    ret = SAP2000Extension.SetLoad(sapModel, elementName, Id);
                    //设置与地连接方式
                    string point1 = null;
                    string point2 = null;
                    sapModel.FrameObj.GetPoints(elementName, ref point1, ref point2);
                    bool[] values = new bool[6] { true, true, true, false, false, false };
                    sapModel.PointObj.SetRestraint(point1, ref values);
                    //端部释放使得刚架柱上端变成滑动支座
                    bool[] ii = new bool[6] { false, false, false, false, true, true };
                    bool[] jj = new bool[6] { true, false, false, true, true, true };
                    double[] start = new double[6] { 1, 1, 1, 1, 0, 0 };
                    double[] end = new double[6] { 0, 1, 1, 0, 0, 0 };
                    ret = sapModel.FrameObj.SetReleases(elementName, ref ii, ref jj, ref start, ref end);
                    //创建Revit ElementId与SAP 2000 Frame Text的对应关系
                    string sqlForColumnRelationship = "insert into Revit_SAP2000_Element_Relationship(Revit_Element_Id,SAP2000_Element_Id) " +
                                                     "values(@Revit_Element_Id,@SAP2000_Element_Id)";
                    DBHelper.PrepareSql(sqlForColumnRelationship);
                    DBHelper.SetParameter("Revit_Element_Id", Id);
                    DBHelper.SetParameter("SAP2000_Element_Id", elementName);
                    DBHelper.ExecNonQuery();

                }

                #endregion

                #region 生成刚架梁同时生成各刚架梁上的荷载
                //刚架梁生成
                string sqlForBeam = "select * from BeamInfo";
                DBHelper.PrepareSql(sqlForBeam);
                DataTable dtForBeam = DBHelper.ExecQuery();
                for (int i = 0; i < dtForBeam.Rows.Count; i++)
                {
                    int Id = Convert.ToInt32(dtForBeam.Rows[i]["Id"]);
                    string frameSection = SAP2000Extension.GetSection(sapModel, dtForBeam.Rows[i]["SectionShape"].ToString(), dtForBeam.Rows[i]["sectionDimension"].ToString(), dtForBeam.Rows[i]["material"].ToString());
                    double x1 = Convert.ToDouble(dtForBeam.Rows[i]["startX"]) / 1000;
                    double y1 = Convert.ToDouble(dtForBeam.Rows[i]["startY"]) / 1000;
                    double z1 = Convert.ToDouble(dtForBeam.Rows[i]["startZ"]) / 1000;
                    double x2 = Convert.ToDouble(dtForBeam.Rows[i]["endX"]) / 1000;
                    double y2 = Convert.ToDouble(dtForBeam.Rows[i]["endY"]) / 1000;
                    double z2 = Convert.ToDouble(dtForBeam.Rows[i]["endZ"]) / 1000;
                    string elementName = null;
                    ret = SAP2000Extension.CreateElement(sapModel, x1, y1, z1, x2, y2, z2, frameSection, ref elementName);
                    ret = SAP2000Extension.SetLoad(sapModel, elementName, Id);
                    //创建Revit ElementId与SAP 2000 Frame Text的对应关系
                    string sqlForColumnRelationship = "insert into Revit_SAP2000_Element_Relationship(Revit_Element_Id,SAP2000_Element_Id) " +
                                                     "values(@Revit_Element_Id,@SAP2000_Element_Id)";
                    DBHelper.PrepareSql(sqlForColumnRelationship);
                    DBHelper.SetParameter("Revit_Element_Id", Id);
                    DBHelper.SetParameter("SAP2000_Element_Id", elementName);
                    DBHelper.ExecNonQuery();

                }

                #endregion

                #region 生成系杆，系杆不作为直接受力构件
                //系杆生成
                string sqlForTie = "select * from TieInfo";
                DBHelper.PrepareSql(sqlForTie);
                DataTable dtForTie = DBHelper.ExecQuery();
                for (int i = 0; i < dtForTie.Rows.Count; i++)
                {
                    int Id = Convert.ToInt32(dtForTie.Rows[i]["Id"]);
                    string frameSection = SAP2000Extension.GetSection(sapModel, dtForTie.Rows[i]["SectionShape"].ToString(), dtForTie.Rows[i]["sectionDimension"].ToString(), dtForTie.Rows[i]["material"].ToString());
                    double x1 = Convert.ToDouble(dtForTie.Rows[i]["startX"]) / 1000;
                    double y1 = Convert.ToDouble(dtForTie.Rows[i]["startY"]) / 1000;
                    double z1 = Convert.ToDouble(dtForTie.Rows[i]["startZ"]) / 1000;
                    double x2 = Convert.ToDouble(dtForTie.Rows[i]["endX"]) / 1000;
                    double y2 = Convert.ToDouble(dtForTie.Rows[i]["endY"]) / 1000;
                    double z2 = Convert.ToDouble(dtForTie.Rows[i]["endZ"]) / 1000;
                    string elementName = null;
                    ret = SAP2000Extension.CreateElement(sapModel, x1, y1, z1, x2, y2, z2, frameSection, ref elementName);
                    //端部释放使得两端变成铰接
                    bool[] ii = new bool[6] { false, false, false, false, true, true };
                    bool[] jj = new bool[6] { false, false, false, true, true, true };
                    double[] start = new double[6] { 1, 1, 1, 1, 0, 0 };
                    double[] end = new double[6] { 1, 1, 1, 0, 0, 0 };
                    ret = sapModel.FrameObj.SetReleases(elementName, ref ii, ref jj, ref start, ref end);
                    //创建Revit ElementId与SAP 2000 Frame Text的对应关系
                    string sqlForColumnRelationship = "insert into Revit_SAP2000_Element_Relationship(Revit_Element_Id,SAP2000_Element_Id) " +
                                                     "values(@Revit_Element_Id,@SAP2000_Element_Id)";
                    DBHelper.PrepareSql(sqlForColumnRelationship);
                    DBHelper.SetParameter("Revit_Element_Id", Id);
                    DBHelper.SetParameter("SAP2000_Element_Id", elementName);
                    DBHelper.ExecNonQuery();

                }
                #endregion

                #region 生成支撑，支撑不作为直接受力构件
                //支撑生成
                string sqlForBracing = "select * from BracingInfo";
                DBHelper.PrepareSql(sqlForBracing);
                DataTable dtForBracing = DBHelper.ExecQuery();
                for (int i = 0; i < dtForBracing.Rows.Count; i++)
                {
                    int Id = Convert.ToInt32(dtForBracing.Rows[i]["Id"]);
                    string frameSection = SAP2000Extension.GetSection(sapModel, dtForBracing.Rows[i]["SectionShape"].ToString(), dtForBracing.Rows[i]["sectionDimension"].ToString(), dtForBracing.Rows[i]["material"].ToString());
                    double x1 = Convert.ToDouble(dtForBracing.Rows[i]["startX"]) / 1000;
                    double y1 = Convert.ToDouble(dtForBracing.Rows[i]["startY"]) / 1000;
                    double z1 = Convert.ToDouble(dtForBracing.Rows[i]["startZ"]) / 1000;
                    double x2 = Convert.ToDouble(dtForBracing.Rows[i]["endX"]) / 1000;
                    double y2 = Convert.ToDouble(dtForBracing.Rows[i]["endY"]) / 1000;
                    double z2 = Convert.ToDouble(dtForBracing.Rows[i]["endZ"]) / 1000;
                    string elementName = null;
                    ret = SAP2000Extension.CreateElement(sapModel, x1, y1, z1, x2, y2, z2, frameSection, ref elementName);
                    //端部释放使得两端变成铰接
                    bool[] ii = new bool[6] { false, false, false, false, true, true };
                    bool[] jj = new bool[6] { false, false, false, true, true, true };
                    double[] start = new double[6] { 1, 1, 1, 1, 0, 0 };
                    double[] end = new double[6] { 1, 1, 1, 0, 0, 0 };
                    ret = sapModel.FrameObj.SetReleases(elementName, ref ii, ref jj, ref start, ref end);
                    //创建Revit ElementId与SAP 2000 Frame Text的对应关系
                    string sqlForColumnRelationship = "insert into Revit_SAP2000_Element_Relationship(Revit_Element_Id,SAP2000_Element_Id) " +
                                                     "values(@Revit_Element_Id,@SAP2000_Element_Id)";
                    DBHelper.PrepareSql(sqlForColumnRelationship);
                    DBHelper.SetParameter("Revit_Element_Id", Id);
                    DBHelper.SetParameter("SAP2000_Element_Id", elementName);
                    DBHelper.ExecNonQuery();

                }
                #endregion

                #region 地震作用
                //定义结构质量  默认：1.0恒+1.5活
                string[] loadPatternForMass = new string[2] { "Dead", "Live" };
                Double[] sf = new double[2] { 1, 0.5 };
                ret = sapModel.SourceMass.SetMassSource("mass", false, false, true, false, 2, ref loadPatternForMass, ref sf);
                ret = sapModel.SourceMass.SetDefault("mass");
                //定义反应谱函数,所需参数：特征周期，周期折减系数，阻尼比
                string sqlForEarthquake = "select * from EarthquakeInfo";
                DBHelper.PrepareSql(sqlForEarthquake);
                DataTable seismicDt = DBHelper.ExecQuery();
                double AlphaMax = Convert.ToDouble(seismicDt.Rows[0]["AlphaMax"]);
                string SITemp = seismicDt.Rows[0]["SeismicIntensity"].ToString();
                int SI;
                switch (SITemp)
                {
                    case "6(0.05g)": SI = 1; break;
                    case "7(0.10g)": SI = 2; break;
                    case "7(0.15g)": SI = 3; break;
                    case "8(0.20g)": SI = 4; break;
                    case "8(0.30g)": SI = 5; break;
                    case "9(0.40g)": SI = 6; break;
                    default: SI = 2; break;
                }
                double Tg = Convert.ToDouble(seismicDt.Rows[0]["CharacteristicPeriod"]);
                double PTDF = Convert.ToDouble(seismicDt.Rows[0]["PeriodTimeReductionFactor"]);
                double DampRatio = Convert.ToDouble(seismicDt.Rows[0]["DampingRatio"]);
                ret = sapModel.Func.FuncRS.SetChinese2010("FUNC", AlphaMax, SI, Tg, PTDF, DampRatio);
                //定义地震工况
                //LoadName:This is an array that includes U1, U2, U3, R1, R2 or R3, indicating the direction of the load.
                string[] loadNameForEX = new string[1] { "U1" };
                string[] loadNameForEY = new string[1] { "U2" };
                //Func:This is an array that includes the name of the response spectrum function associated with each load.
                string[] Func = new string[1] { "FUNC" };
                //SF:This is an array that includes the scale factor of each load assigned to the load case
                double[] sfforE = new double[1] { 9.8 };
                //CSys:This is an array that includes the name of the coordinate system associated with each load. If this item is a blank string, the Global coordinate system is assumed.
                string[] cSys = new string[] { "" };
                //Ang:This is an array that includes the angle between the acceleration local 1 axis and the + X - axis of the coordinate system specified by the CSys item.The rotation is about the Z - axis of the specified coordinate system.
                double[] angle = new double[] { 0 };
                ret = sapModel.LoadCases.ResponseSpectrum.SetCase("EX");
                ret = sapModel.LoadCases.ResponseSpectrum.SetLoads("EX", 1, ref loadNameForEX, ref Func, ref sfforE, ref cSys, ref angle);
                ret = sapModel.LoadCases.ResponseSpectrum.SetCase("EY");
                ret = sapModel.LoadCases.ResponseSpectrum.SetLoads("EY", 1, ref loadNameForEY, ref Func, ref sfforE, ref cSys, ref angle);

                #endregion

                #region 荷载组合
                string sqlForLoadCombination = "select * from loadCombinationInfo";
                DBHelper.PrepareSql(sqlForLoadCombination);
                DataTable dtForLoadCombination = DBHelper.ExecQuery();
                for (int i = 0; i < dtForLoadCombination.Rows.Count; i++)
                {
                    string name = dtForLoadCombination.Rows[i]["Tag"].ToString();
                    ret = sapModel.RespCombo.Add(name, 0);
                    eCNameType type = eCNameType.LoadCase;
                    double deadFactor = Convert.ToDouble(dtForLoadCombination.Rows[i]["Dead"]);
                    if (deadFactor != 0)
                    {
                        ret = sapModel.RespCombo.SetCaseList(name, ref type, "Dead", deadFactor);
                    }
                    double liveFactor = Convert.ToDouble(dtForLoadCombination.Rows[i]["Live"]);
                    if (liveFactor != 0)
                    {
                        ret = sapModel.RespCombo.SetCaseList(name, ref type, "Live", liveFactor);
                    }
                    double wx0_0Factor = Convert.ToDouble(dtForLoadCombination.Rows[i]["WX0_0"]);
                    if (wx0_0Factor != 0)
                    {
                        ret = sapModel.RespCombo.SetCaseList(name, ref type, "WX+(+i)", wx0_0Factor);
                    }
                    double wx0_1Factor = Convert.ToDouble(dtForLoadCombination.Rows[i]["WX0_1"]);
                    if (wx0_1Factor != 0)
                    {
                        ret = sapModel.RespCombo.SetCaseList(name, ref type, "WX+(-i)", wx0_1Factor);
                    }
                    double wx1_0Factor = Convert.ToDouble(dtForLoadCombination.Rows[i]["WX1_0"]);
                    if (wx1_0Factor != 0)
                    {
                        ret = sapModel.RespCombo.SetCaseList(name, ref type, "WX-(+i)", wx1_0Factor);
                    }
                    double wx1_1Factor = Convert.ToDouble(dtForLoadCombination.Rows[i]["WX1_1"]);
                    if (wx1_1Factor != 0)
                    {
                        ret = sapModel.RespCombo.SetCaseList(name, ref type, "WX-(-i)", wx1_1Factor);
                    }
                    double wy0_0Factor = Convert.ToDouble(dtForLoadCombination.Rows[i]["WY0_0"]);
                    if (wy0_0Factor != 0)
                    {
                        ret = sapModel.RespCombo.SetCaseList(name, ref type, "WY+(+i)", wy0_0Factor);
                    }
                    double wy0_1Factor = Convert.ToDouble(dtForLoadCombination.Rows[i]["WY0_1"]);
                    if (wy0_1Factor != 0)
                    {
                        ret = sapModel.RespCombo.SetCaseList(name, ref type, "WY+(-i)", wy0_1Factor);
                    }
                    double wy1_0Factor = Convert.ToDouble(dtForLoadCombination.Rows[i]["WY1_0"]);
                    if (wy1_0Factor != 0)
                    {
                        ret = sapModel.RespCombo.SetCaseList(name, ref type, "WY-(+i)", wy1_0Factor);
                    }
                    double wy1_1Factor = Convert.ToDouble(dtForLoadCombination.Rows[i]["WY1_1"]);
                    if (wy1_1Factor != 0)
                    {
                        ret = sapModel.RespCombo.SetCaseList(name, ref type, "WY-(-i)", wy1_1Factor);
                    }
                    double exFactor = Convert.ToDouble(dtForLoadCombination.Rows[i]["EX"]);
                    if (exFactor != 0)
                    {
                        ret = sapModel.RespCombo.SetCaseList(name, ref type, "EX", exFactor);
                    }
                    double eyFactor = Convert.ToDouble(dtForLoadCombination.Rows[i]["EY"]);

                    if (eyFactor != 0)
                    {
                        ret = sapModel.RespCombo.SetCaseList(name, ref type, "EY", eyFactor);
                    }
                }
                #endregion

                #region 保存SAP2000计算文件
                this.Dispatcher.Invoke(new Action(() =>
                {
                    string ModelDirectory = @FoldPath.Text;
                    Directory.CreateDirectory(ModelDirectory);
                    string modelName = string.Concat(FoldName.Text, ".sdb");
                    ret = sapModel.File.Save(ModelDirectory + "/" + modelName);
                }));
                
                #endregion

                #region 运行分析
                ret = sapModel.Analyze.RunAnalysis();
                #endregion

                #region 提取各构件内力结果至数据库
                string sqlForRelationship = "select * from Revit_SAP2000_Element_Relationship";
                DBHelper.PrepareSql(sqlForRelationship);
                DataTable dtForRelationship = DBHelper.ExecQuery();

                string sqlForTag = "select Tag from LoadCombinationInfo";
                DBHelper.PrepareSql(sqlForTag);
                DataTable dtForTag = DBHelper.ExecQuery();

                for (int i = 0; i < dtForRelationship.Rows.Count; i++)
                {
                    string SAP2000ElementId = dtForRelationship.Rows[i]["SAP2000_Element_Id"].ToString();
                    string RevitElementId = dtForRelationship.Rows[i]["Revit_Element_Id"].ToString();
                    int NumberResultForCombo = 0;
                    string[] ObjForCombo = null;
                    double[] ObjStaForCombo = null;
                    string[] ElmForCombo = null;
                    double[] ElmStaForCombo = null;
                    string[] LoadCaseForCombo = null;
                    string[] StepTypeForCombo = null;
                    double[] StepNumForCombo = null;
                    double[] PForCombo = null;
                    double[] V2ForCombo = null;
                    double[] V3ForCombo = null;
                    double[] TForCombo = null;
                    double[] M2ForCombo = null;
                    double[] M3ForCombo = null;
                    for (int j = 0; j < dtForTag.Rows.Count; j++)
                    {

                        ret = sapModel.Results.Setup.SetComboSelectedForOutput(dtForTag.Rows[j]["Tag"].ToString());
                        ret = sapModel.Results.FrameForce(SAP2000ElementId.ToString(), eItemTypeElm.ObjectElm, ref NumberResultForCombo, ref ObjForCombo, ref ObjStaForCombo,
                                ref ElmForCombo, ref ElmStaForCombo, ref LoadCaseForCombo, ref StepTypeForCombo, ref StepNumForCombo, ref PForCombo, ref V2ForCombo, ref V3ForCombo, ref TForCombo, ref M2ForCombo, ref M3ForCombo);
                    }
                    for (int k = 0; k < NumberResultForCombo; k++)
                    {
                        string sqlForColumnForce = "Insert into ElementForces(Reivt_Element_Id,Station,OutputCase,CaseType,P,V2,V3,T,M2,M3)" +
                                                    "values(@Reivt_Element_Id,@Station,@OutputCase,@CaseType,@P,@V2,@V3,@T,@M2,@M3)";
                        DBHelper.PrepareSql(sqlForColumnForce);
                        DBHelper.SetParameter("Reivt_Element_Id", RevitElementId);
                        DBHelper.SetParameter("Station", ObjStaForCombo[k]);
                        DBHelper.SetParameter("OutputCase", LoadCaseForCombo[k]);
                        DBHelper.SetParameter("CaseType", LoadCaseForCombo[k]);
                        DBHelper.SetParameter("P", PForCombo[k]);
                        DBHelper.SetParameter("V2", V2ForCombo[k]);
                        DBHelper.SetParameter("V3", V2ForCombo[k]);
                        DBHelper.SetParameter("T", TForCombo[k]);
                        DBHelper.SetParameter("M2", M2ForCombo[k]);
                        DBHelper.SetParameter("M3", M3ForCombo[k]);
                        DBHelper.ExecNonQuery();
                    }
                }

                List<string> basicCase = new List<string>() { "Dead", "Live", "WX+(+i)", "WX+(-i)", "WX-(+i)", "WX-(-i)", "WY+(+i)", "WY+(-i)", "WY-(+i)", "WY-(-i)", "EX", "EY" };
                for (int i = 0; i < dtForRelationship.Rows.Count; i++)
                {
                    string SAP2000ElementId = dtForRelationship.Rows[i]["SAP2000_Element_Id"].ToString();
                    string RevitElementId = dtForRelationship.Rows[i]["Revit_Element_Id"].ToString();
                    int NumberResultForBasic = 0;
                    string[] ObjForBasic = null;
                    double[] ObjStaForBasic = null;
                    string[] ElmForBasic = null;
                    double[] ElmStaForBasic = null;
                    string[] LoadCaseForBasic = null;
                    string[] StepTypeForBasic = null;
                    double[] StepNumForBasic = null;
                    double[] PForBasic = null;
                    double[] V2ForBasic = null;
                    double[] V3ForBasic = null;
                    double[] TForBasic = null;
                    double[] M2ForBasic = null;
                    double[] M3ForBasic = null;
                    for (int j = 0; j < basicCase.Count; j++)
                    {

                        ret = sapModel.Results.Setup.SetCaseSelectedForOutput(basicCase[j]);
                        ret = sapModel.Results.FrameForce(SAP2000ElementId.ToString(), eItemTypeElm.ObjectElm, ref NumberResultForBasic, ref ObjForBasic, ref ObjStaForBasic,
                                ref ElmForBasic, ref ElmStaForBasic, ref LoadCaseForBasic, ref StepTypeForBasic, ref StepNumForBasic, ref PForBasic, ref V2ForBasic, ref V3ForBasic, ref TForBasic, ref M2ForBasic, ref M3ForBasic);
                    }
                    for (int k = 0; k < NumberResultForBasic; k++)
                    {
                        string sqlForColumnForce = "Insert into ElementForces(Reivt_Element_Id,Station,OutputCase,CaseType,P,V2,V3,T,M2,M3)" +
                                                    "values(@Reivt_Element_Id,@Station,@OutputCase,@CaseType,@P,@V2,@V3,@T,@M2,@M3)";
                        DBHelper.PrepareSql(sqlForColumnForce);
                        DBHelper.SetParameter("Reivt_Element_Id", RevitElementId);
                        DBHelper.SetParameter("Station", ObjStaForBasic[k]);
                        DBHelper.SetParameter("OutputCase", LoadCaseForBasic[k]);
                        DBHelper.SetParameter("CaseType", LoadCaseForBasic[k]);
                        DBHelper.SetParameter("P", PForBasic[k]);
                        DBHelper.SetParameter("V2", V2ForBasic[k]);
                        DBHelper.SetParameter("V3", V2ForBasic[k]);
                        DBHelper.SetParameter("T", TForBasic[k]);
                        DBHelper.SetParameter("M2", M2ForBasic[k]);
                        DBHelper.SetParameter("M3", M3ForBasic[k]);
                        DBHelper.ExecNonQuery();
                    }
                }


                int NumberResultsForPeriod = 0;
                string[] LoadCaseForPeriod = null;
                string[] StepTypeForPeriod = null;
                double[] StepNumForPeriod = null;
                double[] Period = null;
                double[] Frequency = null;
                double[] CircFreq = null;
                double[] EigenValue = null;
                ret = sapModel.Results.ModalPeriod(ref NumberResultsForPeriod, ref LoadCaseForPeriod, ref StepTypeForPeriod, ref StepNumForPeriod, ref Period, ref Frequency, ref CircFreq, ref EigenValue);
                for (int i = 0; i < NumberResultsForPeriod; i++)
                {
                    string sqlForPeriod = "Insert into ModalPeriod(LoadCase,StepType,StepNum,Period,Frequency,CircFreq,EigenValue)" +
                                          "values(@LoadCase,@StepType,@StepNum,@Period,@Frequency,@CircFreq,@EigenValue)";
                    DBHelper.PrepareSql(sqlForPeriod);
                    DBHelper.SetParameter("LoadCase", LoadCaseForPeriod[i]);
                    DBHelper.SetParameter("StepType", StepTypeForPeriod[i]);
                    DBHelper.SetParameter("StepNum", StepNumForPeriod[i]);
                    DBHelper.SetParameter("Period", Period[i]);
                    DBHelper.SetParameter("Frequency", Frequency[i]);
                    DBHelper.SetParameter("CircFreq", CircFreq[i]);
                    DBHelper.SetParameter("EigenValue", EigenValue[i]);
                    DBHelper.ExecNonQuery();
                }




                #endregion

                sapObject.ApplicationExit(true);

                this.Dispatcher.Invoke(new Action(()=>
                {
                    Messenger.Default.Send<bool>(false, Token.LoadSAP2000Anlysis);
                }));
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dialog=new FolderBrowserDialog();
            dialog.Description = "选择文件路径";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string Path=dialog.SelectedPath;
                FoldPath.Text = Path;
            }
        }
        private void Desc_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            #region 前端数据展示

            if (Desc.SelectedItem.ToString() == "基本组合内力信息")
            {
                string sqlForDetail = "select Tag from LoadCombinationInfo";
                DBHelper.PrepareSql(sqlForDetail);
                DataTable dtForDetail = DBHelper.ExecQuery();
                List<object> loadCombinations = new List<object>();
                for (int i = 0; i < dtForDetail.Rows.Count; i++)
                {
                    loadCombinations.Add(dtForDetail.Rows[i]["Tag"]);
                }
                Detail.DataContext = loadCombinations;
            }

            if (Desc.SelectedItem.ToString() == "单工况内力信息")
            {
                List<object> basicCase =new List<object>() { "Dead", "Live", "WX+(+i)", "WX+(-i)", "WX-(+i)", "WX-(-i)", "WY+(+i)", "WY+(-i)", "WY-(+i)", "WY-(-i)", "EX", "EY" };
                Detail.DataContext =basicCase;
            }

            if (Desc.SelectedItem.ToString() == "周期信息")
            {
                string sql = "select LoadCase,StepNum,round(Period,4) as Period,round(Frequency,4) as Frequency,round(CircFreq,4) as CircFreq,round(EigenValue,4) as EigenValue from ModalPeriod";
                DBHelper.PrepareSql(sql);
                DataTable dtForPeriod= DBHelper.ExecQuery();
                Data.DataContext= dtForPeriod;

                Detail.DataContext = null;
            }


            #endregion
        }
        private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string loadCase = Detail.SelectedItem.ToString();
            string sql = "select Reivt_Element_Id as Id,round(Station,3) 'Station(m)',round(P,3) as 'P(kN)',round(V2,3) as 'V2(kN)',round(V3,3) as 'V3(kN)',round(T,3) as 'T(kN·M)'," +
                            "round(M2,3) as 'M2(kN·M)',round(M3,3) as 'M3(kN·M)' from ElementForces where OutputCase=@loadCase";
            DBHelper.PrepareSql(sql);
            DBHelper.SetParameter("loadCase", loadCase);
            DataTable dtForForce = DBHelper.ExecQuery();
            Data.DataContext = dtForForce;
        }
        private void Data_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var dg = sender as System.Windows.Controls.DataGrid;
            int id = Convert.ToInt32((dg.SelectedItem as DataRowView).Row[0]);
            uidoc.Selection.SetElementIds(new List<ElementId>() { new ElementId(id) });
        }
    }
}
