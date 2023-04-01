using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using System;
using System.Collections.Generic;
using System.Data;
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
using Test.Entity;
using Test.Extensions;

namespace Test.UserControls
{
    /// <summary>
    /// Revit2SQL.xaml 的交互逻辑
    /// </summary>
    public partial class Revit2SQLView : UserControl
    {
        private static Revit2SQLView _instance;
        
        public Revit2SQLView()
        {
            InitializeComponent();
        }
        public void Initial(Document doc)
        {
            this.doc = doc;
            this.Type.Items.Add("刚架柱");
            this.Type.Items.Add("刚架梁");
            this.Type.Items.Add("系杆");
            this.Type.Items.Add("支撑");
            this.Type.Items.Add("抗风柱");
            this.Type.SelectedIndex = 0;

        }
        public static Revit2SQLView GetInstance()
        {
            if (_instance == null)
            {
                _instance = new Revit2SQLView();
            }
            return _instance;
        }
        public Document doc { get; set; }
        private void sumbit_Click(object sender, RoutedEventArgs e)
        {


            #region 提取构件基本信息至数据库中
            FilteredElementCollector columnCol = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_StructuralColumns).OfClass(typeof(FamilyInstance));
            IList<Element> allColumns = columnCol.ToElements();
            List<FamilyInstance> columns = new List<FamilyInstance>();
            List<FamilyInstance> windResistantColumns = new List<FamilyInstance>();
            List<ElementId> columnsId = new List<ElementId>();
            foreach (FamilyInstance item in allColumns)
            {
                string token = item.get_Parameter(BuiltInParameter.DOOR_NUMBER).AsString();
                if (token == "刚架柱")
                {
                    columns.Add(item);
                }
                if (token == "抗风柱")
                {
                    windResistantColumns.Add(item);
                }
            }

            FilteredElementCollector FramingCol = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_StructuralFraming).OfClass(typeof(FamilyInstance));
            IList<Element> allFramings = FramingCol.ToElements();
            List<FamilyInstance> beams = new List<FamilyInstance>();
            List<FamilyInstance> ties = new List<FamilyInstance>();
            List<FamilyInstance> bracings = new List<FamilyInstance>();

            List<ElementId> beamsId = new List<ElementId>();
            foreach (FamilyInstance item in allFramings)
            {
                string token = item.get_Parameter(BuiltInParameter.DOOR_NUMBER).AsString();
                if (token == "刚架梁")
                {
                    beams.Add(item);
                }
                if (token == "系杆")
                {
                    ties.Add(item);
                }
                if (token == "支撑")
                {
                    bracings.Add(item);
                }

            }

            #region 主刚架信息提取至ColumnInfo表中
            //提取刚架柱的关键信息
            List<ColumnInfo> columnsInfos = new List<ColumnInfo>();
            foreach (FamilyInstance column in columns)
            {
                ColumnInfo columnInfo = new ColumnInfo();
                columnInfo.Id = column.Id.IntegerValue;
                columnInfo.StructuralType = column.get_Parameter(BuiltInParameter.DOOR_NUMBER).AsString();
                Entity.XYZ start = new Entity.XYZ();
                start.X = Math.Round(UnitExtension.ConvertToMillimeters((column.Location as LocationPoint).Point.X));
                start.Y = Math.Round(UnitExtension.ConvertToMillimeters((column.Location as LocationPoint).Point.Y));
                start.Z = Math.Round(UnitExtension.ConvertToMillimeters((column.Location as LocationPoint).Point.Z));
                columnInfo.Start = start;

                Entity.XYZ end = new Entity.XYZ();
                end.X = Math.Round(UnitExtension.ConvertToMillimeters((column.Location as LocationPoint).Point.X));
                end.Y = Math.Round(UnitExtension.ConvertToMillimeters((column.Location as LocationPoint).Point.Y));
                end.Z = Math.Round(UnitExtension.ConvertToMillimeters((doc.GetElement(column.get_Parameter(BuiltInParameter.SCHEDULE_TOP_LEVEL_PARAM).AsElementId()) as Level).Elevation));
                columnInfo.End = end;

                columnInfo.SectionShape = column.Symbol.FamilyName;
                columnInfo.SectionDimension = column.Symbol.Name;
                columnInfo.Material = column.get_Parameter(BuiltInParameter.STRUCTURAL_MATERIAL_PARAM).AsValueString().Substring(6);

                columnsInfos.Add(columnInfo);
            }
            //将刚架柱的关键信息存储至数据库中
            for (int i = 0; i < columnsInfos.Count; i++)
            {
                string sql = "insert into ColumnInfo(Id,structuralType,startX,startY,startZ,endX,endY,endZ,SectionShape,SectionDimension,Material) " +
                    "Values(@Id,@structuralType,@startX,@startY,@startZ,@endX,@endY,@endZ,@SectionShape,@SectionDimension,@Material)";
                DBHelper.PrepareSql(sql);
                DBHelper.SetParameter("Id", columnsInfos[i].Id);
                DBHelper.SetParameter("structuralType", columnsInfos[i].StructuralType);
                DBHelper.SetParameter("startX", columnsInfos[i].Start.X);
                DBHelper.SetParameter("startY", columnsInfos[i].Start.Y);
                DBHelper.SetParameter("startZ", columnsInfos[i].Start.Z);
                DBHelper.SetParameter("endX", columnsInfos[i].End.X);
                DBHelper.SetParameter("endY", columnsInfos[i].End.Y);
                DBHelper.SetParameter("endZ", columnsInfos[i].End.Z);
                DBHelper.SetParameter("SectionShape", columnsInfos[i].SectionShape);
                DBHelper.SetParameter("SectionDimension", columnsInfos[i].SectionDimension);
                DBHelper.SetParameter("Material", columnsInfos[i].Material);
                DBHelper.ExecNonQuery();
            }
            #region 主刚架Id存储至LoadInfo表中
            for (int i = 0; i < columnsInfos.Count; i++)
            {
                string sql = "insert into LoadInfo(Element_Id) " +
                    "Values(@Id)";
                DBHelper.PrepareSql(sql);
                DBHelper.SetParameter("Id", columnsInfos[i].Id);
                DBHelper.ExecNonQuery();
            }
            #endregion
            #endregion

            #region 抗风柱信息提取至WindResistantColumnInfo表中
            //提取抗风柱的关键信息
            List<WindResistantColumnInfo> windResistantColumnsInfos = new List<WindResistantColumnInfo>();
            foreach (FamilyInstance windResistantColumn in windResistantColumns)
            {
                WindResistantColumnInfo windResistantColumnInfo = new WindResistantColumnInfo();
                windResistantColumnInfo.Id = windResistantColumn.Id.IntegerValue;
                windResistantColumnInfo.StructuralType = windResistantColumn.get_Parameter(BuiltInParameter.DOOR_NUMBER).AsString();
                Entity.XYZ start = new Entity.XYZ();
                start.X = Math.Round(UnitExtension.ConvertToMillimeters((windResistantColumn.Location as LocationPoint).Point.X));
                start.Y = Math.Round(UnitExtension.ConvertToMillimeters((windResistantColumn.Location as LocationPoint).Point.Y));
                start.Z = Math.Round(UnitExtension.ConvertToMillimeters((windResistantColumn.Location as LocationPoint).Point.Z));
                windResistantColumnInfo.Start = start;

                Entity.XYZ end = new Entity.XYZ();
                end.X = Math.Round(UnitExtension.ConvertToMillimeters((windResistantColumn.Location as LocationPoint).Point.X));
                end.Y = Math.Round(UnitExtension.ConvertToMillimeters((windResistantColumn.Location as LocationPoint).Point.Y));
                end.Z = Math.Round(UnitExtension.ConvertToMillimeters((doc.GetElement(windResistantColumn.get_Parameter(BuiltInParameter.SCHEDULE_TOP_LEVEL_PARAM).AsElementId()) as Level).Elevation
                    + windResistantColumn.get_Parameter(BuiltInParameter.SCHEDULE_TOP_LEVEL_OFFSET_PARAM).AsDouble()));
                windResistantColumnInfo.End = end;

                double X = windResistantColumn.FacingOrientation.X;
                double Y = windResistantColumn.FacingOrientation.Y;
                windResistantColumnInfo.Angle = 180 / Math.PI * Math.Acos(Y / Math.Sqrt(X * X + Y * Y));
                windResistantColumnInfo.SectionShape = windResistantColumn.Symbol.FamilyName;
                windResistantColumnInfo.SectionDimension = windResistantColumn.Symbol.Name;
                windResistantColumnInfo.Material = windResistantColumn.get_Parameter(BuiltInParameter.STRUCTURAL_MATERIAL_PARAM).AsValueString().Substring(6);
                windResistantColumnsInfos.Add(windResistantColumnInfo);
            }
            //将抗风柱的关键信息存储至数据库中
            for (int i = 0; i < windResistantColumnsInfos.Count; i++)
            {
                string sql = "insert into windResistantColumnInfo(Id,structuralType,startX,startY,startZ,endX,endY,endZ,angle,SectionShape,SectionDimension,Material) " +
                    "Values(@Id,@structuralType,@startX,@startY,@startZ,@endX,@endY,@endZ,@angle,@SectionShape,@SectionDimension,@Material)";
                DBHelper.PrepareSql(sql);
                DBHelper.SetParameter("Id", windResistantColumnsInfos[i].Id);
                DBHelper.SetParameter("structuralType", windResistantColumnsInfos[i].StructuralType);
                DBHelper.SetParameter("startX", windResistantColumnsInfos[i].Start.X);
                DBHelper.SetParameter("startY", windResistantColumnsInfos[i].Start.Y);
                DBHelper.SetParameter("startZ", windResistantColumnsInfos[i].Start.Z);
                DBHelper.SetParameter("endX", windResistantColumnsInfos[i].End.X);
                DBHelper.SetParameter("endY", windResistantColumnsInfos[i].End.Y);
                DBHelper.SetParameter("endZ", windResistantColumnsInfos[i].End.Z);
                DBHelper.SetParameter("angle", windResistantColumnsInfos[i].Angle);
                DBHelper.SetParameter("SectionShape", windResistantColumnsInfos[i].SectionShape);
                DBHelper.SetParameter("SectionDimension", windResistantColumnsInfos[i].SectionDimension);
                DBHelper.SetParameter("Material", windResistantColumnsInfos[i].Material);
                DBHelper.ExecNonQuery();
            }
            #region 抗风柱Id存储至LoadInfo表中
            for (int i = 0; i < windResistantColumnsInfos.Count; i++)
            {
                string sql = "insert into LoadInfo(Element_Id) " +
                    "Values(@Id)";
                DBHelper.PrepareSql(sql);
                DBHelper.SetParameter("Id", windResistantColumnsInfos[i].Id);
                DBHelper.ExecNonQuery();
            }
            #endregion
            #endregion

            #region 刚架梁信息提取至BeamInfo表中
            //提取所有刚架梁的关键信息
            List<BeamInfo> beamsInfos = new List<BeamInfo>();
            foreach (FamilyInstance beam in beams)
            {
                BeamInfo beamInfo = new BeamInfo();
                beamInfo.Id = beam.Id.IntegerValue;
                beamInfo.StructuralType = beam.get_Parameter(BuiltInParameter.DOOR_NUMBER).AsString();

                Autodesk.Revit.DB.XYZ origin = ((beam.Location as LocationCurve).Curve as Line).Origin;
                Autodesk.Revit.DB.XYZ direction = ((beam.Location as LocationCurve).Curve as Line).Direction;
                double length = ((beam.Location as LocationCurve).Curve as Line).Length;

                Entity.XYZ start = new Entity.XYZ();
                start.X = Math.Round(UnitExtension.ConvertToMillimeters(origin.X));
                start.Y = Math.Round(UnitExtension.ConvertToMillimeters(origin.Y));
                start.Z = Math.Round(UnitExtension.ConvertToMillimeters(origin.Z));
                beamInfo.Start = start;

                Entity.XYZ end = new Entity.XYZ();
                end.X = Math.Round(UnitExtension.ConvertToMillimeters(origin.X + length * direction.X / Math.Sqrt(direction.X * direction.X + direction.Y * direction.Y + direction.Z * direction.Z)));
                end.Y = Math.Round(UnitExtension.ConvertToMillimeters(origin.Y + length * direction.Y / Math.Sqrt(direction.X * direction.X + direction.Y * direction.Y + direction.Z * direction.Z)));
                end.Z = Math.Round(UnitExtension.ConvertToMillimeters(origin.Z + length * direction.Z / Math.Sqrt(direction.X * direction.X + direction.Y * direction.Y + direction.Z * direction.Z)));
                beamInfo.End = end;

                beamInfo.SectionShape = beam.Symbol.FamilyName;
                beamInfo.SectionDimension = beam.Symbol.Name;

                beamInfo.Material = beam.get_Parameter(BuiltInParameter.STRUCTURAL_MATERIAL_PARAM).AsValueString().Substring(6);

                beamsInfos.Add(beamInfo);
            }
            //将所有刚架梁的关键信息存储至数据库中
            for (int i = 0; i < beamsInfos.Count; i++)
            {
                string sql = "insert into BeamInfo(Id,structuralType,startX,startY,startZ,endX,endY,endZ,SectionShape,SectionDimension,Material)" +
                    "values(@Id,@structuralType,@startX,@startY,@startZ,@endX,@endY,@endZ,@SectionShape,@SectionDimension,@Material)";
                DBHelper.PrepareSql(sql);
                DBHelper.SetParameter("Id", beamsInfos[i].Id);
                DBHelper.SetParameter("structuralType", beamsInfos[i].StructuralType);
                DBHelper.SetParameter("startX", beamsInfos[i].Start.X);
                DBHelper.SetParameter("startY", beamsInfos[i].Start.Y);
                DBHelper.SetParameter("startZ", beamsInfos[i].Start.Z);
                DBHelper.SetParameter("endX", beamsInfos[i].End.X);
                DBHelper.SetParameter("endY", beamsInfos[i].End.Y);
                DBHelper.SetParameter("endZ", beamsInfos[i].End.Z);
                DBHelper.SetParameter("SectionShape", beamsInfos[i].SectionShape);
                DBHelper.SetParameter("SectionDimension", beamsInfos[i].SectionDimension);
                DBHelper.SetParameter("Material", beamsInfos[i].Material);
                DBHelper.ExecNonQuery();
            }
            #region 刚架梁Id存储至LoadInfo表中
            for (int i = 0; i < beamsInfos.Count; i++)
            {
                string sql = "insert into LoadInfo(Element_Id) " +
                    "Values(@Id)";
                DBHelper.PrepareSql(sql);
                DBHelper.SetParameter("Id", beamsInfos[i].Id);
                DBHelper.ExecNonQuery();
            }
            #endregion
            #endregion

            #region 系杆信息提取至TieInfo表中
            //提取所有系杆的关键信息
            List<TieInfo> tiesInfos = new List<TieInfo>();
            foreach (FamilyInstance tie in ties)
            {
                TieInfo tieInfo = new TieInfo();
                tieInfo.Id = tie.Id.IntegerValue;
                tieInfo.StructuralType = tie.get_Parameter(BuiltInParameter.DOOR_NUMBER).AsString();

                Autodesk.Revit.DB.XYZ origin = ((tie.Location as LocationCurve).Curve as Line).Origin;
                Autodesk.Revit.DB.XYZ direction = ((tie.Location as LocationCurve).Curve as Line).Direction;
                double length = ((tie.Location as LocationCurve).Curve as Line).Length;

                Entity.XYZ start = new Entity.XYZ();
                start.X = Math.Round(UnitExtension.ConvertToMillimeters(origin.X));
                start.Y = Math.Round(UnitExtension.ConvertToMillimeters(origin.Y));
                start.Z = Math.Round(UnitExtension.ConvertToMillimeters(origin.Z));
                tieInfo.Start = start;

                Entity.XYZ end = new Entity.XYZ();
                end.X = Math.Round(UnitExtension.ConvertToMillimeters(origin.X + length * direction.X / Math.Sqrt(direction.X * direction.X + direction.Y * direction.Y + direction.Z * direction.Z)));
                end.Y = Math.Round(UnitExtension.ConvertToMillimeters(origin.Y + length * direction.Y / Math.Sqrt(direction.X * direction.X + direction.Y * direction.Y + direction.Z * direction.Z)));
                end.Z = Math.Round(UnitExtension.ConvertToMillimeters(origin.Z + length * direction.Z / Math.Sqrt(direction.X * direction.X + direction.Y * direction.Y + direction.Z * direction.Z)));
                tieInfo.End = end;

                tieInfo.SectionShape = tie.Symbol.FamilyName;
                tieInfo.SectionDimension = tie.Symbol.Name;

                tieInfo.Material = tie.get_Parameter(BuiltInParameter.STRUCTURAL_MATERIAL_PARAM).AsValueString().Substring(6);

                tiesInfos.Add(tieInfo);
            }
            //将所有系杆的关键信息存储至数据库中
            for (int i = 0; i < tiesInfos.Count; i++)
            {
                string sql = "insert into TieInfo(Id,structuralType,startX,startY,startZ,endX,endY,endZ,SectionShape,SectionDimension,Material)" +
                     "values(@Id,@structuralType,@startX,@startY,@startZ,@endX,@endY,@endZ,@SectionShape,@SectionDimension,@Material)";
                DBHelper.PrepareSql(sql);
                DBHelper.SetParameter("Id", tiesInfos[i].Id);
                DBHelper.SetParameter("structuralType", tiesInfos[i].StructuralType);
                DBHelper.SetParameter("startX", tiesInfos[i].Start.X);
                DBHelper.SetParameter("startY", tiesInfos[i].Start.Y);
                DBHelper.SetParameter("startZ", tiesInfos[i].Start.Z);
                DBHelper.SetParameter("endX", tiesInfos[i].End.X);
                DBHelper.SetParameter("endY", tiesInfos[i].End.Y);
                DBHelper.SetParameter("endZ", tiesInfos[i].End.Z);
                DBHelper.SetParameter("SectionShape", tiesInfos[i].SectionShape);
                DBHelper.SetParameter("SectionDimension", tiesInfos[i].SectionDimension);
                DBHelper.SetParameter("Material", tiesInfos[i].Material);
                DBHelper.ExecNonQuery();
            }
            #endregion

            #region 提取支撑信息至BracingInfo表中
            //提取支撑的关键信息
            List<BracingInfo> bracingsInfos = new List<BracingInfo>();
            foreach (FamilyInstance bracing in bracings)
            {
                BracingInfo bracingInfo = new BracingInfo();
                bracingInfo.Id = bracing.Id.IntegerValue;
                bracingInfo.StructuralType = bracing.get_Parameter(BuiltInParameter.DOOR_NUMBER).AsString();

                Autodesk.Revit.DB.XYZ origin = ((bracing.Location as LocationCurve).Curve as Line).Origin;
                Autodesk.Revit.DB.XYZ direction = ((bracing.Location as LocationCurve).Curve as Line).Direction;
                double length = ((bracing.Location as LocationCurve).Curve as Line).Length;

                Entity.XYZ start = new Entity.XYZ();
                start.X = Math.Round(UnitExtension.ConvertToMillimeters(origin.X));
                start.Y = Math.Round(UnitExtension.ConvertToMillimeters(origin.Y));
                start.Z = Math.Round(UnitExtension.ConvertToMillimeters(origin.Z));
                bracingInfo.Start = start;

                Entity.XYZ end = new Entity.XYZ();
                end.X = Math.Round(UnitExtension.ConvertToMillimeters(origin.X + length * direction.X / Math.Sqrt(direction.X * direction.X + direction.Y * direction.Y + direction.Z * direction.Z)));
                end.Y = Math.Round(UnitExtension.ConvertToMillimeters(origin.Y + length * direction.Y / Math.Sqrt(direction.X * direction.X + direction.Y * direction.Y + direction.Z * direction.Z)));
                end.Z = Math.Round(UnitExtension.ConvertToMillimeters(origin.Z + length * direction.Z / Math.Sqrt(direction.X * direction.X + direction.Y * direction.Y + direction.Z * direction.Z)));
                bracingInfo.End = end;

                bracingInfo.SectionShape = bracing.Symbol.FamilyName;
                bracingInfo.SectionDimension = bracing.Symbol.Name;
                bracingInfo.Material = bracing.get_Parameter(BuiltInParameter.STRUCTURAL_MATERIAL_PARAM).AsValueString().Substring(6);

                bracingsInfos.Add(bracingInfo);
            }
            //将所有支撑的关键信息存储至数据库中
            for (int i = 0; i < bracingsInfos.Count; i++)
            {
                string sql = "insert into BracingInfo(Id,structuralType,startX,startY,startZ,endX,endY,endZ,SectionShape,SectionDimension,Material)" +
                            "values(@Id,@structuralType,@startX,@startY,@startZ,@endX,@endY,@endZ,@SectionShape,@SectionDimension,@Material)";
                DBHelper.PrepareSql(sql);
                DBHelper.SetParameter("Id", bracingsInfos[i].Id);
                DBHelper.SetParameter("structuralType", bracingsInfos[i].StructuralType);
                DBHelper.SetParameter("startX", bracingsInfos[i].Start.X);
                DBHelper.SetParameter("startY", bracingsInfos[i].Start.Y);
                DBHelper.SetParameter("startZ", bracingsInfos[i].Start.Z);
                DBHelper.SetParameter("endX", bracingsInfos[i].End.X);
                DBHelper.SetParameter("endY", bracingsInfos[i].End.Y);
                DBHelper.SetParameter("endZ", bracingsInfos[i].End.Z);
                DBHelper.SetParameter("SectionShape", bracingsInfos[i].SectionShape);
                DBHelper.SetParameter("SectionDimension", bracingsInfos[i].SectionDimension);
                DBHelper.SetParameter("Material", bracingsInfos[i].Material);
                DBHelper.ExecNonQuery();
            }
            #endregion



            #endregion

            #region 提取荷载信息至数据库
            #region 恒、活、风
            FilteredElementCollector lineLoadFilter = new FilteredElementCollector(doc).OfClass(typeof(LineLoad));
            List<LineLoad> lineLoads=lineLoadFilter.Cast<LineLoad>().ToList();
            Dictionary<int, List<LineLoad>> dic = new Dictionary<int, List<LineLoad>>();
            for (int i = 0; i < lineLoads.Count; i++)
            {
                string desc=lineLoads[i].get_Parameter(BuiltInParameter.LOAD_DESCRIPTION).AsString().Substring(9);
                int elementId = Convert.ToInt32(desc);
                if (dic.ContainsKey(elementId))
                {
                    dic[elementId].Add(lineLoads[i]);                                 
                }
                else
                {
                    dic.Add(elementId, new List<LineLoad>());
                    dic[elementId].Add(lineLoads[i]);
                }
            }
            foreach (KeyValuePair<int,List<LineLoad>> pair in dic)
            {
                LoadInfo loadInfo=new LoadInfo();
                loadInfo.ElementId = pair.Key;
                string dead_X = null;
                string dead_Y = null;
                string dead_Z = null;

                string live_X = null;
                string live_Y = null;
                string live_Z = null;

                string wx0_0_X = null;
                string wx0_0_Y = null;
                string wx0_0_Z = null;

                string wx0_1_X = null;
                string wx0_1_Y = null;
                string wx0_1_Z = null;

                string wx1_0_X = null;
                string wx1_0_Y = null;
                string wx1_0_Z = null;

                string wx1_1_X = null;
                string wx1_1_Y = null;
                string wx1_1_Z = null;

                string wy0_0_X = null;
                string wy0_0_Y = null;
                string wy0_0_Z = null;

                string wy0_1_X = null;
                string wy0_1_Y = null;
                string wy0_1_Z = null;

                string wy1_0_X = null;
                string wy1_0_Y = null;
                string wy1_0_Z = null;

                string wy1_1_X = null;
                string wy1_1_Y = null;
                string wy1_1_Z = null;
                for (int j = 0; j < pair.Value.Count; j++)
                {
                   
                    if (pair.Value[j].LoadCase.Name == "Dead")
                    {
                        string scope = pair.Value[j].get_Parameter(BuiltInParameter.LOAD_COMMENTS).AsString().Substring(6);
                        string x = pair.Value[j].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).AsValueString();
                        dead_X = string.Concat(dead_X, scope, ":", x,";");
                        string y = pair.Value[j].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).AsValueString();
                        dead_Y = string.Concat(dead_Y, scope, ":", y,";");
                        string z = pair.Value[j].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).AsValueString();
                        dead_Z = string.Concat(dead_Z, scope, ":", z, ";");

                    }
                    else if (pair.Value[j].LoadCase.Name == "Live")
                    {
                        string scope = pair.Value[j].get_Parameter(BuiltInParameter.LOAD_COMMENTS).AsString().Substring(6);
                        string x = pair.Value[j].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).AsValueString();
                        live_X = string.Concat(live_X, scope, ":", x, ";");
                        string y = pair.Value[j].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).AsValueString();
                        live_Y = string.Concat(live_Y, scope, ":", y, ";");
                        string z = pair.Value[j].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).AsValueString();
                        live_Z = string.Concat(live_Z, scope, ":", z, ";");
                    }
                    else if (pair.Value[j].LoadCase.Name == "WX+(+i)")
                    {
                        string scope = pair.Value[j].get_Parameter(BuiltInParameter.LOAD_COMMENTS).AsString().Substring(6);
                        string x = pair.Value[j].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).AsValueString();
                        wx0_0_X = string.Concat(wx0_0_X, scope, ":", x, ";");
                        string y = pair.Value[j].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).AsValueString();
                        wx0_0_Y = string.Concat(wx0_0_Y, scope, ":", y,";");
                        string z = pair.Value[j].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).AsValueString();
                        wx0_0_Z = string.Concat(wx0_0_Z, scope, ":", z,";");
                    }
                    else if (pair.Value[j].LoadCase.Name == "WX+(-i)")
                    {
                        string scope = pair.Value[j].get_Parameter(BuiltInParameter.LOAD_COMMENTS).AsString().Substring(6);
                        string x = pair.Value[j].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).AsValueString();
                        wx0_1_X = string.Concat(wx0_1_X, scope, ":", x, ";");
                        string y = pair.Value[j].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).AsValueString();
                        wx0_1_Y = string.Concat(wx0_1_Y, scope, ":", y, ";");
                        string z = pair.Value[j].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).AsValueString();
                        wx0_1_Z = string.Concat(wx0_1_Z, scope, ":", z, ";");
                    }
                    else if (pair.Value[j].LoadCase.Name == "WX-(+i)")
                    {
                        string scope = pair.Value[j].get_Parameter(BuiltInParameter.LOAD_COMMENTS).AsString().Substring(6);
                        string x = pair.Value[j].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).AsValueString();
                        wx1_0_X = string.Concat(wx1_0_X, scope, ":", x, ";");
                        string y = pair.Value[j].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).AsValueString();
                        wx1_0_Y = string.Concat(wx1_0_Y, scope, ":", y, ";");
                        string z = pair.Value[j].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).AsValueString();
                        wx1_0_Z = string.Concat(wx1_0_Z, scope, ":", z, ";");
                    }
                    else if (pair.Value[j].LoadCase.Name == "WX-(-i)")
                    {
                        string scope = pair.Value[j].get_Parameter(BuiltInParameter.LOAD_COMMENTS).AsString().Substring(6);
                        string x = pair.Value[j].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).AsValueString();
                        wx1_1_X = string.Concat(wx1_1_X, scope, ":", x, ";");
                        string y = pair.Value[j].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).AsValueString();
                        wx1_1_Y = string.Concat(wx1_1_Y, scope, ":", y, ";");
                        string z = pair.Value[j].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).AsValueString();
                        wx1_1_Z = string.Concat(wx1_1_Z, scope, ":", z, ";");
                    }
                    else if (pair.Value[j].LoadCase.Name == "WY+(+i)")
                    {
                        string scope = pair.Value[j].get_Parameter(BuiltInParameter.LOAD_COMMENTS).AsString().Substring(6);
                        string x = pair.Value[j].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).AsValueString();
                        wy0_0_X = string.Concat(wy0_0_X, scope, ":", x, ";");
                        string y = pair.Value[j].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).AsValueString();
                        wy0_0_Y = string.Concat(wy0_0_Y, scope, ":", y, ";");
                        string z = pair.Value[j].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).AsValueString();
                        wy0_0_Z = string.Concat(wy0_0_Z, scope, ":", z, ";");
                    }
                    else if (pair.Value[j].LoadCase.Name == "WY+(-i)")
                    {
                        string scope = pair.Value[j].get_Parameter(BuiltInParameter.LOAD_COMMENTS).AsString().Substring(6);
                        string x = pair.Value[j].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).AsValueString();
                        wy0_1_X = string.Concat(wy0_1_X, scope, ":", x, ";");
                        string y = pair.Value[j].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).AsValueString();
                        wy0_1_Y = string.Concat(wy0_1_Y, scope, ":", y, ";");
                        string z = pair.Value[j].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).AsValueString();
                        wy0_1_Z = string.Concat(wy0_1_Z, scope, ":", z, ";");
                    }
                    else if (pair.Value[j].LoadCase.Name == "WY-(+i)")
                    {
                        string scope = pair.Value[j].get_Parameter(BuiltInParameter.LOAD_COMMENTS).AsString().Substring(6);
                        string x = pair.Value[j].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).AsValueString();
                        wy1_0_X = string.Concat(wy1_0_X, scope, ":", x,";");
                        string y = pair.Value[j].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).AsValueString();
                        wy1_0_Y = string.Concat(wy1_0_Y, scope, ":", y, ";");
                        string z = pair.Value[j].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).AsValueString();
                        wy1_0_Z = string.Concat(wy1_0_Z, scope, ":", z, ";");
                    }
                    else if (pair.Value[j].LoadCase.Name == "WY-(-i)")
                    {
                        string scope = pair.Value[j].get_Parameter(BuiltInParameter.LOAD_COMMENTS).AsString().Substring(6);
                        string x = pair.Value[j].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FX1).AsValueString();
                        wy1_1_X = string.Concat(wy1_1_X, scope, ":", x, ";");
                        string y = pair.Value[j].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FY1).AsValueString();
                        wy1_1_Y = string.Concat(wy1_1_Y, scope, ":", y, ";");
                        string z = pair.Value[j].get_Parameter(BuiltInParameter.LOAD_LINEAR_FORCE_FZ1).AsValueString();
                        wy1_1_Z = string.Concat(wy1_1_Z, scope, ":", z, ";");
                    }
                    else
                    {
                        TextDialog warningTextDialog = new TextDialog("数据传输失败！存在无法识别数据类型！");
                        warningTextDialog.ShowDialog();
                    }
                    loadInfo.Dead_X = dead_X;
                    loadInfo.Dead_Y = dead_Y;
                    loadInfo.Dead_Z = dead_Z;
                    loadInfo.Live_X=live_X;
                    loadInfo.Live_Y=live_Y;
                    loadInfo.Live_Z=live_Z;
                    loadInfo.WX0_0_X=wx0_0_X ;
                    loadInfo.WX0_0_Y=wx0_0_Y ;
                    loadInfo.WX0_0_Z=wx0_0_Z ;
                    loadInfo.WX0_1_X = wx0_1_X;
                    loadInfo.WX0_1_Y = wx0_1_Y;
                    loadInfo.WX0_1_Z = wx0_1_Z;
                    loadInfo.WX1_0_X = wx1_0_X;
                    loadInfo.WX1_0_Y = wx1_0_Y;
                    loadInfo.WX1_0_Z = wx1_0_Z;
                    loadInfo.WX1_1_X = wx1_1_X;
                    loadInfo.WX1_1_Y = wx1_1_Y;
                    loadInfo.WX1_1_Z = wx1_1_Z;

                    loadInfo.WY0_0_X = wy0_0_X;
                    loadInfo.WY0_0_Y = wy0_0_Y;
                    loadInfo.WY0_0_Z = wy0_0_Z;
                    loadInfo.WY0_1_X = wy0_1_X;
                    loadInfo.WY0_1_Y = wy0_1_Y;
                    loadInfo.WY0_1_Z = wy0_1_Z;
                    loadInfo.WY1_0_X = wy1_0_X;
                    loadInfo.WY1_0_Y = wy1_0_Y;
                    loadInfo.WY1_0_Z = wy1_0_Z;
                    loadInfo.WY1_1_X = wy1_1_X;
                    loadInfo.WY1_1_Y = wy1_1_Y;
                    loadInfo.WY1_1_Z = wy1_1_Z;
                }
                //string sql = "update LoadInfo set Dead_Z=@Dead_Z,Live_Z=@Live_Z where Element_Id=@ElementId";
                //DBHelper.PrepareSql(sql);
                //DBHelper.SetParameter("Dead_Z", loadInfo.Dead_Z);
                //DBHelper.SetParameter("Live_Z", loadInfo.Live_Z);
                //DBHelper.SetParameter("ElementId", loadInfo.ElementId);
                //DBHelper.ExecNonQuery();
                string sql = "update LoadInfo set " +
                            "Dead_x=@Dead_X,Dead_y=@Dead_Y,Dead_z=@Dead_Z," +
                            "Live_x=@Live_X,Live_y=@Live_Y,Live_z=@Live_Z," +
                            "WX0_0_x=@WX0_0_X,WX0_0_y=@WX0_0_Y,WX0_0_z=@WX0_0_Z," +
                            "WX0_1_x=@WX0_1_X,WX0_1_y=@WX0_1_Y,WX0_1_z=@WX0_1_Z," +
                            "WX1_0_x=@WX1_0_X,WX1_0_y=@WX1_0_Y,WX1_0_z=@WX1_0_Z," +
                            "WX1_1_x=@WX1_1_X,WX1_1_y=@WX1_1_Y,WX1_1_z=@WX1_1_Z," +
                            "WY0_0_x=@WY0_0_X,WY0_0_y=@WY0_0_Y,WY0_0_z=@WY0_0_Z," +
                            "WY0_1_x=@WY0_1_X,WY0_1_y=@WY0_1_Y,WY0_1_z=@WY0_1_Z," +
                            "WY1_0_x=@WY1_0_X,WY1_0_y=@WY1_0_Y,WY1_0_z=@WY1_0_Z," +
                            "WY1_1_x=@WY1_1_X,WY1_1_y=@WY1_1_Y,WY1_1_z=@WY1_1_Z " +
                            "where Element_Id=@ElementId";
                DBHelper.PrepareSql(sql);
                DBHelper.SetParameter("Dead_X", loadInfo.Dead_X);
                DBHelper.SetParameter("Dead_Y", loadInfo.Dead_Y);
                DBHelper.SetParameter("Dead_Z", loadInfo.Dead_Z);

                DBHelper.SetParameter("Live_X", loadInfo.Live_X);
                DBHelper.SetParameter("Live_Y", loadInfo.Live_Y);
                DBHelper.SetParameter("Live_Z", loadInfo.Live_Z);

                DBHelper.SetParameter("WX0_0_X", loadInfo.WX0_0_X);
                DBHelper.SetParameter("WX0_0_Y", loadInfo.WX0_0_Y);
                DBHelper.SetParameter("WX0_0_Z", loadInfo.WX0_0_Z);

                DBHelper.SetParameter("WX0_1_X", loadInfo.WX0_1_X);
                DBHelper.SetParameter("WX0_1_Y", loadInfo.WX0_1_Y);
                DBHelper.SetParameter("WX0_1_Z", loadInfo.WX0_1_Z);

                DBHelper.SetParameter("WX1_0_X", loadInfo.WX1_0_X);
                DBHelper.SetParameter("WX1_0_Y", loadInfo.WX1_0_Y);
                DBHelper.SetParameter("WX1_0_Z", loadInfo.WX1_0_Z);

                DBHelper.SetParameter("WX1_1_X", loadInfo.WX1_1_X);
                DBHelper.SetParameter("WX1_1_Y", loadInfo.WX1_1_Y);
                DBHelper.SetParameter("WX1_1_Z", loadInfo.WX1_1_Z);

                DBHelper.SetParameter("WY0_0_X", loadInfo.WY0_0_X);
                DBHelper.SetParameter("WY0_0_Y", loadInfo.WY0_0_Y);
                DBHelper.SetParameter("WY0_0_Z", loadInfo.WY0_0_Z);

                DBHelper.SetParameter("WY0_1_X", loadInfo.WY0_1_X);
                DBHelper.SetParameter("WY0_1_Y", loadInfo.WY0_1_Y);
                DBHelper.SetParameter("WY0_1_Z", loadInfo.WY0_1_Z);

                DBHelper.SetParameter("WY1_0_X", loadInfo.WY1_0_X);
                DBHelper.SetParameter("WY1_0_Y", loadInfo.WY1_0_Y);
                DBHelper.SetParameter("WY1_0_Z", loadInfo.WY1_0_Z);

                DBHelper.SetParameter("WY1_1_X", loadInfo.WY1_1_X);
                DBHelper.SetParameter("WY1_1_Y", loadInfo.WY1_1_Y);
                DBHelper.SetParameter("WY1_1_Z", loadInfo.WY1_1_Z);
                DBHelper.SetParameter("ElementId", loadInfo.ElementId);
                DBHelper.ExecNonQuery();
            }
            #endregion

            #region 地震
            string sqlForEarthInfo = "Insert into EarthquakeInfo(SeismicIntensity,AlphaMax,SiteCategory,ClassificationOfDesignEarthquake,"
                                    + "CharacteristicPeriod,PeriodTimeReductionFactor,DampingRatio,ModesCombination)"
                                    + "values(@SeismicIntensity,@AlphaMax,@SiteCategory,@ClassificationOfDesignEarthquake,"
                                    + "@CharacteristicPeriod,@PeriodTimeReductionFactor,@DampingRatio,@ModesCombination)";
            EarthquakeInfo earthquakeInfo = new EarthquakeInfo();
            earthquakeInfo.SeismicIntensity = LoadSetView.GetInstance().SeismicIntensity.Text;
            earthquakeInfo.AlphaMax = Convert.ToDouble(LoadSetView.GetInstance().MaximumValueOfEarthquakeInfluenceCoefficient.Text);
            earthquakeInfo.SiteCategory = LoadSetView.GetInstance().SiteCategory.Text;
            earthquakeInfo.ClassificationOfDesignEarthquake = LoadSetView.GetInstance().ClassificationOfDesignEarthquake.Text;
            earthquakeInfo.CharacteristicPeriod = Convert.ToDouble(LoadSetView.GetInstance().CharacteristicPeriod.Text);
            earthquakeInfo.PeriodTimeReductionFactor =Convert.ToDouble(LoadSetView.GetInstance().PeriodTimeReductionFactor.Text);
            earthquakeInfo.DampingRatio = Convert.ToDouble(LoadSetView.GetInstance().DampingRatio.Text);
            earthquakeInfo.ModesCombination = LoadSetView.GetInstance().ModesCombination.Text;
            DBHelper.PrepareSql(sqlForEarthInfo);
            DBHelper.SetParameter("SeismicIntensity", earthquakeInfo.SeismicIntensity);
            DBHelper.SetParameter("AlphaMax", earthquakeInfo.AlphaMax);
            DBHelper.SetParameter("SiteCategory", earthquakeInfo.SiteCategory);
            DBHelper.SetParameter("ClassificationOfDesignEarthquake", earthquakeInfo.ClassificationOfDesignEarthquake);
            DBHelper.SetParameter("CharacteristicPeriod", earthquakeInfo.CharacteristicPeriod);
            DBHelper.SetParameter("PeriodTimeReductionFactor", earthquakeInfo.PeriodTimeReductionFactor);
            DBHelper.SetParameter("DampingRatio", earthquakeInfo.DampingRatio);
            DBHelper.SetParameter("ModesCombination",earthquakeInfo.ModesCombination);
            DBHelper.ExecNonQuery();
            #endregion
            #endregion

            #region 提取荷载组合至数据库
            FilteredElementCollector LoadCombinationFilter = new FilteredElementCollector(doc).OfClass(typeof(LoadCombination));
            List<LoadCombination> loadCombinations= LoadCombinationFilter.Cast<LoadCombination>().ToList();
            for (int i = 0; i < loadCombinations.Count; i++)
            {
                LoadCombinationInfo loadCombinationInfo = new LoadCombinationInfo();
                loadCombinationInfo.Tag = LoadCombinationView.GetInstance().ResultOfCombination.Items[i].ToString();
                IList<LoadComponent> components = loadCombinations[i].GetComponents();
                for (int j = 0; j < components.Count; j++)
                {
                    string loadCaseName = doc.GetElement(components[j].LoadCaseOrCombinationId).Name;
                    double factor = components[j].Factor;
                    switch (loadCaseName)
                    {
                        case "Dead":loadCombinationInfo.Dead = factor; break;
                        case "Live":loadCombinationInfo.Live = factor; break;
                        case "WX+(+i)":loadCombinationInfo.WX0_0 = factor; break;
                        case "WX+(-i)":loadCombinationInfo.WX0_1 = factor; break;
                        case "WX-(+i)":loadCombinationInfo.WX1_0 = factor; break;
                        case "WX-(-i)":loadCombinationInfo.WX1_1 = factor; break;
                        case "WY+(+i)": loadCombinationInfo.WY0_0 = factor; break;
                        case "WY+(-i)": loadCombinationInfo.WY0_1 = factor; break;
                        case "WY-(+i)": loadCombinationInfo.WY1_0 = factor; break;
                        case "WY-(-i)": loadCombinationInfo.WY1_1 = factor; break;
                        case "EX": loadCombinationInfo.EX = factor; break;
                        case "EY": loadCombinationInfo.EY = factor; break;
                        default:
                            break;
                    }
                }
                string sql = "insert into loadCombinationInfo(Tag,Dead,Live,WX0_0,WX0_1,WX1_0,WX1_1,WY0_0,WY0_1,WY1_0,WY1_1,EX,EY)" +
                           "values(@Tag,@Dead,@Live,@WX0_0,@WX0_1,@WX1_0,@WX1_1,@WY0_0,@WY0_1,@WY1_0,@WY1_1,@EX,@EY)";
                DBHelper.PrepareSql(sql);
                DBHelper.SetParameter("Tag", loadCombinationInfo.Tag);
                DBHelper.SetParameter("Dead", loadCombinationInfo.Dead);
                DBHelper.SetParameter("Live", loadCombinationInfo.Live);
                DBHelper.SetParameter("WX0_0", loadCombinationInfo.WX0_0);
                DBHelper.SetParameter("WX0_1", loadCombinationInfo.WX0_1);
                DBHelper.SetParameter("WX1_0", loadCombinationInfo.WX1_0);
                DBHelper.SetParameter("WX1_1", loadCombinationInfo.WX1_1);
                DBHelper.SetParameter("WY0_0", loadCombinationInfo.WY0_0);
                DBHelper.SetParameter("WY0_1", loadCombinationInfo.WY0_1);
                DBHelper.SetParameter("WY1_0", loadCombinationInfo.WY1_0);
                DBHelper.SetParameter("WY1_1", loadCombinationInfo.WY1_1);
                DBHelper.SetParameter("EX", loadCombinationInfo.EX);
                DBHelper.SetParameter("EY", loadCombinationInfo.EY);
                DBHelper.ExecNonQuery();
            }
            #endregion



            TextDialog textDialog = new TextDialog("数据传输完成");
            textDialog.ShowDialog();
            
            ShowData("ColumnInfo");
        }
        public void ShowData(string typeName)
        {
            string sql = "select * from "+typeName;
            DBHelper.PrepareSql(sql);
            DataTable dt= DBHelper.ExecQuery();
            Data.DataContext = dt;
        }

        private void Type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            switch (this.Type.SelectedItem.ToString())
            {
                case "刚架柱":ShowData("ColumnInfo");break;
                case "刚架梁":ShowData("BeamInfo");break;
                case "系杆":ShowData("TieInfo");break;
                case "支撑":ShowData("BracingInfo");break;
                case "抗风柱":ShowData("WindResistantColumnInfo");break;
                default:break;
            }
        }
    }
}
