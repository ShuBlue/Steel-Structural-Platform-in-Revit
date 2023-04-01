using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using System;

namespace Demo
{
    public class Extension
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
        public static Element RotateByCenter(Document doc,Element element,double angle)
        {
            LocationPoint locationPoint = element.Location as LocationPoint;
            if (locationPoint != null)
            {
                XYZ centerPoint = locationPoint.Point;
                Line rotateAxis = Line.CreateBound(centerPoint, centerPoint+XYZ.BasisZ);
                ElementTransformUtils.RotateElement(doc,element.Id, rotateAxis, angle / 180 * Math.PI);
            }
            return element;
        }
    }
}
