using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Demo_StructuralAnalysis
{
    [Transaction(TransactionMode.Manual)]
    public class Demo : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Dialog dialog = new Dialog();
            if (dialog.ShowDialog().Value)
            {
                return Result.Succeeded;    
            }
            return Result.Cancelled;
        }
    }
}
