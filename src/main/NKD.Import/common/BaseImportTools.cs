using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Metadata.Edm;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NKD.Import;
using NKD.Import.ColumnSpecs;
using NKD.Import.DataWrappers;
using NKD.Import.FormatSpecification;
using NKD.Import.ImportUtils;
using NKD.Import.LAS;
using NKD.Module.BusinessObjects;

namespace NKD.Import
{
    public class BaseImportTools
    {
        System.ComponentModel.BackgroundWorker currentWorker = null;
        public static string XSTRING = System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

        public BaseImportTools() { }

        public string TestConnection(string connString) {

            using (var entityObj = new NKDC(connString, null))
            {
                // talk to the import lib to do the import
                var query = from BlockModel in entityObj.BlockModels select new { BlockModel.BlockModelID, BlockModel.OriginX, BlockModel.OriginY, BlockModel.OriginZ, BlockModel.ProjectID };

                foreach (BlockModel bm in entityObj.BlockModels)
                {
                    Guid gu = bm.BlockModelID;
                    string alias = bm.Alias;
                    int proj = bm.Version;
                }

                return "In NKD.Import";
            }
        }


        public int GetNKDVersion(string connString) {
            int res = 0;
            //NKDImportEntities c = new NKDImportEntities();
            //c.Database.Connection.ConnectionString = connString;
            //DbSet<X_PrivateData> models = c.X_PrivateData;
            
            //var query = from o in c.X_PrivateData where o.UniqueID.Equals("NKDSchemaVersion") select new { versionNumber = o.Version };

            //foreach (var x in query)
            //{
            //    res = x.versionNumber;
            //}
            return res;
        }

        public List<string> GetBMColumns()
        {
            List<string> cols = new List<string>();
            //For each field in the database (or property in Linq object)
            BlockModelBlock ob = new BlockModelBlock();
            foreach (PropertyInfo pi in ob.GetType().GetProperties())
            {

                Type ty = pi.GetType();
                String name = pi.Name;

                cols.Add(name);
            }

            return cols;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bmDataFile"></param>
        /// <param name="selectedFormatBMFile"></param>
        /// <param name="importMap"></param>
        /// <param name="xOrigin"></param>
        /// <param name="yOrigin"></param>
        /// <param name="zOrigin"></param>
        /// <param name="worker"></param>
        /// <param name="approxNumLines"></param>
        /// <param name="NKDProjectID"></param>
        /// <param name="units"></param>
        /// <param name="connString"></param>
        /// <returns></returns>
        public string PerformBMImport(string bmDataFile, string selectedFormatBMFile, ImportDataMap importMap, double xOrigin, double yOrigin, double zOrigin, System.ComponentModel.BackgroundWorker worker, int approxNumLines, string NKDProjectID, string units, string connString)
        {
            this.currentWorker = worker;
            UpdateStatus("Connecting to NKD", 10.0);
            using (var entityObj = new NKDC(connString, null))
            {
                // talk to the import lib to do the import                
                var query = from BlockModel in entityObj.BlockModels select new { BlockModel.BlockModelID, BlockModel.OriginX, BlockModel.OriginY, BlockModel.OriginZ, BlockModel.ProjectID };

                List<string> cn = new List<string>();
                //For each field in the database (or property in Linq object)
                BlockModel ob = new BlockModel();
                foreach (PropertyInfo pi in ob.GetType().GetProperties())
                {
                    Type ty = pi.GetType();
                    String name = pi.Name;
                    cn.Add(name);
                }



                DateTime startTime = DateTime.Now;
                int batchSize = 100;
                UpdateStatus("Creating new NKD block model", 20.0);
                ImportUtils.BlockImport dbIm = new ImportUtils.BlockImport();

                Guid blockModelGUID = Guid.NewGuid();

                BlockModel xAdd = new BlockModel();
                xAdd.OriginX = (Decimal)xOrigin;                                   // TO-DO
                xAdd.OriginY = (Decimal)yOrigin;                                   // TO-DO
                xAdd.OriginZ = (Decimal)zOrigin;                                   // TO-DO


                xAdd.BlockModelID = blockModelGUID;
                xAdd.ProjectID = new Guid(NKDProjectID);       // TODO - allow user to pick size
                entityObj.BlockModels.AddObject(xAdd);
                entityObj.SaveChanges();
                UpdateStatus("Setting model meta data", 25.0);
                // add the meta data to identify all of the oclumns etc.
                List<BlockModelMetadata> blockColumnMetaData = dbIm.SetBlockModelMetaData(blockModelGUID, importMap, connString);

                // add the new BM guid to the column map as a default so that it is always entered
                importMap.columnMap.Add(new ColumnMap("", -1, "BlockModelBlock", "BlockModelID", ImportDataMap.TEXTDATATYPE, blockModelGUID.ToString(), null, units));

                // add the individual blocks
                dbIm.AddBlockData(bmDataFile, importMap, blockModelGUID, batchSize, UpdateStatus, approxNumLines, connString);
                //dbIm.AddBlockDataNorm(bmDataFile, importMap, blockModelGUID, batchSize, UpdateStatus, approxNumLines, blockColumnMetaData);

                DateTime endTime = DateTime.Now;
                long compVal = (endTime.Ticks - startTime.Ticks) / 1000;
                string message = "" + startTime.ToShortTimeString() + " Ended: " + endTime.ToShortTimeString();

                long xval = compVal;

                return "";
            }
        }


        public List<string> PerformBMImport(ModelImportStatus mos, Guid blockModelGUID, System.IO.Stream bmFileStream, System.IO.Stream ffFileStream, ImportDataMap importMap, double xOrigin, double yOrigin, double zOrigin, System.ComponentModel.BackgroundWorker worker, int approxNumLines, string NKDProjectID, string alias, Guid authorGuid, string connString)
        {
            this.currentWorker = worker;
            using (var entityObj = new NKDC(connString, null))
            {
                // talk to the import lib to do the import

                DateTime startTime = DateTime.Now;
                int batchSize = 1000;
                //UpdateStatus("Creating new NKD block model", 20.0);
                ImportUtils.BlockImport dbIm = null;
                try
                {
                    dbIm = new ImportUtils.BlockImport();
                    //ImportDataMap importMapLoaded = FormatSpecificationIO.ImportMapIO.LoadImportMap(ffFileStream);
                    BlockModel xAdd = new BlockModel();
                    xAdd.OriginX = (Decimal)xOrigin;                                   // TO-DO
                    xAdd.OriginY = (Decimal)yOrigin;                                   // TO-DO
                    xAdd.OriginZ = (Decimal)zOrigin;                                   // TO-DO
                    xAdd.Alias = alias;
                    // when on server, automatically pick up the author GUID and apply it to the model.
                    if (currentWorker == null)
                    {
                        xAdd.AuthorContactID = authorGuid;
                        xAdd.ResponsibleContactID = authorGuid;
                    }
                    xAdd.VersionUpdated = DateTime.UtcNow;

                    xAdd.BlockModelID = blockModelGUID;
                    xAdd.ProjectID = new Guid(NKDProjectID);       // TODO - allow user to pick size
                    entityObj.BlockModels.AddObject(xAdd);
                    entityObj.SaveChanges();
                    UpdateStatus("Setting model meta data", 25.0);
                    // add the meta data to identify all of the oclumns etc.
                }
                catch (Exception ex)
                {
                    mos.AddErrorMessage("Error setting block model defintion data. " + ex.ToString());
                }
                List<string> domains = new List<string>();
                if (dbIm != null)
                {
                    try
                    {
                        List<BlockModelMetadata> blockColumnMetaData = dbIm.SetBlockModelMetaData(blockModelGUID, importMap, connString);
                    }
                    catch (Exception ex)
                    {
                        mos.AddErrorMessage("Error setting block model meta data:\n" + ex.ToString());
                    }
                    try
                    {
                        // add the new BM guid to the column map as a default so that it is always entered
                        importMap.columnMap.Add(new ColumnMap("", -1, "BlockModelBlock", "BlockModelID", ImportDataMap.TEXTDATATYPE, blockModelGUID.ToString(), blockModelGUID.ToString(), ImportDataMap.UNIT_NONE));
                        // add the individual blocks
                        domains = dbIm.AddBlockData(mos, bmFileStream, importMap, blockModelGUID, batchSize, UpdateStatus, approxNumLines, connString);
                        // run this only if in wonows client (determined by the status of the worker thread at this stage)
                        if (currentWorker != null)
                        {
                            List<Tuple<string, string>> doms = new List<Tuple<string, string>>();
                            string domainColumnName = "Domain";
                            foreach (string ss in domains)
                            {
                                doms.Add(new Tuple<string, string>(domainColumnName, ss));
                            }
                            dbIm.UpdateDomains(doms, blockModelGUID);
                        }
                    }
                    catch (Exception ex)
                    {
                        mos.AddErrorMessage("Error adding block data:\n" + ex.ToString());
                    }

                }
                return domains;
            }
        }


        public void PerformCollarImport(ModelImportStatus mos, System.IO.Stream bmFileStream, System.IO.Stream ffFileStream, ImportDataMap importMap, System.ComponentModel.BackgroundWorker backgroundWorker, Guid NKDProjectID, string connString, List<string> existingHoleNames, bool overwrite)
        {
            this.currentWorker = null;

            // talk to the import lib to do the import

            DateTime startTime = DateTime.Now;
            int batchSize = 1000;
            //UpdateStatus("Creating new NKD block model", 20.0);
            ImportUtils.CollarImport collImp = null;

            collImp = new ImportUtils.CollarImport();
            int approxNumLines = 100;

            importMap.columnMap.Add(new ColumnMap("", -1, "Header", "ProjectID", ImportDataMap.TEXTDATATYPE, NKDProjectID.ToString(), NKDProjectID.ToString(), ImportDataMap.UNIT_NONE));
            collImp.AddCollarData(mos, bmFileStream, importMap, batchSize, UpdateStatus, approxNumLines, connString, existingHoleNames, NKDProjectID, overwrite);

        }



        public void PerformAssayImport(ModelImportStatus mos, System.IO.Stream bmFileStream, System.IO.Stream ffFileStream, 
                                        ImportDataMap importMap, System.ComponentModel.BackgroundWorker backgroundWorker, Guid NKDProjectID, 
                                        string connString, int numLines,bool checkForDuplicates, bool doImportOverwrite)
        {
            this.currentWorker = backgroundWorker;
            // talk to the import lib to do the import
            DateTime startTime = DateTime.Now;
            int batchSize = 100;
            //UpdateStatus("Creating new NKD block model", 20.0);
            ImportUtils.AssayImport assImp = null;
            assImp = new ImportUtils.AssayImport();
            assImp.AddAssayData(mos, bmFileStream, importMap, batchSize, UpdateStatus, numLines, connString, NKDProjectID, checkForDuplicates, doImportOverwrite);


        }

        public void PerformCoalQualityImport(ModelImportStatus mos, System.IO.Stream bmFileStream, System.IO.Stream ffFileStream,
                                      ImportDataMap importMap, System.ComponentModel.BackgroundWorker backgroundWorker, Guid NKDProjectID,
                                      string connString, int numLines, bool checkForDuplicates, bool doImportOverwrite)
        {
            this.currentWorker = backgroundWorker;
            // talk to the import lib to do the import
            DateTime startTime = DateTime.Now;
            int batchSize = 100;
            //UpdateStatus("Creating new NKD block model", 20.0);
            ImportUtils.CoalQualityImport cqImp = null;
            cqImp = new ImportUtils.CoalQualityImport();
            cqImp.AddCoalQualityData(mos, bmFileStream, importMap, batchSize, UpdateStatus, numLines, connString, NKDProjectID, checkForDuplicates, doImportOverwrite);


        }

        public void PerformSurveyImport(ModelImportStatus mos, System.IO.Stream bmFileStream, System.IO.Stream ffFileStream, ImportDataMap importMap, System.ComponentModel.BackgroundWorker backgroundWorker, Guid NKDProjectID, string connString, int numLines, bool doOverwrite, bool checkForDuplicates)
        {
            this.currentWorker = backgroundWorker;
            // talk to the import lib to do the import
            DateTime startTime = DateTime.Now;
            int batchSize = 100;
            //UpdateStatus("Creating new NKD block model", 20.0);
            ImportUtils.SurveyImport surImp = null;
            surImp = new ImportUtils.SurveyImport();
            
            surImp.AddSurveyData(mos, bmFileStream, importMap, batchSize, UpdateStatus, numLines, connString, NKDProjectID, doOverwrite, checkForDuplicates);


        }


        public void PerformLithoImport(ModelImportStatus mos, System.IO.Stream bmFileStream, System.IO.Stream ffFileStream, ImportDataMap importMap, System.ComponentModel.BackgroundWorker backgroundWorker, Guid NKDProjectID, string connectionString, int numLines, bool doOverwrite, bool checkForDuplicates)
        {
            this.currentWorker = backgroundWorker;
            // talk to the import lib to do the import
            DateTime startTime = DateTime.Now;
            int batchSize = 100;
            //UpdateStatus("Creating new NKD block model", 20.0);
            ImportUtils.LithoImport lithImp = null;
            lithImp = new ImportUtils.LithoImport();
            lithImp.AddLithoData(mos, bmFileStream, importMap, batchSize, UpdateStatus, numLines, connectionString, NKDProjectID, doOverwrite, checkForDuplicates);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="pct"></param>
        private void UpdateStatus(string message, double pct)
        {
            if (currentWorker != null)
            {
                currentWorker.ReportProgress((int)pct, message);
            }
        }




        /// <summary>
        /// 
        /// </summary>
        /// <param name="headerLine"></param>
        /// <param name="firstDataLine"></param>
        /// <param name="_originX"></param>
        /// <param name="_originY"></param>
        /// <param name="_originZ"></param>
        public void ParseDataLinesForOrigins(string headerLine, string firstDataLine, char delimeter, out double _originX, out double _originY, out double _originZ)
        {
            string[] headerItems = headerLine.Split(new char[] { delimeter }, StringSplitOptions.None);
            string[] lineItems = firstDataLine.Split(new char[] { delimeter }, StringSplitOptions.None);
            _originX = 0;
            _originY = 0;
            _originZ = 0;
         
            int index = -1;
            
            index = FindItemInLine(headerItems, "XMORIG");
            if (index > -1)
            {
                double.TryParse(lineItems[index], out _originX);
            }
            index = FindItemInLine(headerItems, "YMORIG");
            if (index > -1)
            {
                double.TryParse(lineItems[index], out _originY);
            }
            
            index = FindItemInLine(headerItems, "ZMORIG");
            if (index > -1)
            {
                double.TryParse(lineItems[index], out _originZ);
            }

         
        }

        private static int FindItemInLine(string[] headerItems, string find)
        {
            int ct = 0;
            int index = -1;
            foreach (string s in headerItems)
            {
                if (s.Trim().Equals(find))
                {
                    index = ct;
                    break;
                }
                ct++;
            }
            return index;
        }



        // attempt to use the block model file header to automatically create a defintion based on goldfields typical model formats
        public ImportDataMap AutoGenerateFormatDefinition(string headerLine, char delimeter)
        {
                       
            Dictionary<string, bool> autoMap = new Dictionary<string, bool>();
            string[] headerItems = headerLine.Split(new char[] { delimeter }, StringSplitOptions.None);
            // iterate through each item in the header and assign it to a target column. 
            foreach(string ss in headerItems){
                autoMap.Add(ss, false);
            }

            // manually here get the core BM fields.
            // DB field = [Domain]
            ImportDataMap idm = new ImportDataMap();
            idm.columnMap = new List<ColumnMap>();
            idm.MaxColumns = headerItems.Length;
            idm.inputDelimiter = delimeter;
            idm.mapTargetPrimaryTable = "BlockModelBlock";
            idm.dataStartLine = 2;

            List<string> dmFields = new List<string>();

            int idx = -1;   
            
            string dbArea = "BlockModel";

            idx = AutoGenColMap(headerItems, idm,  "DOMAIN", "Domain", dbArea, autoMap);
            idx = AutoGenColMap(headerItems, idm, "XC", "CentroidX", dbArea, autoMap);     
            idx = AutoGenColMap(headerItems, idm, "YC", "CentroidY", dbArea, autoMap);
            idx = AutoGenColMap(headerItems, idm, "ZC", "CentroidZ", dbArea, autoMap);
            idx = AutoGenColMap(headerItems, idm, "XINC", "LengthX", dbArea, autoMap);
            idx = AutoGenColMap(headerItems, idm, "YINC", "LengthY", dbArea, autoMap);
            idx = AutoGenColMap(headerItems, idm, "ZINC", "LengthZ", dbArea, autoMap);
            idx = AutoGenColMap(headerItems, idm, "DENSITY", "Density", dbArea, autoMap);
            idx = AutoGenColMap(headerItems, idm, "RESCAT", "ResourceCategory", dbArea, autoMap);
            int num = 1;
            // auto map all ofther fields into numeric 1 to n
            foreach (KeyValuePair<string, bool> kvp in autoMap) {
                if ((bool)kvp.Value == false) {
                    string nm = kvp.Key;
                    // make a column map for this stright into the numeric, and keep track of numeric number
                    string targetFieldName = "Numeric" + num;
                    AutoGenColMap(headerItems, idm, nm, targetFieldName, dbArea, null);
                    num++;
                }
            }

            return idm;

        }

        private static int AutoGenColMap(string[] headerItems, ImportDataMap idm, string sourceName, string targetName, string dbArea, Dictionary<string, bool> autoMap)
        {
            int idx = FindItemInLine(headerItems, sourceName);
            if(idx > -1){
                
                idm.columnMap.Add(new ColumnMap(sourceName, idx, dbArea, targetName, ImportDataMap.NUMERICDATATYPE, null, null, null));
                if (autoMap != null) { autoMap[sourceName] = true; }
            }
            return idx;
        }

        public ModelImportStatus PerformBMAppend(System.IO.Stream bmStream, Guid bmGuid, string alias, string columnNameToImport, int columnIndexToImport, string connString, char delimiter)
        {
            // TODO: read stream and write updates to database

            // get the next column to write to - search meta data to get the list of occupied columns
            using (var entityObj = new NKDC(connString, null))
            {
                List<BlockModelMetadata> d = new List<BlockModelMetadata>();
                var o = entityObj.BlockModelMetadatas.Where(f => f.BlockModelID == bmGuid && f.IsColumnData == true).Select(f => (string)f.BlockModelMetadataText).ToArray();
                // yuk, ugly hack to get the next column to update into.  In the long run, use normalised data as it will be much easier
                int lastIndex = 0;
                foreach (string s in o)
                {
                    if (s.StartsWith("Numeric"))
                    {
                        string endBit = s.Substring(7);
                        int ival = -1;
                        bool parsed = int.TryParse(endBit, out ival);
                        if (parsed)
                        {
                            lastIndex = Math.Max(ival, lastIndex);
                        }

                    }
                }
                string colToInsertTo = "Numeric" + (lastIndex + 1);
                //TODO: add this new meta data item into the database

                //TODO: update the data within the database itself
                ImportUtils.BlockImport dbIm = new ImportUtils.BlockImport();
                ImportDataMap idm = new ImportDataMap();
                idm.columnMap = new List<ColumnMap>();
                idm.inputDelimiter = delimiter;
                idm.columnMap.Add(new ColumnMap(columnNameToImport, columnIndexToImport, "BlockModelBlock", colToInsertTo, ImportDataMap.NUMERICDATATYPE, null, null, null));
                dbIm.SetBlockModelMetaData(bmGuid, idm, connString);

                return dbIm.UpdateBlockData(bmStream, bmGuid, colToInsertTo, connString, delimiter);
            }

        }

        public List<ColumnMetaInfo> GetGeophysicsColumns()
        {
            //TODO Go away to database schema and get the columns
            

            List<ColumnMetaInfo> colList = new List<ColumnMetaInfo>();
            colList.Add(new ColumnMetaInfo() { columnName = "FROM" });
            colList.Add(new ColumnMetaInfo() { columnName = "[RESULT]" });
            

            return colList;
        }



        public List<ColumnMetaInfo> GetAssayColumns(string connectionString)
        {
            List<ColumnMetaInfo> colList = new List<ColumnMetaInfo>();

            List<FKSpecification> fkList = ForeignKeyUtils.QueryForeignKeyRelationships(connectionString, "X_Sample");

            Sample xag = new Sample();
            QueryColumnData(colList, fkList, xag);
            AssayGroupTestResult xtr = new AssayGroupTestResult();
            fkList = ForeignKeyUtils.QueryForeignKeyRelationships(connectionString, "X_AssayGroupTestResult");
            QueryColumnData(colList, fkList, xtr);

            List<string> removeStubs = new List<string>();
            removeStubs.Add("Sample");
            removeStubs.Add("Assay");
            removeStubs.Add("Version");
            removeStubs.Add("Dict");
            List<ColumnMetaInfo> colListP = new List<ColumnMetaInfo>();
            colListP = PruneColumnList(removeStubs, colList);
            ColumnMetaInfo ci = new ColumnMetaInfo();
            ci.columnName = "[ASSAY RESULT]";
            ci.fkSpec = null;
            colListP.Insert(0,ci);
            ColumnMetaInfo ci2 = new ColumnMetaInfo();
            ci2.columnName = "SampleNumber";
            ci2.fkSpec = null;
            colListP.Insert(1, ci2);
            ColumnMetaInfo ci3 = new ColumnMetaInfo();
            ci3.columnName = "SampleMassKg";
            ci3.fkSpec = null;
            colListP.Insert(2, ci3);
            ColumnMetaInfo ci4 = new ColumnMetaInfo();
            ci4.columnName = "StandardSampleTypeName";
            ci4.fkSpec = null;
            colListP.Insert(3, ci4);


            // now mark only the mandatory fields - for assay this will be header id, from, to and result
            foreach (ColumnMetaInfo c in colListP) {
                
                if (c.columnName.Equals("HeaderID")) {
                    c.isMandatory = true;
                }
                else if (c.columnName.Equals("FromDepth")) {
                    c.isMandatory = true;
                }
                else if (c.columnName.Equals("ToDepth")) {
                    c.isMandatory = true;
                }
            }

            return colListP;
        }

        public List<ColumnMetaInfo> GetCoalQualityColumns(string connectionString)
        {
            List<ColumnMetaInfo> colList = new List<ColumnMetaInfo>();

            List<FKSpecification> fkList = ForeignKeyUtils.QueryForeignKeyRelationships(connectionString, "X_Sample");

            Sample xag = new Sample();
            QueryColumnData(colList, fkList, xag);
            AssayGroupTestResult xtr = new AssayGroupTestResult();
            fkList = ForeignKeyUtils.QueryForeignKeyRelationships(connectionString, "X_AssayGroupTestResult");
            QueryColumnData(colList, fkList, xtr);

            List<string> removeStubs = new List<string>();
            removeStubs.Add("SampleCategoryID");
            removeStubs.Add("SampleStateID");
            removeStubs.Add("SampleTypeID");
            //removeStubs.Add("Sample");
            removeStubs.Add("Assay");
            removeStubs.Add("Version");
            removeStubs.Add("Dict");
            List<ColumnMetaInfo> colListP = new List<ColumnMetaInfo>();
            colListP = PruneColumnList(removeStubs, colList);

            ColumnMetaInfo ci5 = new ColumnMetaInfo();
            ci5.columnName = "[RESULT]";
            ci5.fkSpec = null;
            colListP.Insert(0, ci5);
            

            ColumnMetaInfo ci4 = new ColumnMetaInfo();
            ci4.columnName = "[WASH FRACTION]";
            ci4.fkSpec = null;
            colListP.Insert(0, ci4);

            ColumnMetaInfo ci3 = new ColumnMetaInfo();
            ci3.columnName = "[SIZE FRACTION]";
            ci3.fkSpec = null;
            colListP.Insert(0, ci3);

            ColumnMetaInfo ci2 = new ColumnMetaInfo();
            ci2.columnName = "[STAGE]";
            ci2.isMandatory = true;
            ci2.fkSpec = null;
            colListP.Insert(0, ci2);

            ColumnMetaInfo ci0 = new ColumnMetaInfo();
            ci0.columnName = "[PROGRAM]";
            ci0.isMandatory = true;
            ci0.fkSpec = null;
            colListP.Insert(0, ci0);

            // now mark only the mandatory fields - for assay this will be header id, from, to and result
            foreach (ColumnMetaInfo c in colListP) {
                
                if (c.columnName.Equals("HeaderID")) {
                    c.isMandatory = true;
                }
                else if (c.columnName.Equals("FromDepth")) {
                    c.isMandatory = true;
                }
                else if (c.columnName.Equals("ToDepth")) {
                    c.isMandatory = true;
                }
                else if (c.columnName.Equals("SampleID"))
                {
                    c.isMandatory = true;
                }
                else if (c.columnName.Equals("[PROCESS]"))
                {
                    c.isMandatory = false;
                }
                else if (c.columnName.Equals("[SCREEN]"))
                {
                    c.isMandatory = false;
                }
                else if (c.columnName.Equals("[FLOAT]"))
                {
                    c.isMandatory = false;
                }
                else if (c.columnName.Equals("[QUALITY]"))
                {
                    c.isMandatory = false;
                }
            }

            return colListP;
        }

        

        private List<ColumnMetaInfo> PruneColumnList(List<string> removeStubs, List<ColumnMetaInfo> colList)
        {
            List<ColumnMetaInfo> colListP = new List<ColumnMetaInfo>();
            
            foreach (ColumnMetaInfo cim in colList) {
                string colName = cim.columnName;
                bool doExclusion = false;
                foreach (string rem in removeStubs) {

                    if (colName.Trim().StartsWith(rem.Trim()))
                    {
                        doExclusion = true;
                        break;
                    }
                    

                }
                if (!doExclusion)
                {
                    colListP.Add(cim);
                }
            }
            return colListP;
        }

        private void QueryColumnData(List<ColumnMetaInfo> colList, List<FKSpecification> fkList, object xag)
        {
            foreach (PropertyInfo pi in xag.GetType().GetProperties())
            {

                MemberTypes mt = pi.MemberType;
                string val = mt.ToString();
                PropertyAttributes patt = pi.Attributes;
                string nm = pi.PropertyType.Name;
                String name = pi.Name;
                Type rt = pi.GetMethod.ReturnType;
                string tName = GuessMainType(rt.FullName);

                FKSpecification fkSpec = null;

                bool hasFK = LookupFKForColumn(name, out fkSpec, fkList);
                ColumnMetaInfo cmi = new ColumnMetaInfo() { columnName = name, fkSpec = fkSpec, hasFK = hasFK, dbTypeName = tName };
                colList.Add(cmi);
            }
        }

        public string GuessMainType(string typeFromEDM) {
            string rt = ImportDataMap.NUMERICDATATYPE;
            
            if (typeFromEDM.ToLower().Contains("system.string")) {
                rt = ImportDataMap.TEXTDATATYPE;
            }
            else if (typeFromEDM.ToLower().Contains("system.datetime"))
            {
                rt = ImportDataMap.TIMESTAMPDATATYPE;// "TIMESTAMP";
            }
            return rt;
        
        }

        public List<ColumnMetaInfo> GetCollarColumns(string connString)
        {
            Header ob = new Header();            
            string tableName = "X_Header";
            List<ColumnMetaInfo> colList = this.QueryColumns(connString, ob, tableName);

            List<string> removeStubs = new List<string>();
            removeStubs.Add("HeaderID");
            removeStubs.Add("ProjectID");
            removeStubs.Add("Header");
            removeStubs.Add("Version");
            removeStubs.Add("Dict");
            List<ColumnMetaInfo> colListP = new List<ColumnMetaInfo>();
            colListP = PruneColumnList(removeStubs, colList);

            // now mark only the mandatory fields - for assay this will be header id, from, to and result
            foreach (ColumnMetaInfo c in colListP)
            {

                if (c.columnName.Equals("HoleName"))
                {
                    c.isMandatory = true;
                }
                else if (c.columnName.Equals("Easting"))
                {
                    c.isMandatory = true;
                }
                else if (c.columnName.Equals("Northing"))
                {
                    c.isMandatory = true;
                }
                else if (c.columnName.Equals("Elevation"))
                {
                    c.isMandatory = true;
                }
            }

            return colListP;
        }


        public List<ColumnMetaInfo> GetSurveyColumns(string connString)
        {
            Survey ob = new Survey();

            string tableName = "X_Survey";

            List<ColumnMetaInfo> colList = QueryColumns(connString, ob, tableName);

            List<string> removeStubs = new List<string>();
            removeStubs.Add("Survey");
            removeStubs.Add("Version");
            removeStubs.Add("Dict");
            List<ColumnMetaInfo> colListP = new List<ColumnMetaInfo>();
            colListP = PruneColumnList(removeStubs, colList);

            // now mark only the mandatory fields - for assay this will be header id, from, to and result
            foreach (ColumnMetaInfo c in colListP)
            {

                if (c.columnName.Equals("HeaderID"))
                {
                    c.isMandatory = true;
                }
                else if (c.columnName.Equals("Depth"))
                {
                    c.isMandatory = true;
                }
                else if (c.columnName.Equals("Dip"))
                {
                    c.isMandatory = true;
                }
                else if (c.columnName.Equals("OriginalAzimuth"))
                {
                    c.isMandatory = true;
                }
                else if (c.columnName.Equals("CorrectedAzimuth"))
                {
                    c.isMandatory = true;
                }
            }

            return colListP;
        }


        public List<ColumnMetaInfo> GetLithoColumns(string connString)
        {
            Lithology ob = new Lithology();
            string tableName = "X_Lithology";

            List<ColumnMetaInfo> colList = QueryColumns(connString, ob, tableName);


            List<string> removeStubs = new List<string>();
            //removeStubs.Add("Litho"); //Removed due to this segment stripping out LithQual/Percentage etc
            removeStubs.Add("Version");
            removeStubs.Add("Dict");
            List<ColumnMetaInfo> colListP = new List<ColumnMetaInfo>();
            colListP = PruneColumnList(removeStubs, colList);

            // now mark only the mandatory fields - for assay this will be header id, from, to and result
            foreach (ColumnMetaInfo c in colListP)
            {

                if (c.columnName.Equals("HeaderID"))
                {
                    c.isMandatory = true;
                }
                else if (c.columnName.Equals("FromDepth"))
                {
                    c.isMandatory = true;
                }
                else if (c.columnName.Equals("ToDepth"))
                {
                    c.isMandatory = true;
                }

            }

            return colListP;
        }

        private List<ColumnMetaInfo> QueryColumns(string connString, object ob, string tableName)
        {
            
            List<FKSpecification> fkList = ForeignKeyUtils.QueryForeignKeyRelationships(connString, tableName);
            List<ColumnMetaInfo> colList = new List<ColumnMetaInfo>();
            foreach (PropertyInfo pi in ob.GetType().GetProperties())
            {
                MemberTypes mt = pi.MemberType;
                string val = mt.ToString();
                PropertyAttributes patt = pi.Attributes;
                string nm = pi.PropertyType.Name;
                String name = pi.Name;
                FKSpecification fkSpec = null;
                bool hasFK = LookupFKForColumn(name, out fkSpec, fkList);
                Type rt = pi.GetMethod.ReturnType;
                string tName = GuessMainType(rt.FullName);
                ColumnMetaInfo cmi = new ColumnMetaInfo() { columnName = name, fkSpec = fkSpec, hasFK = hasFK, dbTypeName = tName };
                colList.Add(cmi);
            }
            return colList;
        }

        private bool LookupFKForColumn(string name, out FKSpecification fkSpec, List<FKSpecification> fkList)
        {
            fkSpec = null;
            bool hasFK = false;
            foreach (FKSpecification f in fkList) {
                if (name.Trim().Equals(f.parentColumnName.Trim())) {
                    hasFK = true;
                    fkSpec = f;
                    break;
                }
            }

            return hasFK;
        }

        public Dictionary<Guid, string> GetProjectsList()
        {
            return ProjectUtils.GetProjectList();
        }

        public List<CollarInfo> FindCollarsInProject(Guid currentSelectedProject)
        {

            List<CollarInfo> collars = CollarQueries.FindCollarsForProject(currentSelectedProject);
            return collars;
        }

        public List<object> ImportLasFile(NKD.Import.LAS.LASFile lasFile, string origFilename, ModelImportStatus mos, Guid currentProjectID, System.ComponentModel.BackgroundWorker backgroundWorker)
        {
            this.currentWorker = backgroundWorker;
            // get the pre holeID from the filename
            List<object> data = new List<object>();
            LasImportUtils liu = new LasImportUtils();
            data = liu.ImportLASFile(lasFile, origFilename, mos, currentProjectID, UpdateStatus);

            return data;
        }

        public static void PopulateCMapShortcut(FormatSpecification.ImportDataMap importMap, Dictionary<string, int> columnIDX)
        {
            foreach (ColumnMap cm in importMap.columnMap)
            {
                if (cm.targetColumnName[0] != '[')
                    columnIDX.Add(cm.targetColumnName.Trim(), cm.sourceColumnNumber);

            }
        }

        public static List<string> ParseTestLine(string ln, char delimeter)
        {
            string[] items = ln.Split(new char[] { delimeter }, StringSplitOptions.None);
            return new List<string>(items);

        }
    }

}
