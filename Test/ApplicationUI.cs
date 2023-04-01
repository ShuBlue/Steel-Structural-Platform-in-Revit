using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Test.Extensions;

namespace Test
{
    public class ApplicationUI : IExternalApplication
    {
        private const string _tab = "单层钢结构正向设计平台";
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            application.CreateRibbonTab(_tab);
            RibbonPanel panel = application.CreateRibbonPanel(_tab, "资源");
            panel.CreateButton<Command>((t) =>
            {
                t.Text = "正向设计平台";
                string imageSource = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\Images\Icon.png";
                t.LargeImage = new BitmapImage(new Uri(imageSource));
            });
            return Result.Succeeded;
        }
    }
}
