using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Media3D;

namespace Test.Extensions
{
    public class SetLoadExtension
    {
        /// <summary>
        /// 使用射线法找到距离最近的同类型实例，并返回距离（此处只能查询梁和柱）
        /// </summary>
        /// <param name="instance">查询的实例</param>
        /// <param name="direction">查询的方向</param>
        /// <param name="referenceIntersector">查询过滤器</param>
        /// <returns></returns>
        public static double CalculateNearestDistance(Document doc,FamilyInstance instance, XYZ direction,ReferenceIntersector referenceIntersector)
        {
            FilteredElementCollector view3DFilter = new FilteredElementCollector(doc).OfClass(typeof(View3D));
            View3D View3D = view3DFilter.Cast<View3D>().FirstOrDefault(t=>t.Name=="{三维}");
            BoundingBoxXYZ box = instance.get_BoundingBox(View3D);
            XYZ Center = box.Min.Add(box.Max).Multiply(0.5);
            ReferenceWithContext referenceWithContext = referenceIntersector.FindNearest(Center, direction);
            if (referenceWithContext != null)
            {
                FamilyInstance other = doc.GetElement(referenceWithContext.GetReference()) as FamilyInstance;
                if (instance.StructuralType == Autodesk.Revit.DB.Structure.StructuralType.Column)
                {                 
                    return UnitExtension.ConvertToMillimeters((instance.Location as LocationPoint).Point.DistanceTo((other.Location as LocationPoint).Point));
                }
                else if (instance.StructuralType == Autodesk.Revit.DB.Structure.StructuralType.Beam)
                {
                    return UnitExtension.ConvertToMillimeters((instance.Location as LocationCurve).Curve.Distance((other.Location as LocationCurve).Curve.GetEndPoint(0)));
                }
                else
                {
                    return 0;
                }
            }
            return 0;
        }

        /// <summary>
        /// 计算构件的风压高度变化系数
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="instance">构件</param>
        /// <param name="groundRoughness">地面粗糙度</param>
        /// <returns></returns>
        public static double GetHeightVariationCoefficientOfWindPressure(Document doc,FamilyInstance instance,string groundRoughness)
        {
            string sql = "Select * from HeightVariationCoefficientOfWindPressure";
            DBHelper.PrepareSql(sql);
            DataTable dt=DBHelper.ExecQuery();
            double height=0;
            if (instance.StructuralType==Autodesk.Revit.DB.Structure.StructuralType.Column)//为柱
            {
                Level TopLevel= doc.GetElement(instance.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM).AsElementId()) as Level;
                double TopOffset = instance.get_Parameter(BuiltInParameter.SCHEDULE_TOP_LEVEL_OFFSET_PARAM).AsDouble();
                height= UnitExtension.ConvertToMillimeters(TopLevel.Elevation + TopOffset);
                             
            }
            if (instance .StructuralType==Autodesk.Revit.DB.Structure.StructuralType.Beam)//为梁
            {
                Curve curve = (instance.Location as LocationCurve).Curve;
                XYZ start = curve.GetEndPoint(0);
                XYZ end = curve.GetEndPoint(1);
                height =UnitExtension.ConvertToMillimeters(Math.Max(start.Z, end.Z));
                
            }
            int count = 0;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                double temp = Convert.ToDouble(dt.Rows[i]["Height"]);
                if (height > temp)
                {
                    count++;
                    break;
                }
            }
            if (count == 0)
            {
                return Convert.ToDouble(dt.Rows[0]["Factor" + groundRoughness]);
            }
            else
            {
                return Convert.ToDouble(dt.Rows[count - 1]["Factor" + groundRoughness]) + (height - Convert.ToDouble(dt.Rows[count - 1]["Height"])) / (Convert.ToDouble(dt.Rows[count]["Height"]) - Convert.ToDouble(dt.Rows[count - 1]["Height"])) * (Convert.ToDouble(dt.Rows[count]["Factor" + groundRoughness]) - Convert.ToDouble(dt.Rows[count - 1]["Factor" + groundRoughness]));
            }
        }
        
    }
}
