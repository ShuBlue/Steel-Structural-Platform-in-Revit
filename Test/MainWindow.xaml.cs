using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using GalaSoft.MvvmLight.Messaging;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
using Test.Common;
using Test.UserControls;

namespace Test
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow
    {
        //private static MainWindow _instance;
        
        public MainWindow(Document doc,UIDocument uidoc)
        {
            InitializeComponent();
            this.doc = doc;
            this.uidoc=uidoc;
            Loaded += (s, e) =>
            {
                gridAndLevelView = GridAndLevelView.GetInstance();
                gridAndLevelView.Initial();
                framingView = FramingView.GetInstance();
                framingView.Initial(doc);
                bracingView = BracingView.GetInstance();
                bracingView.Initial(doc);
                purlinView = PurlinView.GetInstance();
                purlinView.Initial(doc);
                //RVTCreation = RVTCreation.GetInstance();
                //RVTCreation.Initial(doc);
                loadSetView= LoadSetView.GetInstance();
                loadSetView.Initial(doc);
                loadCombinationView = LoadCombinationView.GetInstance();
                loadCombinationView.Initial(doc);
                Revit2SQLView = Revit2SQLView.GetInstance();
                Revit2SQLView.Initial(doc);
                SAP2000AnalysisView = SAP2000AnalysisView.GetInstance();
                SAP2000AnalysisView.Initial(uidoc);
            };
            Messenger.Default.Register<bool>(this,Token.LoadSAP2000Anlysis, t =>
            {
                if (t == true)
                {
                    container.Content = new ProgressView();
                }
                else 
                {
                    container.Content = SAP2000AnalysisView.GetInstance();
                }
            }); 
        }
        public Document doc { get; set; }
        public UIDocument uidoc { get; set; }
        //public void Initial(Document doc)
        //{
        //    Loaded += (s, e) =>
        //    {
        //        gridAndLevelView = GridAndLevelView.GetInstance();
        //        gridAndLevelView.Initial();
        //        framingView = FramingView.GetInstance();
        //        framingView.Initial(doc);
        //        bracingView = BracingView.GetInstance();
        //        bracingView.Initial(doc);
        //        purlinView = PurlinView.GetInstance();
        //        purlinView.Initial(doc);
        //        RVTCreation = RVTCreation.GetInstance();
        //        RVTCreation.Initial(doc);
        //        Revit2SQL = Revit2SQL.GetInstance();
        //        Revit2SQL.Initial(doc);
        //    };
        //}
        //public static MainWindow GetInstance()
        //{
        //    if (_instance == null)
        //    {
        //        return new MainWindow();
        //    }
        //    return _instance;
        //}
        public GridAndLevelView gridAndLevelView { get; set; }
        public FramingView framingView { get; set; }
        public BracingView bracingView { get; set; }
        public PurlinView purlinView { get; set; }
        public RVTCreationView RVTCreationView { get; set; }
        public LoadSetView loadSetView { get; set; }
        public LoadDisplayView loadDisplayView { get; set; }
        public LoadCombinationView loadCombinationView { get; set; }
        public Revit2SQLView Revit2SQLView { get; set; }
        public SAP2000AnalysisView SAP2000AnalysisView { get; set; }
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (!(sender is RadioButton button)) return;
            if (string.IsNullOrEmpty(button.Content.ToString())) return;
            switch (button.Content.ToString())
            {
                
                case "开间与净深":container.Content = GridAndLevelView.GetInstance(); break;
                case "主刚架":container.Content =FramingView.GetInstance(); break;
                case "支撑": container.Content = BracingView.GetInstance(); break;
                case "檩条": container.Content = PurlinView.GetInstance();  break;
                case "RVT模型生成":   container.Content =new RVTCreationView(doc);  break;
                case "荷载信息": container.Content = LoadSetView.GetInstance();  break;
                case "荷载显示":  container.Content= new LoadDisplayView(doc); break;
                case "荷载组合":  container.Content= LoadCombinationView.GetInstance(); break;
                case "Revit": container.Content = Revit2SQLView.GetInstance(); break;
                case "内力分析":container.Content=SAP2000AnalysisView.GetInstance(); break;
                default:
                    break;
                
            }
         }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            container.Content= GridAndLevelView.GetInstance();
        }
    }
}
