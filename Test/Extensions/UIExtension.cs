using Autodesk.Revit.UI;
using System;
using System.Windows;

using System.Collections.Generic;
using System.Text;
using System.Windows.Media.Imaging;

namespace Test.Extensions
{
    public static class UIExtension
    {
        public static RibbonPanel CreateButton<T>(this RibbonPanel panel, Action<PushButtonData> action)
        {
            Type type = typeof(T);
            string name = $"btn_{type.Name}";
            PushButtonData pushButtonData = new PushButtonData(name, name, type.Assembly.Location, type.FullName);
            action.Invoke(pushButtonData);
            panel.AddItem(pushButtonData);
            return panel;
        }

        
    }
}
