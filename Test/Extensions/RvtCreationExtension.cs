using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.Extensions
{
    public class RvtCreationExtension
    {
        /// <summary>
        /// 得到实体柱的分析模型的端点引用
        /// </summary>
        /// <param name="col">实体柱</param>
        /// <returns></returns>
        public static Reference GetAMColReference(FamilyInstance col)
        {
            AnalyticalModel am = col.GetAnalyticalModel();
            Curve curve = am.GetCurve();
            AnalyticalModelSelector selector = new AnalyticalModelSelector(curve);
            selector.CurveSelector = AnalyticalCurveSelector.StartPoint;
            Reference reference = am.GetReference(selector);

            return reference;
        }
        /// <summary>
        /// 族实例在中心点旋转
        /// </summary>
        /// <param name="element">旋转的元素</param>
        /// <param name="angle">旋转的角度</param>
        public static Element RotateByCenter(Document doc, Element element, double angle)
        {
            LocationPoint locationPoint = element.Location as LocationPoint;
            if (locationPoint != null)
            {
                XYZ centerPoint = locationPoint.Point;
                Line rotateAxis = Line.CreateBound(centerPoint, centerPoint + XYZ.BasisZ);
                ElementTransformUtils.RotateElement(doc, element.Id, rotateAxis, angle / 180 * Math.PI);
            }
            return element;
        }
        public static List<double> GetWindResistantSpace(string str)
        {
            List<double> WindResistantSpaceList = new List<double>();
            string[] temp = str.Split(';');
            for (int i = 0; i < temp.Length; i++)
            {
                WindResistantSpaceList.Add(Convert.ToDouble(temp[i]) / 304.8);
            }
            return WindResistantSpaceList;
        }
    }
}
