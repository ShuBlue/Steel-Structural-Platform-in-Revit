using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Lighting;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Web.Util;
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
    /// LoadDisplayView.xaml 的交互逻辑
    /// </summary>
    public partial class LoadDisplayView : UserControl
    {
        public LoadDisplayView(Document doc)
        {
            InitializeComponent();
            this.doc = doc;
            Loads.Items.Add("Dead");
            Loads.Items.Add("Live");
            Loads.Items.Add("WY+(+i)");
            Loads.Items.Add("WY+(-i)");
            Loads.Items.Add("WY-(+i)");
            Loads.Items.Add("WY-(-i)");
            Loads.Items.Add("WX+(+i)");
            Loads.Items.Add("WX+(-i)");
            Loads.Items.Add("WX-(+i)");
            Loads.Items.Add("WX-(-i)");
            Loads.SelectedIndex = 0;

          
        }
        public Document doc { get; set; }
        public RVTCreationView RVTCreation { get; set; }

        private void Loads_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            try
            {
                FilteredElementCollector viewFilter = new FilteredElementCollector(doc).OfClass(typeof(View));
                View view = viewFilter.Cast<View>().FirstOrDefault(t => t.Name == "分析模型");
                using (Transaction ts = new Transaction(doc, "显示目标荷载"))
                {
                    ts.Start();
                    view.DisableTemporaryViewMode(TemporaryViewMode.TemporaryHideIsolate);
                    FilteredElementCollector lineLoadCollector = new FilteredElementCollector(doc).OfClass(typeof(LineLoad));
                    List<LineLoad> hideLineLoad = lineLoadCollector.Cast<LineLoad>().Where(t => t.LoadCaseName != Loads.SelectedValue.ToString()).ToList();
                    List<ElementId> ids = new List<ElementId>();
                    for (int i = 0; i < hideLineLoad.Count; i++)
                    {
                        ids.Add(hideLineLoad[i].Id);
                    }
                    view.HideElementsTemporary(ids);
                    ts.Commit();
                }

                PreviewControl previewControl = Grid.Children.Cast<PreviewControl>().FirstOrDefault();
                if (previewControl != null)
                {
                    previewControl.Dispose();
                }

                Grid.Children.Add(new PreviewControl(doc, view.Id));
            }
            catch
            {
               
                
            }
        }
    }
}
