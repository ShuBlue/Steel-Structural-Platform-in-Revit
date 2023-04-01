using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
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
using UIFramework;

namespace Test.UserControls
{
    /// <summary>
    /// LoadCombinations.xaml 的交互逻辑
    /// </summary>
    public partial class LoadCombinationView : UserControl
    {
        private static LoadCombinationView _instance;
        private LoadCombinationView()
        {
            InitializeComponent();
        }
        public static LoadCombinationView GetInstance()
        {
            if (_instance == null)
            {
                _instance=new LoadCombinationView();    
            }
            return _instance;
        }
        public void Initial(Document doc)
        {
            this.doc = doc;
            StructureImportanceCoefficient.Text = "1.0";
            DeadLoadPartialCoefficient.Text = "1.3";
            LiveLoadCombinationCoefficient.Text = "0.7";
            LiveLoadPartialCoefficient.Text = "1.5";
            WindLoadCombinationCoefficient.Text = "0.6";
            WindLoadPartialCoefficient.Text = "1.5";
            GravityPartialCoefficient.Text = "1.3";
            HorizentalEarthquakePartialCoefficient.Text = "1.4";
            FilteredElementCollector loadCombinationFilter = new FilteredElementCollector(doc).OfClass(typeof(LoadCombination));
            List<LoadCombination> loadCombinations = loadCombinationFilter.Cast<LoadCombination>().ToList();
            for (int i = 0; i < loadCombinations.Count; i++)
            {
                StringBuilder s = new StringBuilder();
                List<LoadComponent> components = loadCombinations[i].GetComponents().ToList();
                for (int j = 0; j < components.Count; j++)
                {
                    if (doc.GetElement(components[j].LoadCaseOrCombinationId).Name == "Dead")
                    {
                        s.Append(components[j].Factor.ToString() + " * " + "Dead");
                    }
                    if (components[j].Factor > 0&& doc.GetElement(components[j].LoadCaseOrCombinationId).Name != "Dead")
                    {
                        s.Append(" + ");
                        s.Append(components[j].Factor.ToString() + " * " + doc.GetElement(components[j].LoadCaseOrCombinationId).Name);
                    }
                    if (components[j].Factor < 0)
                    {
                        s.Append(" - ");
                        s.Append(Math.Abs(components[j].Factor).ToString() + " * " + doc.GetElement(components[j].LoadCaseOrCombinationId).Name);
                    }
                }
                ResultOfCombination.Items.Add(s);
            }
        }
        public Document doc { get; set; }
        private void Button_Click(object sender, RoutedEventArgs e)
        {

            List<LoadCombination> loadCombinations = new List<LoadCombination>();
            int count = 0;
            FilteredElementCollector loadCaseFilter = new FilteredElementCollector(doc).OfClass(typeof(LoadCase));
            LoadCase dead = loadCaseFilter.Cast<LoadCase>().FirstOrDefault(t => t.Name == "Dead");
            LoadCase live = loadCaseFilter.Cast<LoadCase>().FirstOrDefault(t => t.Name == "Live");
            List<LoadCase> windList = loadCaseFilter.Cast<LoadCase>().Where(t => t.Name.Contains("WX")||t.Name.Contains("WY")).ToList();
            List<LoadCase> earthquakeList = loadCaseFilter.Cast<LoadCase>().Where(t => t.Name=="EX"||t.Name=="EY").ToList();
            double deadPartialCoefficient=Convert.ToDouble(DeadLoadPartialCoefficient.Text);
            double livePartialCoefficient = Convert.ToDouble(LiveLoadPartialCoefficient.Text);
            double liveCombinationCoefficient = Convert.ToDouble(LiveLoadCombinationCoefficient.Text);
            double windPartialCoefficient = Convert.ToDouble(WindLoadPartialCoefficient.Text);
            double windCombinationCoefficient = Convert.ToDouble(WindLoadCombinationCoefficient.Text);
            double gravityPartialCoefficient = Convert.ToDouble(GravityPartialCoefficient.Text);
            double earthquakePartialCoefficient = Convert.ToDouble(HorizentalEarthquakePartialCoefficient.Text);
            double[,] deadMatrix = new double[,] { { deadPartialCoefficient }, { 1.0 }};
            double[,] liveMatrix = new double[,] { { livePartialCoefficient, 1.0 }, { livePartialCoefficient, liveCombinationCoefficient },{ 0,1.0} };
            double[,] windMatrix = new double[,] { { windPartialCoefficient, 1.0 }, { windPartialCoefficient, windCombinationCoefficient },{ 0,1.0} };
            //  |1.5|   |1.5  1.0|   |1.5  1.0|  |1.4 |
            //  |1.0|   |1.5  0.7|   |1.5  0.6|  |-1.4|
            //          |0    1.0|   |0    1.0|
            double[,] gravityMatrixForEarthquake = new double[,] { { gravityPartialCoefficient }, { 1.0 } };
            double[,] earthquakeMatrixForEarthquake = new double[,] { { earthquakePartialCoefficient }, { -earthquakePartialCoefficient } };
            using (Transaction ts = new Transaction(doc, "LoadCombination"))
            {
                ts.Start();
                //恒、活、风组合
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        for (int k = 0; k < 3; k++)
                        {
                            if (j == k || (j == 1 && k == 2) || (j == 2 && k == 1)) continue;
                            double deadCoefficient = deadMatrix[i, 0];
                            double liveCoefficient = liveMatrix[j, 0] * liveMatrix[j, 1];
                            double windCoefficient = windMatrix[k, 0] * liveMatrix[k, 1];
                           
                            StringBuilder temp1 = new StringBuilder();

                            LoadComponent deadComponent = new LoadComponent(dead.Id, deadCoefficient);
                            if (deadCoefficient != 0)
                            {
                                temp1.Append(deadCoefficient.ToString() + " * " + dead.Name);
                            }
                            LoadComponent liveComponent = new LoadComponent(live.Id, liveCoefficient);
                            if (liveCoefficient != 0)
                            {
                                temp1.Append(" + ");
                                temp1.Append(liveCoefficient.ToString() + " * " + live.Name);
                            }
                            if (windCoefficient == 0)
                            {
                                count++;
                                LoadCombination LC = LoadCombination.Create(doc, "LC" + count.ToString(), LoadCombinationType.Combination, LoadCombinationState.Ultimate);
                                LC.SetComponents(new List<LoadComponent>() { deadComponent, liveComponent });
                                ResultOfCombination.Items.Add(temp1);
                            }
                            else
                            {
                                for (int z = 0; z < windList.Count; z++)
                                {
                                    count++;
                                    StringBuilder temp2 = new StringBuilder();
                                    temp2.Append(temp1 + " + ");
                                    temp2.Append(windCoefficient.ToString() + " * " + windList[z].Name);
                                    LoadComponent windComponent = new LoadComponent(windList[z].Id, windCoefficient);
                                    LoadCombination LC = LoadCombination.Create(doc, "LC" + count.ToString(), LoadCombinationType.Combination, LoadCombinationState.Ultimate);
                                    LC.SetComponents(new List<LoadComponent>() { deadComponent, liveComponent, windComponent });
                                    ResultOfCombination.Items.Add(temp2);
                                }
                            }                          
                        }
                    }                   
                }
                //地震荷载参与组合
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        double deadCoefficient = gravityMatrixForEarthquake[i, 0];
                        double liveCoefficient =0.5* gravityMatrixForEarthquake[i, 0];
                        StringBuilder temp1 = new StringBuilder();
                        LoadComponent deadComponent = new LoadComponent(dead.Id, deadCoefficient);
                        temp1.Append(deadCoefficient.ToString() + " * " + dead.Name);
                        LoadComponent liveComponent=new LoadComponent(live.Id, liveCoefficient);
                        temp1.Append(" + ");
                        temp1.Append(liveCoefficient.ToString() + " * " + live.Name);
                        for (int k = 0; k < 2; k++)
                        {
                            StringBuilder temp2=new StringBuilder();
                            double earthquakeCoefficient = earthquakeMatrixForEarthquake[k,0];
                            if (earthquakeCoefficient < 0)
                            {
                                temp2.Append(temp1+" - ");                              
                            }
                            if (earthquakeCoefficient > 0)
                            {
                                temp2.Append(temp1 + " + ");
                            }
                            count++;
                            temp2.Append(Math.Abs(earthquakeCoefficient).ToString() + " * " + earthquakeList[k].Name);
                            LoadComponent earthquakeComponent = new LoadComponent(earthquakeList[k].Id, earthquakeCoefficient);
                            LoadCombination LC = LoadCombination.Create(doc, "LC" + count.ToString(), LoadCombinationType.Combination, LoadCombinationState.Ultimate);
                            LC.SetComponents(new List<LoadComponent>() { deadComponent, liveComponent, earthquakeComponent });
                            ResultOfCombination.Items.Add(temp2);
                        }

                    }
                }
                ts.Commit();
            }
        }
    }
}
