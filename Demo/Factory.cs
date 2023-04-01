using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Demo.Model;
using System;
using System.Collections.Generic;

namespace Demo
{
    [Transaction(TransactionMode.Manual)]
    public class Factory : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            Windows dialog = new Windows(doc);
            dialog.ShowDialog();
            //try
            //{
            double bentNumber = Convert.ToDouble(dialog.BentNumber.Text);
            double depth = Convert.ToDouble(dialog.NetDepth.Text) / 304.8;
            double length = Convert.ToDouble(dialog.Length.Text) / 304.8;
            double colHeight = Convert.ToDouble(dialog.ColHeight.Text) / 304.8;
            double roofHeight = Convert.ToDouble(dialog.RfHeight.Text) / 304.8;
            //.....
            double purlinNumberL = Convert.ToDouble(dialog.PurlinNumberL.Text);
            double purlinNumberR = Convert.ToDouble(dialog.PurlinNumberR.Text);
            double purlinSpaceL = Convert.ToDouble(dialog.PurlinSpaceL.Text) / 304.8;
            double purlinSpaceR = Convert.ToDouble(dialog.PurlinSpaceR.Text) / 304.8;
            double purlinRR = Convert.ToDouble(dialog.PurlinRR.Text) / 304.8;
            double purlinCR = Convert.ToDouble(dialog.PurlinCR.Text) / 304.8;
            double purlinCL = Convert.ToDouble(dialog.PurlinCL.Text) / 304.8;
            double purlinRL = Convert.ToDouble(dialog.PurlinRL.Text) / 304.8;
            //拿到柱所在位置
            List<double> spaceList = new List<double>();
            if (dialog.IsUniform.IsChecked == true)
            {
                double space = Convert.ToInt32(dialog.Space.Text) / 304.8;
                for (int i = 0; i < bentNumber; i++)
                {
                    spaceList.Add(i * space);
                }
            }
            else
            {
                double sum = 0;
                spaceList.Add(0);
                foreach (SpaceInfo item in dialog.SpaceList.Items)
                {
                    sum += item.Space;
                    spaceList.Add(sum / 304.8);
                }
            }
            List<XYZ> pColL = new List<XYZ>();
            List<XYZ> pColR = new List<XYZ>();
            for (int i = 0; i < bentNumber; i++)
            {
                pColR.Add(new XYZ(spaceList[i], 0, 0));
                pColL.Add(new XYZ(spaceList[i], depth, 0));
            }
            //拿到柱底标高
            FilteredElementCollector lvlFilter = new FilteredElementCollector(doc).OfClass(typeof(Level));
            IList<Element> lvlList = lvlFilter.ToElements();
            Level ColBtmlevel = null;
            foreach (Level item in lvlList)
            {
                if (item.Elevation == 0)
                {
                    ColBtmlevel = item;
                }
            }

            //拿到柱的族类型
            FilteredElementCollector colFilter = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_StructuralColumns);
            IList<Element> colTypeList = colFilter.ToElements();
            FamilySymbol colType = null;
            foreach (FamilySymbol item in colTypeList)
            {
                if (item.Name == dialog.ColSelection.SelectedItem.ToString())
                {
                    colType = item;
                    break;
                }
            }
            //拿到抗风柱的族类型
            FilteredElementCollector windResistantColFilter = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_StructuralColumns);
            IList<Element> windResistantColTypeList = windResistantColFilter.ToElements();
            FamilySymbol windResistantColType = null;
            foreach (FamilySymbol item in windResistantColTypeList)
            {
                if (item.Name == dialog.WindResistantColSelection.SelectedItem.ToString())
                {
                    windResistantColType = item;
                    break;
                }
            }
            //拿到梁的族类型
            FilteredElementCollector beamFilter = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_StructuralFraming);
            IList<Element> beamTypeList = beamFilter.ToElements();
            FamilySymbol beamType = null;
            foreach (FamilySymbol item in beamTypeList)
            {
                if (item.Name == dialog.BeamSelection.SelectedItem.ToString())
                {
                    beamType = item;
                    break;
                }
            }
            //拿到屋脊梁的族类型
            FilteredElementCollector roofBeamFilter = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_StructuralFraming);
            IList<Element> roofBeamTypeList = beamFilter.ToElements();
            FamilySymbol roofBeamType = null;
            foreach (FamilySymbol item in roofBeamTypeList)
            {
                if (item.Name == dialog.RfBeamSelection.SelectedItem.ToString())
                {
                    roofBeamType = item;
                    break;
                }
            }
            //拿到屋架的族类型
            FilteredElementCollector roofFilter = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_StructuralFraming);
            IList<Element> roofTypeList = roofFilter.ToElements();
            FamilySymbol roofType = null;
            foreach (FamilySymbol item in roofTypeList)
            {
                if (item.Name == dialog.RfSelection.SelectedItem.ToString())
                {
                    roofType = item;
                    break;
                }
            }
            //拿到屋架顶点
            List<XYZ> pRoof = new List<XYZ>();
            for (int i = 0; i < bentNumber; i++)
            {
                pRoof.Add(new XYZ(spaceList[i], depth / 2, colHeight + roofHeight));
            }
            //拿到柱顶点
            List<XYZ> pColTopR = new List<XYZ>();
            List<XYZ> pColTopL = new List<XYZ>();
            foreach (var item in pColR)
            {
                pColTopR.Add(item + new XYZ(0, 0, colHeight));
            }
            foreach (var item in pColL)
            {
                pColTopL.Add(item + new XYZ(0, 0, colHeight));
            }
            //拿到檩条的族类型
            FilteredElementCollector purlinFilter = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_StructuralFraming);
            IList<Element> purlinTypeList = roofFilter.ToElements();
            FamilySymbol purlinType = null;
            foreach (FamilySymbol item in purlinTypeList)
            {
                if (item.Name == dialog.PurlinSelection.SelectedItem.ToString())
                {
                    purlinType = item;
                    break;
                }
            }
            //拿到柱间支撑的族类型
            FilteredElementCollector colBracingFilter = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_StructuralFraming);
            IList<Element> colBracingTypeList = colBracingFilter.ToElements();
            FamilySymbol colBracingType = null;
            foreach (FamilySymbol item in colBracingTypeList)
            {
                if (item.Name == dialog.ColBracingSelection.SelectedItem.ToString())
                {
                    colBracingType = item;
                    break;
                }
            }
            //拿到屋面支撑的族类型
            FilteredElementCollector roofBracingFilter = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_StructuralFraming);
            IList<Element> roofBracingTypeList = roofBracingFilter.ToElements();
            FamilySymbol roofBracingType = null;
            foreach (FamilySymbol item in roofBracingTypeList)
            {
                if (item.Name == dialog.RofBracingSelection.SelectedItem.ToString())
                {
                    roofBracingType = item;
                    break;
                }
            }
            //拿到材质
            FilteredElementCollector materialFilter = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Materials);
            IList<Element> materialList = materialFilter.ToElements();
            Material colMaterial = null;
            Material windResistantColMaterial = null;
            Material beamMaterial = null;
            Material roofMaterial = null;
            Material roofBeamMaterial = null;
            Material purlinMaterial = null;
            Material colBracingMaterial = null;
            Material roofBracingMaterial = null;
            foreach (Material item in materialList)
            {
                if (item.Name == dialog.ColMaterial.SelectedItem.ToString())
                {
                    colMaterial = item;
                }
                if (item.Name == dialog.BeamMaterial.SelectedItem.ToString())
                {
                    beamMaterial = item;
                }
                if (item.Name == dialog.RfMaterial.SelectedItem.ToString())
                {
                    roofMaterial = item;
                }
                if (item.Name == dialog.RfBeamMaterial.SelectedItem.ToString())
                {
                    roofBeamMaterial = item;
                }
                if (item.Name == dialog.PurlinMaterial.SelectedItem.ToString())
                {
                    purlinMaterial = item;
                }
                if (item.Name == dialog.ColBracingMaterial.SelectedItem.ToString())
                {
                    colBracingMaterial = item;
                }
                if (item.Name == dialog.RofBracingMaterial.SelectedItem.ToString())
                {
                    roofBracingMaterial = item;
                }
                if (item.Name == dialog.WindResistantColMaterial.SelectedItem.ToString())
                {
                    windResistantColMaterial = item;
                }
            }
            //拿到边界条件
            //FilteredElementCollector bcFilter = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_BoundaryConditions);
            //IList<Element> bcList = bcFilter.ToElements();
            IList<FamilyInstance> allColList = new List<FamilyInstance>();
            List<FamilyInstance> windResistantColList = new List<FamilyInstance>();
            if (dialog.DialogResult.Value)
            {
                using (Transaction ts1 = new Transaction(doc, "FactoryCreation"))
                {
                    ts1.Start();
                    //创建柱顶标高及相应平面
                    Level ColToplevel = Level.Create(doc, colHeight);
                    FilteredElementCollector viewPlanFilter = new FilteredElementCollector(doc).OfClass(typeof(ViewFamilyType));
                    foreach (ViewFamilyType viewFamilyType in viewPlanFilter)
                    {
                        if (viewFamilyType.ViewFamily == ViewFamily.StructuralPlan)
                        {
                            ViewPlan.Create(doc, viewFamilyType.Id, ColToplevel.Id);
                        }
                    }
                    //生成柱

                    if (!colType.IsActive)
                    {
                        colType.Activate();
                    }
                    foreach (XYZ item in pColL)
                    {
                        FamilyInstance col = doc.Create.NewFamilyInstance(item, colType, ColBtmlevel, Autodesk.Revit.DB.Structure.StructuralType.Column);
                        col.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM).Set(ColToplevel.Id);
                        col.StructuralMaterialId = colMaterial.Id;
                        //col.get_Parameter(BuiltInParameter.SCHEDULE_TOP_LEVEL_OFFSET_PARAM).Set();
                        allColList.Add(col);
                    }
                    foreach (XYZ item in pColR)
                    {
                        FamilyInstance col = doc.Create.NewFamilyInstance(item, colType, ColBtmlevel, Autodesk.Revit.DB.Structure.StructuralType.Column);
                        col.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM).Set(ColToplevel.Id);
                        col.StructuralMaterialId = colMaterial.Id;
                        //col.get_Parameter(BuiltInParameter.SCHEDULE_TOP_LEVEL_OFFSET_PARAM).Set(colHeight/304.8);
                        allColList.Add(col);
                    }
                    //生成边跨抗风柱
                    if (dialog.IsSetWindResistantCol.IsChecked == true)
                    {
                        
                        int windResistantColNumber = Convert.ToInt32(dialog.WindResistantColNumber.Text);
                        if (!windResistantColType.IsActive)
                        {
                            windResistantColType.Activate();
                        }
                        if (windResistantColNumber % 2 == 1)
                        {
                            FamilyInstance middleWindResistantColWest = doc.Create.NewFamilyInstance(new XYZ(0, depth / 2, 0), windResistantColType, ColBtmlevel, Autodesk.Revit.DB.Structure.StructuralType.Column);
                            middleWindResistantColWest.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM).Set(ColToplevel.Id);
                            middleWindResistantColWest.get_Parameter(BuiltInParameter.SCHEDULE_TOP_LEVEL_OFFSET_PARAM).Set(roofHeight);
                            middleWindResistantColWest.StructuralMaterialId = windResistantColMaterial.Id;
                            allColList.Add(middleWindResistantColWest);
                            windResistantColList.Add(middleWindResistantColWest);
                            FamilyInstance middleWindResistantColEast = doc.Create.NewFamilyInstance(new XYZ(length, depth / 2, 0), windResistantColType, ColBtmlevel, Autodesk.Revit.DB.Structure.StructuralType.Column);
                            middleWindResistantColEast.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM).Set(ColToplevel.Id);
                            middleWindResistantColEast.get_Parameter(BuiltInParameter.SCHEDULE_TOP_LEVEL_OFFSET_PARAM).Set(roofHeight);
                            middleWindResistantColEast.StructuralMaterialId = windResistantColMaterial.Id;
                            allColList.Add(middleWindResistantColEast);
                            windResistantColList.Add(middleWindResistantColEast);

                        }
                        for (int i = 1; i <= windResistantColNumber / 2; i++)
                        {
                            //生成西侧右边抗风柱
                            XYZ windResistantColPointWestR = new XYZ(0, depth / 2 / (windResistantColNumber / 2 + 1) * i, 0);
                            FamilyInstance windResistantColWestR = doc.Create.NewFamilyInstance(windResistantColPointWestR, windResistantColType, ColBtmlevel, Autodesk.Revit.DB.Structure.StructuralType.Column);
                            windResistantColWestR.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM).Set(ColToplevel.Id);
                            windResistantColWestR.get_Parameter(BuiltInParameter.SCHEDULE_TOP_LEVEL_OFFSET_PARAM).Set(roofHeight / (windResistantColNumber / 2 + 1) * i);
                            windResistantColWestR.StructuralMaterialId = windResistantColMaterial.Id;                          
                            allColList.Add(windResistantColWestR);
                            windResistantColList.Add(windResistantColWestR);

                            //生成西侧左边抗风柱
                            XYZ windResistantColPointWestL = new XYZ(0, depth / 2 + depth / 2 / (windResistantColNumber / 2 + 1) * i, 0);
                            FamilyInstance windResistantColWestL = doc.Create.NewFamilyInstance(windResistantColPointWestL, windResistantColType, ColBtmlevel, Autodesk.Revit.DB.Structure.StructuralType.Column);
                            windResistantColWestL.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM).Set(ColToplevel.Id);
                            windResistantColWestL.get_Parameter(BuiltInParameter.SCHEDULE_TOP_LEVEL_OFFSET_PARAM).Set(roofHeight / (windResistantColNumber / 2 + 1) *(windResistantColNumber/2+1 - i));
                            windResistantColWestL.StructuralMaterialId = windResistantColMaterial.Id;
                            allColList.Add(windResistantColWestL);
                            windResistantColList.Add(windResistantColWestL);

                            //生成东侧右边抗风柱
                            XYZ windResistantColPointEastR = new XYZ(length, depth / 2 / (windResistantColNumber / 2 + 1) * i, 0);
                            FamilyInstance windResistantColEastR = doc.Create.NewFamilyInstance(windResistantColPointEastR, windResistantColType, ColBtmlevel, Autodesk.Revit.DB.Structure.StructuralType.Column);
                            windResistantColEastR.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM).Set(ColToplevel.Id);
                            windResistantColEastR.get_Parameter(BuiltInParameter.SCHEDULE_TOP_LEVEL_OFFSET_PARAM).Set(roofHeight / (windResistantColNumber / 2 + 1) * i);
                            windResistantColEastR.StructuralMaterialId = windResistantColMaterial.Id;
                            allColList.Add(windResistantColEastR);
                            windResistantColList.Add(windResistantColEastR);

                            //生成东侧左边抗风柱
                            XYZ windResistantColPointEastL = new XYZ(length, depth / 2 + depth / 2 / (windResistantColNumber / 2 + 1) * i, 0);
                            FamilyInstance windResistantColEastL = doc.Create.NewFamilyInstance(windResistantColPointEastL, windResistantColType, ColBtmlevel, Autodesk.Revit.DB.Structure.StructuralType.Column);
                            windResistantColEastL.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM).Set(ColToplevel.Id);
                            windResistantColEastL.get_Parameter(BuiltInParameter.SCHEDULE_TOP_LEVEL_OFFSET_PARAM).Set(roofHeight / (windResistantColNumber / 2 + 1) *(windResistantColNumber/2+1- i));
                            windResistantColEastL.StructuralMaterialId = windResistantColMaterial.Id;
                            allColList.Add(windResistantColEastL);
                            windResistantColList.Add(windResistantColEastL);

                        }

                    }
                    //生成梁
                    if (!beamType.IsActive)
                    {
                        beamType.Activate();
                    }
                    for (int i = 0; i < bentNumber - 1; i++)
                    {
                        Curve curve1 = Line.CreateBound(pColL[i] + new XYZ(0, 0, colHeight), pColL[i + 1] + new XYZ(0, 0, colHeight));
                        FamilyInstance beam1 = doc.Create.NewFamilyInstance(curve1, beamType, ColToplevel, Autodesk.Revit.DB.Structure.StructuralType.Beam);
                        beam1.StructuralMaterialId = beamMaterial.Id;
                        Curve curve2 = Line.CreateBound(pColR[i] + new XYZ(0, 0, colHeight), pColR[i + 1] + new XYZ(0, 0, colHeight));
                        FamilyInstance beam2 = doc.Create.NewFamilyInstance(curve2, beamType, ColToplevel, Autodesk.Revit.DB.Structure.StructuralType.Beam);
                        beam2.StructuralMaterialId = beamMaterial.Id;

                    }
                    //生成屋脊梁
                    if (dialog.IsRoofBeam.IsChecked == true)
                    {
                        if (!roofBeamType.IsActive)
                        {
                            roofBeamType.Activate();
                        }
                        for (int i = 0; i < pRoof.Count - 1; i++)
                        {
                            Curve curve = Line.CreateBound(pRoof[i], pRoof[i + 1]);
                            FamilyInstance roofBeam = doc.Create.NewFamilyInstance(curve, roofBeamType, ColToplevel, Autodesk.Revit.DB.Structure.StructuralType.Beam);
                            roofBeam.StructuralMaterialId = roofBeamMaterial.Id;
                        }
                    }
                    //生成屋架
                    if (!roofType.IsActive)
                    {
                        roofType.Activate();
                    }
                    for (int i = 0; i < bentNumber; i++)
                    {
                        Curve curve1 = Line.CreateBound(pColL[i] + new XYZ(0, 0, colHeight), pRoof[i]);
                        FamilyInstance rf1 = doc.Create.NewFamilyInstance(curve1, roofType, ColToplevel, Autodesk.Revit.DB.Structure.StructuralType.Beam);
                        rf1.StructuralMaterialId = roofMaterial.Id;
                        Curve curve2 = Line.CreateBound(pColR[i] + new XYZ(0, 0, colHeight), pRoof[i]);
                        FamilyInstance rf2 = doc.Create.NewFamilyInstance(curve2, roofType, ColToplevel, Autodesk.Revit.DB.Structure.StructuralType.Beam);
                        rf2.StructuralMaterialId = roofMaterial.Id;
                    }

                    //以数量定义,生成檩条                                  
                    double angle = Math.Atan(roofHeight / depth * 2);
                    if (!purlinType.IsActive)
                    {
                        purlinType.Activate();
                    }
                    if (dialog.IsNumberDefine.IsChecked == true)
                    {
                        //生成右侧檩条   
                        for (int i = 0; i < bentNumber - 1; i++)
                        {
                            for (int j = 0; j <= purlinNumberR - 1; j++)
                            {
                                XYZ purlinStart = pColTopR[i] + new XYZ(0, purlinCR * Math.Cos(angle), purlinCR * Math.Sin(angle))
                                    + j / (purlinNumberR - 1) * (pRoof[i] - new XYZ(0, purlinRR * Math.Cos(angle), purlinRR * Math.Sin(angle))
                                    - pColTopR[i] - new XYZ(0, purlinCR * Math.Cos(angle), purlinCR * Math.Sin(angle)));
                                XYZ purlinEnd = pColTopR[i + 1] + new XYZ(0, purlinCR * Math.Cos(angle), purlinCR * Math.Sin(angle))
                                    + j / (purlinNumberR - 1) * (pRoof[i + 1] - new XYZ(0, purlinRR * Math.Cos(angle), purlinRR * Math.Sin(angle))
                                    - pColTopR[i + 1] - new XYZ(0, purlinCR * Math.Cos(angle), purlinCR * Math.Sin(angle)));
                                Curve curve = Line.CreateBound(purlinStart, purlinEnd);
                                FamilyInstance purlin = doc.Create.NewFamilyInstance(curve, purlinType, ColToplevel, Autodesk.Revit.DB.Structure.StructuralType.Beam);
                                //设置檩条旋转角度
                                purlin.StructuralMaterialId = purlinMaterial.Id;
                                purlin.get_Parameter(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE).Set(-angle);
                                purlin.get_Parameter(BuiltInParameter.Z_JUSTIFICATION).Set(3);
                            }
                        }
                        //生成左侧檩条
                        for (int i = 0; i < bentNumber - 1; i++)
                        {
                            for (int j = 0; j <= purlinNumberL - 1; j++)
                            {
                                XYZ purlinStart = pColTopL[i] - new XYZ(0, purlinCL * Math.Cos(angle), -purlinCL * Math.Sin(angle))
                                    + j / (purlinNumberL - 1) * (pRoof[i] + new XYZ(0, purlinRL * Math.Cos(angle), -purlinRL * Math.Sin(angle))
                                    - pColTopL[i] + new XYZ(0, purlinCL * Math.Cos(angle), -purlinCL * Math.Sin(angle)));
                                XYZ purlinEnd = pColTopL[i + 1] - new XYZ(0, purlinCL * Math.Cos(angle), -purlinCL * Math.Sin(angle))
                                    + j / (purlinNumberL - 1) * (pRoof[i + 1] + new XYZ(0, purlinRL * Math.Cos(angle), -purlinRL * Math.Sin(angle))
                                    - pColTopL[i + 1] + new XYZ(0, purlinCL * Math.Cos(angle), -purlinCL * Math.Sin(angle)));
                                Curve curve = Line.CreateBound(purlinStart, purlinEnd);
                                FamilyInstance purlin = doc.Create.NewFamilyInstance(curve, purlinType, ColToplevel, Autodesk.Revit.DB.Structure.StructuralType.Beam);
                                //设置檩条旋转角度
                                purlin.StructuralMaterialId = purlinMaterial.Id;
                                purlin.get_Parameter(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE).Set(angle);
                                purlin.get_Parameter(BuiltInParameter.Z_JUSTIFICATION).Set(3);
                            }
                        }
                    }
                    //以间隔定义，生成檩条
                    if (dialog.IsSpaceDefine.IsChecked == true)
                    {
                        //生成右侧檩条
                        for (int i = 0; i < bentNumber - 1; i++)
                        {
                            double initialValue = 0;
                            double roofLengthR = Math.Sqrt(depth * depth / 4 + roofHeight * roofHeight);
                            while (initialValue < roofLengthR - purlinCR - purlinRR)
                            {

                                XYZ purlinStart = pColTopR[i] + new XYZ(0, purlinCR * Math.Cos(angle), purlinCR * Math.Sin(angle))
                                    + new XYZ(0, initialValue * Math.Cos(angle), initialValue * Math.Sin(angle));
                                XYZ purlinEnd = pColTopR[i + 1] + new XYZ(0, purlinCR * Math.Cos(angle), purlinCR * Math.Sin(angle))
                                    + new XYZ(0, initialValue * Math.Cos(angle), initialValue * Math.Sin(angle));
                                Curve curve = Line.CreateBound(purlinStart, purlinEnd);
                                FamilyInstance purlin = doc.Create.NewFamilyInstance(curve, purlinType, ColToplevel, Autodesk.Revit.DB.Structure.StructuralType.Beam);
                                //设置檩条旋转角度
                                purlin.StructuralMaterialId = purlinMaterial.Id;
                                purlin.get_Parameter(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE).Set(-angle);
                                purlin.get_Parameter(BuiltInParameter.Z_JUSTIFICATION).Set(3);
                                initialValue += purlinSpaceR;

                            }
                        }
                        //生成左侧檩条
                        for (int i = 0; i < bentNumber - 1; i++)
                        {
                            double initialValue = 0;
                            double roofLengthL = Math.Sqrt(depth * depth / 4 + roofHeight * roofHeight);
                            while (initialValue < roofLengthL - purlinCL - purlinRL)
                            {
                                XYZ purlinStart = pColTopL[i] + new XYZ(0, -purlinCL * Math.Cos(angle), purlinCL * Math.Sin(angle))
                                    + new XYZ(0, -initialValue * Math.Cos(angle), initialValue * Math.Sin(angle));
                                XYZ purlinEnd = pColTopL[i + 1] + new XYZ(0, -purlinCL * Math.Cos(angle), purlinCL * Math.Sin(angle))
                                    + new XYZ(0, -initialValue * Math.Cos(angle), initialValue * Math.Sin(angle));
                                Curve curve = Line.CreateBound(purlinStart, purlinEnd);
                                FamilyInstance purlin = doc.Create.NewFamilyInstance(curve, purlinType, ColToplevel, Autodesk.Revit.DB.Structure.StructuralType.Beam);
                                //设置檩条旋转角度
                                purlin.StructuralMaterialId = purlinMaterial.Id;
                                purlin.get_Parameter(BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE).Set(angle);
                                purlin.get_Parameter(BuiltInParameter.Z_JUSTIFICATION).Set(3);
                                initialValue += purlinSpaceL;
                            }
                        }
                    }
                    //生成柱间支撑
                    if (!colBracingType.IsActive)
                    {
                        colBracingType.Activate();
                    }
                    for (int i = 0; i < bentNumber - 1; i++)
                    {
                        switch ((dialog.ColBracingList.Items[i] as ColBracingInfo).SelectedBracingTypeR)
                        {
                            case "十字形支撑":
                                Curve crossCurve1 = Line.CreateBound(pColR[i], pColTopR[i + 1]);
                                FamilyInstance crossBracingR1 = doc.Create.NewFamilyInstance(crossCurve1, colBracingType, ColToplevel, Autodesk.Revit.DB.Structure.StructuralType.Beam);
                                crossBracingR1.StructuralMaterialId = colBracingMaterial.Id;
                                Curve crossCurve2 = Line.CreateBound(pColTopR[i], pColR[i + 1]);
                                FamilyInstance crossBracingR2 = doc.Create.NewFamilyInstance(crossCurve2, colBracingType, ColToplevel, Autodesk.Revit.DB.Structure.StructuralType.Beam);
                                crossBracingR1.StructuralMaterialId = colBracingMaterial.Id;
                                break;
                            case "人字形支撑":
                                Curve herringboneCurve1 = Line.CreateBound(pColR[i], (pColTopR[i] + pColTopR[i + 1]) / 2);
                                FamilyInstance herringboneBracingR1 = doc.Create.NewFamilyInstance(herringboneCurve1, colBracingType, ColToplevel, Autodesk.Revit.DB.Structure.StructuralType.Beam);
                                herringboneBracingR1.StructuralMaterialId = colBracingMaterial.Id;
                                Curve herringboneCurve2 = Line.CreateBound((pColTopR[i] + pColTopR[i + 1]) / 2, pColR[i + 1]);
                                FamilyInstance herringboneBracingR2 = doc.Create.NewFamilyInstance(herringboneCurve2, colBracingType, ColToplevel, Autodesk.Revit.DB.Structure.StructuralType.Beam);
                                herringboneBracingR2.StructuralMaterialId = colBracingMaterial.Id;
                                break;
                            default:
                                break;
                        }
                        switch ((dialog.ColBracingList.Items[i] as ColBracingInfo).SelectedBracingTypeL)
                        {
                            case "十字形支撑":
                                Curve crossCurve1 = Line.CreateBound(pColL[i], pColTopL[i + 1]);
                                FamilyInstance crossBracingL1 = doc.Create.NewFamilyInstance(crossCurve1, colBracingType, ColToplevel, Autodesk.Revit.DB.Structure.StructuralType.Beam);
                                crossBracingL1.StructuralMaterialId = colBracingMaterial.Id;
                                Curve crossCurve2 = Line.CreateBound(pColTopL[i], pColL[i + 1]);
                                FamilyInstance crossBracingL2 = doc.Create.NewFamilyInstance(crossCurve2, colBracingType, ColToplevel, Autodesk.Revit.DB.Structure.StructuralType.Beam);
                                crossBracingL1.StructuralMaterialId = colBracingMaterial.Id;
                                break;
                            case "人字形支撑":
                                Curve herringboneCurve1 = Line.CreateBound(pColL[i], (pColTopL[i] + pColTopL[i + 1]) / 2);
                                FamilyInstance herringboneBracingL1 = doc.Create.NewFamilyInstance(herringboneCurve1, colBracingType, ColToplevel, Autodesk.Revit.DB.Structure.StructuralType.Beam);
                                herringboneBracingL1.StructuralMaterialId = colBracingMaterial.Id;
                                Curve herringboneCurve2 = Line.CreateBound((pColTopL[i] + pColTopL[i + 1]) / 2, pColL[i + 1]);
                                FamilyInstance herringboneBracingL2 = doc.Create.NewFamilyInstance(herringboneCurve2, colBracingType, ColToplevel, Autodesk.Revit.DB.Structure.StructuralType.Beam);
                                herringboneBracingL2.StructuralMaterialId = colBracingMaterial.Id;
                                break;
                            default:
                                break;
                        }
                    }
                    //生成屋面支撑
                    if (!roofBracingType.IsActive)
                    {
                        roofBracingType.Activate();
                    }
                    for (int i = 0; i < bentNumber - 1; i++)
                    {                        
                        switch ((dialog.RofBracingList.Items[i] as RofBracingInfo).SelectedBracingTypeR)
                        {
                            case "十字形支撑":
                                Curve crossCurve1 = Line.CreateBound(pRoof[i], pColTopR[i + 1]);
                                FamilyInstance crossBracingR1 = doc.Create.NewFamilyInstance(crossCurve1, roofBracingType, ColToplevel, Autodesk.Revit.DB.Structure.StructuralType.Beam);
                                crossBracingR1.StructuralMaterialId = roofBracingMaterial.Id;
                                Curve crossCurve2 = Line.CreateBound(pColTopR[i], pRoof[i + 1]);
                                FamilyInstance crossBracingR2 = doc.Create.NewFamilyInstance(crossCurve2, roofBracingType, ColToplevel, Autodesk.Revit.DB.Structure.StructuralType.Beam);
                                crossBracingR1.StructuralMaterialId = roofBracingMaterial.Id;
                                break;
                            default:
                                break;
                        }
                        switch ((dialog.RofBracingList.Items[i] as RofBracingInfo).SelectedBracingTypeL)
                        {
                            case "十字形支撑":
                                Curve crossCurve1 = Line.CreateBound(pRoof[i], pColTopL[i + 1]);
                                FamilyInstance crossBracingL1 = doc.Create.NewFamilyInstance(crossCurve1, roofBracingType, ColToplevel, Autodesk.Revit.DB.Structure.StructuralType.Beam);
                                crossBracingL1.StructuralMaterialId = roofBracingMaterial.Id;
                                Curve crossCurve2 = Line.CreateBound(pColTopL[i], pRoof[i + 1]);
                                FamilyInstance crossBracingL2 = doc.Create.NewFamilyInstance(crossCurve2, roofBracingType, ColToplevel, Autodesk.Revit.DB.Structure.StructuralType.Beam);
                                crossBracingL1.StructuralMaterialId = roofBracingMaterial.Id;
                                break;
                            default:
                                break;
                        }
                    }
                    ts1.Commit();
                    //设置柱端连接方式
                    using (Transaction ts2 = new Transaction(doc, "AdvancedSetting"))
                    {
                        ts2.Start();
                        for (int i = 0; i < windResistantColList.Count; i++)
                        {
                            Extension.RotateByCenter(doc, windResistantColList[i], 90);
                        }
                        foreach (FamilyInstance col in allColList)
                        {
                            if (dialog.Connection.SelectedItem.ToString() == "固定")
                            {
                                Reference reference = Extension.GetAMColReference(col);
                                BoundaryConditions boundaryConditions = doc.Create.NewPointBoundaryConditions(reference, TranslationRotationValue.Fixed, 0, TranslationRotationValue.Fixed, 0
                                    , TranslationRotationValue.Fixed, 0, TranslationRotationValue.Fixed, 0, TranslationRotationValue.Fixed, 0, TranslationRotationValue.Fixed, 0);
                            }
                            if (dialog.Connection.SelectedItem.ToString() == "铰接")
                            {
                                Reference reference = Extension.GetAMColReference(col);
                                BoundaryConditions boundaryConditions = doc.Create.NewPointBoundaryConditions(reference, TranslationRotationValue.Fixed, 0, TranslationRotationValue.Fixed, 0
                                    , TranslationRotationValue.Fixed, 0, TranslationRotationValue.Release, 0, TranslationRotationValue.Release, 0, TranslationRotationValue.Release, 0);
                            }
                        }
                        ts2.Commit();

                    }
                }
            }
            //}
            //catch (Exception e)
            //{
            //    TaskDialog expTaskDialog = new TaskDialog("单层厂房Demo");
            //    expTaskDialog.MainContent = "无法生成模型，请重新输入参数";
            //    expTaskDialog.MainInstruction = "提示";
            //    TaskDialogResult expTaskDialogResult = expTaskDialog.Show();

            //}
            return Result.Succeeded;
        }
    }
}
