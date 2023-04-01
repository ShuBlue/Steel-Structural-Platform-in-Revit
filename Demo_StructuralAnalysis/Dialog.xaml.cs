using SAP2000v1;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;

namespace Demo_StructuralAnalysis
{
    /// <summary>
    /// Dialog.xaml 的交互逻辑
    /// </summary>
    public partial class Dialog : Window
    {
        public Dialog()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //Helper中有创建Sap2000的方法
            cHelper myHelper=new Helper();
            cOAPI mySapObject= myHelper.CreateObjectProgID("CSI.SAP2000.API.SapObject");
            //cOAPI mySapObject = myHelper.CreateObject(@"C:\Program Files\Computers and Structures\SAP2000 23\SAP2000.exe");
            //sap2000 oapi中方法的返回值均为int，若方法执行成功返回值为0，不成功则为非0数。故定义ret，保存方法是否执行成功。
            int ret=0;
            //打开sap2000
            ret=mySapObject.ApplicationStart();
            //通过object拿到model
            cSapModel mySapModel = mySapObject.SapModel;
            //在SAP2000中初始化model，这里改了单位
            ret = mySapModel.InitializeNewModel(eUnits.kN_m_C);
            //在SAP2000中的文件下拉列表中选择新模型，这里选择三维框架
            ret = mySapModel.File.New3DFrame(e3DFrameType.OpenFrame, 2, 8, 3, 6, 2, 6);
            //添加材料属性
            string Name="123456";
            ret = mySapModel.PropMaterial.AddMaterial(ref Name,eMatType.Steel,"China","GB","Q355","Lhj");
            //ret = mySapModel.PropMaterial.AddMaterial(ref Name, eMatType.Steel, "China", "GB", "Q235","Lhj");

            MessageBox.Show(ret.ToString());
            //将SAP2000模型文件存入指定的文件夹
            //string ModelDirectory = @"C:\factory";
            //Directory.CreateDirectory(ModelDirectory);
            //string modelName = "factory.sdb";
            //ret = mySapModel.File.Save(ModelDirectory+"//"+modelName);
            //退出SAP2000程序，说明文档规定需将Object和model设为null
            //mySapObject.ApplicationExit(false);
            //mySapModel = null;
            //mySapObject = null;
        }
    }
}
