using Autodesk.Revit.DB;
using SAP2000v1;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml.Linq;

namespace Test.Extensions
{
    public static class SAP2000Extension
    {

        /// <summary>
        /// 在SAP2000中创建截面，创建截面需要材料属性
        /// </summary>
        /// <param name="sapModel">sap2000项目</param>
        /// <param name="sectionShape">数据库中的SectionShape</param>
        /// <param name="sectionDimension">数据库中的SecitonDimension</param>
        /// <param name="material">数据库中的Material</param>
        /// <returns></returns>
        public static string GetSection(cSapModel sapModel,string sectionShape, string sectionDimension,string material)
        {
            int ret = 0;
            if (material == " Q345")
            {
                material = "Q355";
            }
            //查找材料名称，若已包含则不重复导入，无则进行导入
            int NumberNames = 0;
            string[] materialList = null;
            ret = sapModel.PropMaterial.GetNameList(ref NumberNames, ref materialList);
            bool isContainMaterial=materialList.Contains(material);
            string materialName = null;          
            if (!isContainMaterial)
            {
                ret = sapModel.PropMaterial.AddMaterial(ref materialName, eMatType.Steel, "China", "GB", material);
            }
            else
            {
                materialName = material;
            }
            //将数据库中的框架截面信息与SAP2000中的框架截面信息进行匹配
            if (sectionShape.Contains("H型"))
            {
                
            }
            else if (sectionShape.Contains("工字"))
            {
                sectionDimension = "GB-" + sectionDimension;
            }
            else if (sectionShape.Contains("钢管"))
            {
                sectionDimension="GB-SSP"+sectionDimension;
            }
            //查找框架属性名称，若已包含则不重复导入，无则进行导入
            string[] frameList = null;
            ret=sapModel.PropFrame.GetNameList(ref NumberNames, ref frameList);
            if (frameList == null)
            {
                ret = sapModel.PropFrame.ImportProp(sectionDimension, materialName, "ChineseGB08.xml", sectionDimension);
            }
            else
            {
                bool isContainFrame = frameList.Contains(sectionDimension);
                if (!isContainFrame)
                {
                    ret = sapModel.PropFrame.ImportProp(sectionDimension, materialName, "ChineseGB08.xml", sectionDimension);
                }
            }
            //设置框架的属性修正系数
            double[] ModValue = new double[8];
            for (int j = 0; j <= 7; j++)
            {
                ModValue[j] = 1;
            }
            ret = sapModel.PropFrame.GetNameList(ref NumberNames, ref frameList);
            for (int i = 0; i < NumberNames; i++)
            {
                ret = sapModel.PropFrame.SetModifiers(frameList[i],ref ModValue);
            }

            return sectionDimension;

        }


        /// <summary>
        /// 在SAP2000中创建梁柱等元素
        /// </summary>
        /// <param name="sapModel">SAP2000项目</param>
        /// <param name="x1">起点x</param>
        /// <param name="y1">起点y</param>
        /// <param name="z1">起点z</param>
        /// <param name="x2">终点x</param>
        /// <param name="y2">终点y</param>
        /// <param name="z2">终点z</param>
        /// <param name="section">截面（由GetSection方法得来）</param>
        /// <returns></returns>
        public static int CreateElement(cSapModel sapModel,double x1, double y1, double z1, double x2, double y2, double z2,string section,ref string elementName)
        {
            int ret = 0;
            ret = sapModel.FrameObj.AddByCoord(x1, y1, z1, x2, y2, z2, ref elementName, section, "Global");
            return ret;
        }


        /// <summary>
        /// 给构件添加Dead,Liev,WX+(+i),WX+(-i),WX-(+i),WX-(-i),WY+(+i),WY+(-i),WY-(+i),WY-(-i)
        /// </summary>
        /// <param name="sapmodel"></param>
        /// <param name="name">元素引用</param>
        /// <param name="elementId">元素Id</param>
        /// <returns></returns>
        public static int SetLoad(cSapModel sapModel, string name,int elementId)
        {
            int ret = 0;
            string sql="select * from LoadInfo where Element_Id=@elementId";
            DBHelper.PrepareSql(sql);
            DBHelper.SetParameter("elementId", elementId);
            DataTable dt = DBHelper.ExecQuery();
            ret = InternalSetLoad(sapModel, "Dead_x", "Dead", dt, ref name);
            ret = InternalSetLoad(sapModel, "Dead_y", "Dead", dt, ref name);
            ret = InternalSetLoad(sapModel, "Dead_z", "Dead", dt, ref name);

            ret = InternalSetLoad(sapModel, "Live_x", "Live", dt, ref name);
            ret = InternalSetLoad(sapModel, "Live_y", "Live", dt, ref name);
            ret = InternalSetLoad(sapModel, "Live_z", "Live", dt, ref name);

            ret = InternalSetLoad(sapModel, "WX0_0_x", "WX+(+i)", dt, ref name);
            ret = InternalSetLoad(sapModel, "WX0_0_y", "WX+(+i)", dt, ref name);
            ret = InternalSetLoad(sapModel, "WX0_0_z", "WX+(+i)", dt, ref name);

            ret = InternalSetLoad(sapModel, "WX0_1_x", "WX+(-i)", dt, ref name);
            ret = InternalSetLoad(sapModel, "WX0_1_y", "WX+(-i)", dt, ref name);
            ret = InternalSetLoad(sapModel, "WX0_1_z", "WX+(-i)", dt, ref name);

            ret = InternalSetLoad(sapModel, "WX1_0_x", "WX-(+i)", dt, ref name);
            ret = InternalSetLoad(sapModel, "WX1_0_y", "WX-(+i)", dt, ref name);
            ret = InternalSetLoad(sapModel, "WX1_0_z", "WX-(+i)", dt, ref name);

            ret = InternalSetLoad(sapModel, "WX1_1_z", "WX-(-i)", dt, ref name);
            ret = InternalSetLoad(sapModel, "WX1_1_y", "WX-(-i)", dt, ref name);
            ret = InternalSetLoad(sapModel, "WX1_1_z", "WX-(-i)", dt, ref name);


            ret = InternalSetLoad(sapModel, "WY0_0_x", "WY+(+i)", dt, ref name);
            ret = InternalSetLoad(sapModel, "WY0_0_y", "WY+(+i)", dt, ref name);
            ret = InternalSetLoad(sapModel, "WY0_0_z", "WY+(+i)", dt, ref name);

            ret = InternalSetLoad(sapModel, "WY0_1_x", "WY+(-i)", dt, ref name);
            ret = InternalSetLoad(sapModel, "WY0_1_y", "WY+(-i)", dt, ref name);
            ret = InternalSetLoad(sapModel, "WY0_1_z", "WY+(-i)", dt, ref name);

            ret = InternalSetLoad(sapModel, "WY1_0_x", "WY-(+i)", dt, ref name);
            ret = InternalSetLoad(sapModel, "WY1_0_y", "WY-(+i)", dt, ref name);
            ret = InternalSetLoad(sapModel, "WY1_0_z", "WY-(+i)", dt, ref name);

            ret = InternalSetLoad(sapModel, "WY1_1_z", "WY-(-i)", dt, ref name);
            ret = InternalSetLoad(sapModel, "WY1_1_y", "WY-(-i)", dt, ref name);
            ret = InternalSetLoad(sapModel, "WY1_1_z", "WY-(-i)", dt, ref name);

            return ret;
        }
        private static int InternalSetLoad(cSapModel sapModel,string fieldInDB, string field,DataTable dt,ref string name)
        {
            int ret = 0;
            object queryResult = dt.Rows[0][fieldInDB];
            if (queryResult != null)
            {
                string queryResultStr = queryResult.ToString();
                string[] strings = queryResultStr.Split(';');
                for (int i = 0; i < strings.Length - 1; i++)
                {
                    int loadValStart = strings[i].IndexOf(':')+1;
                    int loadValEnd = strings[i].IndexOf('k') - 1;
                    double loadVal = Convert.ToDouble(strings[i].ToString().Substring(loadValStart, loadValEnd-loadValStart+1));
                    if (loadVal == 0) continue;
                    int dis1Start = strings[i].IndexOf('(')+1;
                    int dis1End = strings[i].IndexOf(',')-1;
                    double dist1 = Convert.ToDouble(strings[i].ToString().Substring(dis1Start,dis1End-dis1Start+1));
                    int dis2Start = strings[i].IndexOf(',') + 1;
                    int dis2End = strings[i].IndexOf(')') - 1;
                    double dist2 = Convert.ToDouble(strings[i].ToString().Substring(dis2Start, dis2End-dis2Start+1));
                    int dir;
                    if (fieldInDB.Last()=='x')
                    {
                        dir = 4;
                    }
                    else if (fieldInDB.Last()=='y')
                    {
                        dir = 5;
                    }
                    else 
                    {
                        dir = 6;
                    }
                    ret = sapModel.FrameObj.SetLoadDistributed(name, field, 1, dir, dist1, dist2, loadVal, loadVal,"Global",true,false);
                }
            }
            return ret;
        }
    }
}
