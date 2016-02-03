using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using NKD.Module.BusinessObjects;
using NKD.Import.LAS;
using System.Data.Objects.DataClasses;

namespace NKD.Import.ImportUtils
{
    public class LasImportUtils
    {
        public LasImportUtils() { }


        internal List<object> ImportLASFile(NKD.Import.LAS.LASFile lasFile, string origFilename, ModelImportStatus mos, Guid currentProject, Action<string, double> UpdateStatus)
        {
            int li = origFilename.LastIndexOf("\\");
            string tempHoleIDa = origFilename.Substring(li);
            li = tempHoleIDa.LastIndexOf(".");
            string tempHoleID = tempHoleIDa.Substring(1, li - 1);
            int rowCounter = 0;
            // now try and get the hole name from a header item.  Typically the header name might be in
            // WELL in the Well information header section
            string res = lasFile.LookupWellHeaderSection("WELL");
            if (res != null && res.Trim().Length > 0) {

                tempHoleID = res;
            }

            List<object> dataList = new List<object>();
            try
            {
                // here we need to create the Geophyiscs data row item
                var entityObj = new NKDC(BaseImportTools.XSTRING, null, false);
                //entityObj.AutoDetectChangesEnabled = false; //TODO: Exhaust this, should be faster now

                var physDataList = new List<Geophysics>();
                //var fDataList = new List<NKD.Module.BusinessObjects.File>();
                var fdDataList = new List<FileData>();
                
                Geophysics xG = new Geophysics();
                xG.FileName = origFilename;
                Guid gg = Guid.NewGuid();
                xG.GeophysicsID = gg;

                Guid holeGuid = CollarQueries.FindHeaderGuid(tempHoleID, currentProject);
                Guid resHole = new Guid();
                if (!holeGuid.ToString().Equals(resHole.ToString()))
                {
                    xG.HeaderID = holeGuid;
                }
                Guid unitGuid = new Guid("2395DE56-8F6F-4B0C-806C-DD2606B9902B"); //FIXME: Magic Number
                UnitQueries uq = new UnitQueries();
                DictionaryUnit xu = uq.FindUnits("m");
                if (xu != null)
                {
                    unitGuid = xu.UnitID;
                }
                xG.DimensionUnitID = unitGuid;
                xG.LasVersion = string.Format("{0:N1}",lasFile.versionValue);
                xG.LasWrap = lasFile.versionWrap;
                xG.LasNullValue = string.Format("{0:N2}",lasFile.nullValue);

                FileStream sr = null;
                try
                {
                    sr = new FileStream(lasFile.filePath, FileMode.Open);
                }
                catch (FileNotFoundException fex)
                {
                    Console.WriteLine("FileNotFoundException:" + fex.ToString());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

                NKD.Module.BusinessObjects.File F = new NKD.Module.BusinessObjects.File();
                F.LoadFromStream(lasFile.FileName(), sr);
                sr = null;
                Guid fdGUID = Guid.NewGuid();
                var fD = new FileData
                {
                    Author = default(string),
                    FileDataID = fdGUID,
                    ReferenceID = xG.GeophysicsID,
                    TableType = "X_Geophysics",
                    FileName = F.FileName,
                    FileBytes = F.FileBytes,
                    FileChecksum = Hash.ComputeHash(F.FileBytes),
                    MimeType = MimeTypes.MimeTypeHelper.GetMimeTypeByFileName(F.FileName)
                };
                xG.OriginalFileDataID = fD.FileDataID;
                physDataList.Add(xG);
                fdDataList.Add(fD);
                F = null;

                // here we need to add a GeophysicsMetadata item for each column
                Dictionary<string, Guid> metaDataIDLookup = new Dictionary<string, Guid>();
                var unitDataList = new List<DictionaryUnit>();
                var paramDataList = new List<Parameter>();
                var metaDataList = new List<GeophysicsMetadata>();

                foreach (string s in lasFile.columnHeaders)
                {
                    Parameter xp = null;
                    xp = GetParameterIDFor(entityObj, "LAS data column", s);

                    //test to see if we already have the unit
                    string splitter = s.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault().Trim();
                    uq = new UnitQueries();
                    xu = uq.FindUnits(splitter);

                    if (xp != null && xu != null)
                    {
                        xp.UnitID = xu.UnitID;
                        //xp.Unit = xu;
                    }
                    else
                    {
                        bool xpnull = false;
                        if (xp == null)
                        {
                            xpnull = true;
                            xp = new Parameter();
                            Guid pg = Guid.NewGuid();
                            xp.ParameterID = pg;
                            xp.ParameterType = "LAS data column";
                            xp.ParameterName = s;
                        }

                        //test to see if we already have added a unit into the list
                        if (unitDataList.Count > 0 && xu == null)
                        {
                            xu = unitDataList.Where(c => c.StandardUnitName == splitter).FirstOrDefault();
                        }
                        if (xu == null)
                        {
                            //create new unit here store it and pass to parameters
                            Guid ug = Guid.NewGuid();
                            xu = new DictionaryUnit
                            {
                                UnitID = ug,
                                StandardUnitName = splitter,
                                CoalUnitName = splitter,
                                StrictlySI = false
                            };

                            unitDataList.Add(xu);
                        }
                        
                        xp.UnitID = xu.UnitID;
                        if (xpnull)
                        {
                            paramDataList.Add(xp);
                        }
                    }

                    Guid gmid = Guid.NewGuid();
                    GeophysicsMetadata xgm = new GeophysicsMetadata
                    {
                        GeophysicsID = gg,
                        GeophysicsMetadataID = gmid,
                        Unit = xu.StandardUnitName,
                        Mnemonic = s,
                        ParameterID = xp.ParameterID
                    };

                    metaDataList.Add(xgm);
                    metaDataIDLookup.Add(s, gmid);
                }
                
                int insertCounter = 0;
                var geoDataList = new List<GeophysicsData>();
                foreach (LASDataRow ldr in lasFile.dataRows)
                {
                    double depth = ldr.depth;
                    
                    for (int i = 0; i < ldr.rowData.Count(); i++)
                    {
                        GeophysicsData xd1 = new GeophysicsData();
                        string s = lasFile.columnHeaders[i];
                        xd1.GeophysicsDataID = Guid.NewGuid();
                        Guid g = new Guid();
                        bool found = metaDataIDLookup.TryGetValue(s, out g);
                        if (found)
                        {
                            xd1.GeophysicsMetadataID = g;
                        }
                        xd1.Dimension = (decimal)depth;
                        xd1.MeasurementValue = (decimal)ldr.rowData[i];

                        geoDataList.Add(xd1);
                    }
                    insertCounter++;
                    rowCounter++;
                }

                lasFile = null;

                dataList.Add(fdDataList);
                dataList.Add(physDataList);
                dataList.Add(unitDataList);
                dataList.Add(paramDataList);
                dataList.Add(metaDataList);
                dataList.Add(geoDataList);

                fdDataList = null;
                physDataList = null;
                unitDataList = null;
                paramDataList = null;
                metaDataIDLookup = null;
                metaDataList = null;
                geoDataList = null;
            }
            catch (Exception ex) {
                mos.errorMessages.Add("Failed to complete import of LAS file: " + origFilename);
                mos.errorMessages.Add("Details: " + ex.Message.ToString());
                if (ex.InnerException != null)
                    mos.errorMessages.Add("Inner Exception: " + ex.InnerException.Message.ToString());
                mos.errorMessages.Add("Row: " + rowCounter);
            }
            mos.recordsAdded = rowCounter;
            return dataList;
        }

        private Parameter GetParameterIDFor(NKDC entityObj, string paramType, string paramName)
        {
            Parameter res = null;
            IQueryable<Parameter> resGP = entityObj.Parameters.Where(c => c.ParameterType.Trim().Equals(paramType) && c.ParameterName.Trim().Equals(paramName));
            foreach (Parameter xx in resGP)
            {
                res = xx;
                break;
            }
            return res;
        }
    }
}
