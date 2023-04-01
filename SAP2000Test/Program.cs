using SAP2000v1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SAP2000Test
{
    class Program
    {
        static void Main(string[] args)
        {
            cHelper myHelper = new Helper();
            cOAPI mySapObject = myHelper.CreateObjectProgID("CSI.SAP2000.API.SapObject");
            //cOAPI mySapObject = myHelper.CreateObject(@"C:\Program Files\Computers and Structures\SAP2000 23\SAP2000.exe");
            //sap2000 oapi中方法的返回值均为int，若方法执行成功返回值为0，不成功则为非0数。故定义ret，保存方法是否执行成功。
            int ret = 0;
            //打开sap2000
            ret = mySapObject.ApplicationStart();
            //通过object拿到model
            cSapModel mySapModel = mySapObject.SapModel;
            //在SAP2000中初始化model，这里改了单位
            ret = mySapModel.InitializeNewModel(eUnits.kN_m_C);
            //在SAP2000中的文件下拉列表中选择新模型，这里选择三维框架
            ret = mySapModel.File.NewBlank();
            //添加材料属性,这里的引用参数的值按我的理解是在程序中的材料属性的实例名称
            string Q355 = "Q355";
            ret = mySapModel.PropMaterial.AddMaterial(ref Q355, eMatType.Steel, "China", "GB", "Q355");
            string Q235 = "Q235";
            ret = mySapModel.PropMaterial.AddMaterial(ref Q235, eMatType.Steel, "China", "GB", "Q235");
            //导入框架截面
            //1.梁柱截面皆为HW350X350X12X19，抗风柱为HW200X200X8X12
            ret = mySapModel.PropFrame.ImportProp("HW350X350X12X19", "Q355", "ChineseGB08.xml", "HW350X350X12X19");
            ret = mySapModel.PropFrame.ImportProp("HW200X200X8X12", "Q355", "ChineseGB08.xml", "HW200X200X8X12");
            //创建框架截面
            //2.系杆支撑皆为圆管89*3.5
            ret = mySapModel.PropFrame.SetPipe("89X3.5", "Q355", 0.089, 0.0035);
            double[] ModValue = new double[8];
            for (int j = 0; j <= 7; j++)
            {
                ModValue[j] = 1;
            }
            ret = mySapModel.PropFrame.SetModifiers("HW350X350X12X19", ref ModValue);
            ret = mySapModel.PropFrame.SetModifiers("89X3.5", ref ModValue);

            //画框架
            //主刚架
            string columnName1 = "";
            ret = mySapModel.FrameObj.AddByCoord(0, 0, 0, 0, 0, 6, ref columnName1, "HW350X350X12X19","1","Global");
            ret = mySapModel.FrameObj.SetLocalAxes(columnName1, 90);

            string columnName2 = "";
            ret = mySapModel.FrameObj.AddByCoord(0, 9, 6+0.9, 0, 18, 6, ref columnName2, "HW350X350X12X19","2", "Global");

            string columnName3 = "";
            ret = mySapModel.FrameObj.AddByCoord(0, 0, 6, 0, 9, 6+0.9, ref columnName3, "HW350X350X12X19", "3", "Global");

            string columnName4 = "";
            ret = mySapModel.FrameObj.AddByCoord(0, 18,0, 0, 18, 6, ref columnName4, "HW350X350X12X19", "4", "Global");
            ret = mySapModel.FrameObj.SetLocalAxes(columnName4, 90);

            //带属性复制
            ret = mySapModel.SelectObj.All();
            string[] Frams = new string[20];
            int[] FramsType = new int[20];
            int FramsNumber = 20;
            ret = mySapModel.EditGeneral.ReplicateLinear(6, 0, 0, 5, ref FramsNumber, ref Frams, ref FramsType);

            //抗风柱
            string columnName5 = "";
            ret = mySapModel.FrameObj.AddByCoord(0, 6, 0, 0, 6,6+ 0.9/9*6, ref columnName5, "HW200X200X8X12", "5", "Global");
            ret = mySapModel.FrameObj.SetLocalAxes(columnName5, 90);

            string columnName6 = "";
            ret = mySapModel.FrameObj.AddByCoord(0, 12, 0, 0, 12, 6+0.9/9*6, ref columnName6, "HW200X200X8X12", "6", "Global");
            ret = mySapModel.FrameObj.SetLocalAxes(columnName6, 90);

            string columnName7 = "";
            ret = mySapModel.FrameObj.AddByCoord(30, 6, 0, 30, 6, 6 + 0.9 / 9 * 6, ref columnName7, "HW200X200X8X12", "7", "Global");
            ret = mySapModel.FrameObj.SetLocalAxes(columnName7, 90);

            string columnName8 = "";
            ret = mySapModel.FrameObj.AddByCoord(30, 12, 0, 30, 12, 6 + 0.9 / 9 * 6, ref columnName8, "HW200X200X8X12", "8", "Global");
            ret = mySapModel.FrameObj.SetLocalAxes(columnName8, 90);

            //柱间支撑
            string columnBracing1_1 = "";
            ret = mySapModel.FrameObj.AddByCoord(0, 0, 0, 6, 0, 6, ref columnBracing1_1, "89X3.5");
            string columnBracing1_2 = "";
            ret = mySapModel.FrameObj.AddByCoord(0, 0, 6, 6, 0, 0, ref columnBracing1_2, "89X3.5");
            string columnBracing2_1 = "";
            ret = mySapModel.FrameObj.AddByCoord(0, 18, 0, 6, 18, 6, ref columnBracing2_1, "89X3.5");
            string columnBracing2_2 = "";
            ret = mySapModel.FrameObj.AddByCoord(0, 18, 6, 6, 18, 0, ref columnBracing2_2, "89X3.5");
            string columnBracing3_1 = "";
            ret = mySapModel.FrameObj.AddByCoord(24, 0, 0, 30, 0, 6, ref columnBracing3_1, "89X3.5");
            string columnBracing3_2 = "";
            ret = mySapModel.FrameObj.AddByCoord(30, 0, 0, 24, 0, 6, ref columnBracing3_2, "89X3.5");
            string columnBracing4_1 = "";
            ret = mySapModel.FrameObj.AddByCoord(24, 18, 0, 30, 18, 6, ref columnBracing4_1, "89X3.5");
            string columnBracing4_2 = "";
            ret = mySapModel.FrameObj.AddByCoord(24, 18, 6, 30, 18, 0, ref columnBracing4_2, "89X3.5");
            //系杆
            for (int i = 0; i < 5; i++)
            {
                string xigan1 = "";
                ret = mySapModel.FrameObj.AddByCoord(i * 6, 0, 6, (i + 1) * 6, 0, 6, ref xigan1, "89X3.5");
                string xigan2 = "";
                ret = mySapModel.FrameObj.AddByCoord(i * 6, 9, 6 + 0.9, (i + 1) * 6, 9, 6 + 0.9, ref xigan2, "89X3.5");
                string xigan3 = "";
                ret = mySapModel.FrameObj.AddByCoord(i * 6, 18, 6, (i + 1) * 6, 18, 6, ref xigan3, "89X3.5");
            }
            //屋面支撑
            #region  系杆和支撑
            string roofxigan1 = "";
            ret = mySapModel.FrameObj.AddByCoord(0, 9 / 3, 6 + 0.9 / 3, 6, 9 / 3, 6 + 0.9 / 3, ref roofxigan1, "89X3.5");
            ret = mySapModel.FrameObj.AddByCoord(24, 9 / 3, 6 + 0.9 / 3, 30, 9 / 3, 6 + 0.9 / 3, ref roofxigan1, "89X3.5");

            string roofxigan2 = "";
            ret = mySapModel.FrameObj.AddByCoord(0, 9 / 3 * 2, 6 + 0.9 / 3 * 2, 6, 9 / 3 * 2, 6 + 0.9 / 3 * 2, ref roofxigan2, "89X3.5");
            ret = mySapModel.FrameObj.AddByCoord(24, 9 / 3 * 2, 6 + 0.9 / 3 * 2, 30, 9 / 3 * 2, 6 + 0.9 / 3 * 2, ref roofxigan2, "89X3.5");

            string roofxigan3 = "";
            ret = mySapModel.FrameObj.AddByCoord(0, 9 + 9 / 3, 6 + 0.9 / 3 * 2, 6, 9 + 9 / 3, 6 + 0.9 / 3 * 2, ref roofxigan3, "89X3.5");
            ret = mySapModel.FrameObj.AddByCoord(24, 9 + 9 / 3, 6 + 0.9 / 3 * 2, 30, 9 + 9 / 3, 6 + 0.9 / 3 * 2, ref roofxigan3, "89X3.5");

            string roofxigan4 = "";
            ret = mySapModel.FrameObj.AddByCoord(0, 9 + 9 / 3 * 2, 6 + 0.9 / 3, 6, 9 + 9 / 3 * 2, 6 + 0.9 / 3, ref roofxigan4, "89X3.5");
            ret = mySapModel.FrameObj.AddByCoord(24, 9 + 9 / 3 * 2, 6 + 0.9 / 3, 30, 9 + 9 / 3 * 2, 6 + 0.9 / 3, ref roofxigan4, "89X3.5");

            string roofbracing1_1 = "";
            ret = mySapModel.FrameObj.AddByCoord(0, 0, 6, 6, 9 / 3, 6 + 0.9 / 3, ref roofbracing1_1, "89X3.5");
            ret = mySapModel.FrameObj.AddByCoord(24, 0, 6, 30, 9 / 3, 6 + 0.9 / 3, ref roofbracing1_1, "89X3.5");

            string roofbracing1_2 = "";
            ret = mySapModel.FrameObj.AddByCoord(0, 9 / 3, 6 + 0.9 / 3, 6, 0, 6, ref roofbracing1_2, "89X3.5");
            ret = mySapModel.FrameObj.AddByCoord(24, 9 / 3, 6 + 0.9 / 3, 30, 0, 6, ref roofbracing1_2, "89X3.5");

            string roofbracing2_1 = "";
            ret = mySapModel.FrameObj.AddByCoord(0, 9 / 3, 6 + 0.9 / 3, 6, 9 / 3 * 2, 6 + 0.9 / 3 * 2, ref roofbracing2_1, "89X3.5");
            ret = mySapModel.FrameObj.AddByCoord(24, 9 / 3, 6 + 0.9 / 3, 30, 9 / 3 * 2, 6 + 0.9 / 3 * 2, ref roofbracing2_1, "89X3.5");

            string roofbracing2_2 = "";
            ret = mySapModel.FrameObj.AddByCoord(0, 9 / 3 * 2, 6 + 0.9 / 3 * 2, 6, 9 / 3, 6 + 0.9 / 3, ref roofbracing2_2, "89X3.5");
            ret = mySapModel.FrameObj.AddByCoord(24, 9 / 3 * 2, 6 + 0.9 / 3 * 2, 30, 9 / 3, 6 + 0.9 / 3, ref roofbracing2_2, "89X3.5");

            string roofbracing3_1 = "";
            ret = mySapModel.FrameObj.AddByCoord(0, 9 / 3 * 2, 6 + 0.9 / 3 * 2, 6, 9, 6 + 0.9, ref roofbracing3_1, "89X3.5");
            ret = mySapModel.FrameObj.AddByCoord(24, 9 / 3 * 2, 6 + 0.9 / 3 * 2, 30, 9, 6 + 0.9, ref roofbracing3_1, "89X3.5");

            string roofbracing3_2 = "";
            ret = mySapModel.FrameObj.AddByCoord(0, 9, 6 + 0.9, 6, 9 / 3 * 2, 6 + 0.9 / 3 * 2, ref roofbracing3_2, "89X3.5");
            ret = mySapModel.FrameObj.AddByCoord(24, 9, 6 + 0.9, 30, 9 / 3 * 2, 6 + 0.9 / 3 * 2, ref roofbracing3_2, "89X3.5");

            string roofbracing4_1 = "";
            ret = mySapModel.FrameObj.AddByCoord(0, 9, 6 + 0.9, 6, 9 + 9 / 3, 6 + 0.9 / 3 * 2, ref roofbracing4_1, "89X3.5");
            ret = mySapModel.FrameObj.AddByCoord(24, 9, 6 + 0.9, 30, 9 + 9 / 3, 6 + 0.9 / 3 * 2, ref roofbracing4_1, "89X3.5");

            string roofbracing4_2 = "";
            ret = mySapModel.FrameObj.AddByCoord(0, 9 + 9 / 3, 6 + 0.9 / 3 * 2, 6, 9, 6 + 0.9, ref roofbracing4_2, "89X3.5");
            ret = mySapModel.FrameObj.AddByCoord(24, 9 + 9 / 3, 6 + 0.9 / 3 * 2, 30, 9, 6 + 0.9, ref roofbracing4_2, "89X3.5");

            string roofbracing5_1 = "";
            ret = mySapModel.FrameObj.AddByCoord(0, 9 + 9 / 3 * 2, 6 + 0.9 / 3, 6, 9 + 9 / 3, 6 + 0.9 / 3 * 2, ref roofbracing5_1, "89X3.5");
            ret = mySapModel.FrameObj.AddByCoord(24, 9 + 9 / 3 * 2, 6 + 0.9 / 3, 30, 9 + 9 / 3, 6 + 0.9 / 3 * 2, ref roofbracing5_1, "89X3.5");

            string roofbracing5_2 = "";
            ret = mySapModel.FrameObj.AddByCoord(0, 9 + 9 / 3, 6 + 0.9 / 3 * 2, 6, 9 + 9 / 3 * 2, 6 + 0.9 / 3, ref roofbracing5_2, "89X3.5");
            ret = mySapModel.FrameObj.AddByCoord(24, 9 + 9 / 3, 6 + 0.9 / 3 * 2, 30, 9 + 9 / 3 * 2, 6 + 0.9 / 3, ref roofbracing5_2, "89X3.5");

            string roofbracing6_1 = "";
            ret = mySapModel.FrameObj.AddByCoord(0, 9 + 9 / 3 * 2, 6 + 0.9 / 3, 6, 9 + 9, 6, ref roofbracing6_1, "89X3.5");
            ret = mySapModel.FrameObj.AddByCoord(24, 9 + 9 / 3 * 2, 6 + 0.9 / 3, 30, 9 + 9, 6, ref roofbracing6_1, "89X3.5");

            string roofbracing6_2 = "";
            ret = mySapModel.FrameObj.AddByCoord(0, 9 + 9, 6, 6, 9 + 9 / 3 * 2, 6 + 0.9 / 3, ref roofbracing6_2, "89X3.5");
            ret = mySapModel.FrameObj.AddByCoord(24, 9 + 9, 6, 30, 9 + 9 / 3 * 2, 6 + 0.9 / 3, ref roofbracing6_2, "89X3.5");







            #endregion

            //柱与地铰接
            ret = mySapModel.SelectObj.PlaneXY("1");
            int numberItems = 0;
            int[] selectedType = null;
            string[] selectedName = null;
            ret = mySapModel.SelectObj.GetSelected(ref numberItems, ref selectedType, ref selectedName);
            ret = mySapModel.SelectObj.ClearSelection();
            bool[] values = new bool[6] { true, true, true, false, false, false };
            for (int i = 0; i < selectedName.Length; i++)
            {
                mySapModel.PointObj.SetRestraint(selectedName[i], ref values);
            }
            //端部释放（系杆和支撑）
            ret = mySapModel.SelectObj.PropertyFrame("89X3.5");
            ret = mySapModel.SelectObj.GetSelected(ref numberItems, ref selectedType, ref selectedName);
            bool[] ii = new bool[6] { false, false, false, false, true, true };
            bool[] jj = new bool[6] { false, false, false, true, true, true };
            double[] start = new double[6] { 1, 1, 1, 1, 0, 0 };
            double[] end = new double[6] { 1, 1, 1, 0, 0, 0 };
            for (int i = 0; i < selectedName.Length; i++)
            {
                ret = mySapModel.FrameObj.SetReleases(selectedName[i], ref ii, ref jj, ref start, ref end);
            }
            ret = mySapModel.SelectObj.ClearSelection();
            //端部释放 （抗风柱）
            ret = mySapModel.SelectObj.PropertyFrame("HW200X200X8X12");
            ret = mySapModel.SelectObj.GetSelected(ref numberItems, ref selectedType, ref selectedName);
            bool[] ii1 = new bool[6] { false, false, false, false, true, true };
            bool[] jj1 = new bool[6] { true, false, false, true, true, true };
            double[] start1 = new double[6] { 1, 1, 1, 1, 0, 0 };
            double[] end1 = new double[6] { 0, 1, 1, 0, 0, 0 };
            for (int i = 0; i < selectedName.Length; i++)
            {
                ret = mySapModel.FrameObj.SetReleases(selectedName[i], ref ii1, ref jj1, ref start1, ref end1);
            }
            ret = mySapModel.SelectObj.ClearSelection();
            //测站
            ret = mySapModel.SelectObj.All();
            ret = mySapModel.SelectObj.GetSelected(ref numberItems, ref selectedType, ref selectedName);
            for (int i = 0; i < selectedName.Length; i++)
            {
                ret = mySapModel.FrameObj.SetOutputStations(selectedName[i], 2,0.5 ,9);
            }
            ret = mySapModel.SelectObj.ClearSelection();


            //添加荷载模式
            ret = mySapModel.LoadPatterns.Add("LIVE", eLoadPatternType.Live);
            ret = mySapModel.LoadPatterns.Add("WX+", eLoadPatternType.Wind);
            ret = mySapModel.LoadPatterns.Add("WX-", eLoadPatternType.Wind);
            ret = mySapModel.LoadPatterns.Add("WY+", eLoadPatternType.Wind);
            ret = mySapModel.LoadPatterns.Add("WY-", eLoadPatternType.Wind);

            //在主刚架上施加屋面恒荷载（分布荷载）（边跨中间跨计算宽度不同）
            ret = mySapModel.FrameObj.SetLoadDistributed("2", "DEAD", 1, 10, 0, 1, 0.9, 0.9);
            ret = mySapModel.FrameObj.SetLoadDistributed("3", "DEAD", 1, 10, 0, 1, 0.9, 0.9);
            ret = mySapModel.FrameObj.SetLoadDistributed("22", "DEAD", 1, 10, 0, 1, 0.9, 0.9);
            ret = mySapModel.FrameObj.SetLoadDistributed("23", "DEAD", 1, 10, 0, 1, 0.9, 0.9);

            for (int i = 6; i < 20; i += 4)
            {
                ret = mySapModel.FrameObj.SetLoadDistributed(i.ToString(), "DEAD", 1, 10, 0, 1, 1.8, 1.8);
                ret = mySapModel.FrameObj.SetLoadDistributed((i + 1).ToString(), "DEAD", 1, 10, 0, 1, 1.8, 1.8);
            }
            //施加屋面活载
            ret = mySapModel.FrameObj.SetLoadDistributed("2", "LIVE", 1, 10, 0, 1, 1.5, 1.5);
            ret = mySapModel.FrameObj.SetLoadDistributed("3", "LIVE", 1, 10, 0, 1, 1.5, 1.5);
            ret = mySapModel.FrameObj.SetLoadDistributed("22", "LIVE", 1, 10, 0, 1, 1.5, 1.5);
            ret = mySapModel.FrameObj.SetLoadDistributed("23", "LIVE", 1, 10, 0, 1, 1.5, 1.5);

            for (int i = 6; i < 20; i += 4)
            {
                ret = mySapModel.FrameObj.SetLoadDistributed(i.ToString(), "LIVE", 1, 10, 0, 1, 3, 3);
                ret = mySapModel.FrameObj.SetLoadDistributed((i + 1).ToString(), "LIVE", 1, 10, 0, 1, 3, 3);
            }
            //定义结构质量
            string[] loadPatternForMass = new string[2] { "DEAD", "LIVE" };
            Double[] sf = new double[2] { 1, 0.5 };
            ret = mySapModel.SourceMass.SetMassSource("mass", false, false, true, false, 2, ref loadPatternForMass, ref sf);
            ret = mySapModel.SourceMass.SetDefault("mass");
            //定义反应谱函数,这里我填的是7（0.1）度，特征周期0.45，周期折减系数1，阻尼比0.05
            ret = mySapModel.Func.FuncRS.SetChinese2010("FUNC", 0.08, 2, 0.45, 1, 0.05);
            //定义地震工况
            string[] loadNameForEX = new string[1] { "U1" };
            string[] loadNameForEY = new string[1] { "U2" };
            string[] Fun = new string[1] { "FUNC" };
            double[] sfforE = new double[1] { 9.8 };
            string[] cSys = new string[] { "" };
            double[] angle = new double[] { 0 };
            ret = mySapModel.LoadCases.ResponseSpectrum.SetCase("EX");
            ret = mySapModel.LoadCases.ResponseSpectrum.SetLoads("EX", 1, ref loadNameForEX, ref Fun, ref sfforE, ref cSys, ref angle);
            ret = mySapModel.LoadCases.ResponseSpectrum.SetCase("EY");
            ret = mySapModel.LoadCases.ResponseSpectrum.SetLoads("EY", 1, ref loadNameForEY, ref Fun, ref sfforE, ref cSys, ref angle);
            //定义荷载组合
            //1.5恒+1.3活
            ret = mySapModel.RespCombo.Add("1.3DEAD+1.5LIVE", 0);
            eCNameType type = eCNameType.LoadCase;
            ret = mySapModel.RespCombo.SetCaseList("1.3DEAD+1.5LIVE", ref type, "DEAD", 1.3);
            ret = mySapModel.RespCombo.SetCaseList("1.3DEAD+1.5LIVE", ref type, "LIVE", 1.5);

            mySapModel.View.RefreshView();
            mySapModel.View.RefreshWindow();
            MessageBox.Show(ret.ToString());
            //运行分析前，需要将SAP2000模型文件存入指定的文件夹
            string ModelDirectory = @"C:\factory";
            Directory.CreateDirectory(ModelDirectory);
            string modelName = "factory.sdb";
            ret = mySapModel.File.Save(ModelDirectory + "//" + modelName);

            //运行分析
            ret = mySapModel.Analyze.RunAnalysis();

            //退出SAP2000程序，说明文档规定需将Object和model设为null
            //mySapObject.ApplicationExit(false);
            //mySapModel = null;
            //mySapObject = null;
        }
    }
}
