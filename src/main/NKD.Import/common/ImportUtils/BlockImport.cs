using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using NKD.Module.BusinessObjects;
using NKD.Import.FormatSpecification;
using NKD.Import.ColumnSpecs;
using NKD.Import.ImportUtils;

namespace NKD.Import.ImportUtils
{
    public class BlockImport
    {


        /// <summary>
        /// Add block model data
        /// </summary>
        /// <param name="textInputDataFile"></param>
        /// <param name="testMap"></param>
        /// <param name="blockModelGUID"></param>
        /// <param name="batchSize"></param>
        /// <param name="UpdateStatus"></param>
        /// <param name="numLines"></param>
        internal void AddBlockData(string textInputDataFile, ImportDataMap testMap, Guid blockModelGUID, int batchSize, Action<string, double> UpdateStatus, int numLines, string connString)
        {

            // iterate through the data lines
            int ct = 1;
            SqlConnection connection = null;
            // get a connection to the database
            try
            {
                connection = new SqlConnection(connString);
                connection.Open();
                int numCommits = 0;
                SqlTransaction trans;
                trans = connection.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted);
                List<SqlCommand> commands = new List<SqlCommand>();
                int tb = 0;
                int transactionBatchLimit = batchSize;

                // open the filestream and read the first line
                StreamReader sr = null;
                FileStream fs = null;
                try
                {
                    fs = new FileStream(textInputDataFile, FileMode.Open, FileAccess.Read, FileShare.Read);
                    sr = new StreamReader(fs);
                }
                catch (FileNotFoundException fex)
                {
                    throw fex;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                string line = null;
                float pct = 0;
                float bct = 1;

                // report every X blocks
                int repCount = 0;
                int reportOnBlock = 1000;
                float fNumLines = (float)numLines;
                if (sr != null)
                {
                    while ((line = sr.ReadLine()) != null)
                    {
                        repCount++;
                        
                        if (repCount == reportOnBlock) {
                            repCount = 0;
                            // now report status
                            pct = ( bct / fNumLines) * 100.0f;
                            UpdateStatus("Writing block " + bct + " to database" , pct);
                        }
                        bct++;

                        if (ct >= testMap.dataStartLine)
                        {
                            string statementPart1 = "INSERT INTO " + testMap.mapTargetPrimaryTable + " ";
                            string clauseValues = "";
                            string clauseParameters = "";

                            List<string> items = parseTestLine(line, testMap.inputDelimiter);
                            // now pick out all the mapped values
                            foreach (ColumnMap cmap in testMap.columnMap)
                            {
                                int colID = cmap.sourceColumnNumber;
                                string columnValue = cmap.defaultValue;
                                if (colID >= 0)
                                {
                                    columnValue = items[colID];
                                }
                                string targetCol = cmap.targetColumnName;
                                string targetTable = cmap.targetColumnTable;
                                clauseValues += "" + targetTable + "." + targetCol + ",";


                                if (cmap.importDataType.Equals(ImportDataMap.NUMERICDATATYPE))
                                {
                                    if (columnValue.Equals("-")) {
                                        if (cmap.defaultValue != null && cmap.defaultValue.Length > 0)
                                        {
                                            columnValue = cmap.defaultValue;
                                        }
                                        else
                                        {
                                            columnValue = "NULL";
                                        }
                                    }
                                    clauseParameters += columnValue + ",";
                                }
                                else
                                {
                                    clauseParameters += "\'" + columnValue + "\',";
                                }
                            }
                            // now just a hack to remove the final coma from the query
                            clauseParameters = clauseParameters.Substring(0, clauseParameters.Length - 1);
                            clauseValues = clauseValues.Substring(0, clauseValues.Length - 1);

                            string commandText = statementPart1 + "(" + clauseValues + ") VALUES (" + clauseParameters + ")";
                            SqlCommand sqc = new SqlCommand(commandText, connection, trans);

                            //commands.Add(sqc);
                            sqc.ExecuteNonQuery();
                            tb++;
                            if (tb == transactionBatchLimit)
                            {
                                // commit batch, then renew the transaction
                                trans.Commit();
                                numCommits++;
                                //   trans = null;
                                trans = connection.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted);
                                // reset counter
                                tb = 0;
                            }
                        }

                        ct++;

                    }
                }
                if (tb > 0)
                {
                    trans.Commit();
                    numCommits++;
                }
                UpdateStatus("Finished writing blocks to database ", 100.0);

            }
            catch (Exception ex)
            {
                UpdateStatus("Error writing blocks to database\n\n " + ex.ToString(), 0);
            }
            finally {
                try { connection.Close(); }
                catch { }
            }
           
            
            

        }


        /// <summary>
        /// Add block data to the model, provided by the supplied file stream and format defintion
        /// </summary>
        /// <param name="bmFileStream"></param>
        /// <param name="importMap"></param>
        /// <param name="blockModelGUID"></param>
        /// <param name="batchSize"></param>
        /// <param name="UpdateStatus"></param>
        /// <param name="approxNumLines"></param>
        internal List<string> AddBlockData(ModelImportStatus mos, Stream bmFileStream, ImportDataMap importMap, Guid blockModelGUID, int batchSize, Action<string, double> UpdateStatus, int numLines, string connString)
        {
            // iterate through the data lines
            int ct = 1;
            int linesRead = 0;
            int total = 0;
            SqlConnection connection = null;
            List<string> uniqueDomains = new List<string>();
            // get a connection to the database
            try
            {
                int domainColIDX = -1;
                // find the column ID for the specified Domain field, as we need to capture this list.
                foreach (ColumnMap cm in importMap.columnMap) {
                    if (cm.targetColumnName.Trim().Equals("Domain")) {
                        domainColIDX = cm.sourceColumnNumber;
                    }
                }


                connection = new SqlConnection(connString);
                connection.Open();
                
                int numCommits = 0;
                SqlTransaction trans;
                trans = connection.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted);
                List<SqlCommand> commands = new List<SqlCommand>();
                int tb = 0;
                int transactionBatchLimit = batchSize;

                // open the filestream and read the first line
                StreamReader sr = null;
                //FileStream fs = null;
                try
                {
                    //fs = new FileStream(textInputDataFile, FileMode.Open, FileAccess.Read, FileShare.Read);
                    sr = new StreamReader(bmFileStream);
                }               
                catch (Exception ex)
                {
                    mos.AddErrorMessage("Error getting data stream for input model:\n" + ex.ToString());
                    mos.finalErrorCode = ModelImportStatus.ERROR_LOADING_FILE;
                }
                string line = null;
                float pct = 0;
                float bct = 1;

                // report every X blocks
                int repCount = 0;
                int reportOnBlock = 500;
                float fNumLines = (float)numLines;
                bool commitToDB = true;
                if (sr != null)
                {
                    while ((line = sr.ReadLine()) != null)
                    {                        
                        linesRead++;
                        repCount++;

                        if (repCount == reportOnBlock)
                        {
                            repCount = 0;
                            // now report status
                            pct = (bct / fNumLines) * 100.0f;
                            UpdateStatus("Writing block " + bct + " to database", pct);
                        }
                        bct++;

                        if (ct >= importMap.dataStartLine)
                        {
                            string statementPart1 = "INSERT INTO " + importMap.mapTargetPrimaryTable + " ";
                            string clauseValues = "";
                            string clauseParameters = "";

                            List<string> items = parseTestLine(line, importMap.inputDelimiter);
                            // now pick out all the mapped values
                            foreach (ColumnMap cmap in importMap.columnMap)
                            {
                                int colID = cmap.sourceColumnNumber;
                                string columnValue = cmap.defaultValue;
                                if (colID >= 0)
                                {
                                    columnValue = items[colID];
                                }
                                if (cmap.sourceColumnNumber == domainColIDX) {
                                    if (!uniqueDomains.Contains(columnValue.Trim())) {
                                        uniqueDomains.Add(columnValue);
                                    }
                                }
                                string targetCol = cmap.targetColumnName;
                                string targetTable = cmap.targetColumnTable;
                                clauseValues += "" + targetTable + "." + targetCol + ",";


                                if (cmap.importDataType.Equals(ImportDataMap.NUMERICDATATYPE))
                                {
                                    if (columnValue.Equals("-"))
                                    {
                                        if (cmap.defaultValue != null && cmap.defaultValue.Length > 0)
                                        {
                                            columnValue = cmap.defaultValue;
                                        }
                                        else
                                        {
                                            columnValue = "NULL";
                                        }
                                    }
                                    clauseParameters += columnValue + ",";
                                }
                                else
                                {
                                    clauseParameters += "\'" + columnValue + "\',";
                                }
                            }
                            // now just a hack to remove the final coma from the query
                            clauseParameters = clauseParameters.Substring(0, clauseParameters.Length - 1);
                            clauseValues = clauseValues.Substring(0, clauseValues.Length - 1);

                            string commandText = statementPart1 + "(" + clauseValues + ") VALUES (" + clauseParameters + ")";
                            SqlCommand sqc = new SqlCommand(commandText, connection, trans);
                            if (commitToDB)
                            {
                                sqc.ExecuteNonQuery();
                            }
                            tb++;
                            if (tb == transactionBatchLimit)
                            {
                                // commit batch, then renew the transaction
                                if (commitToDB)
                                {
                                    total += tb;
                                    trans.Commit();
                                    numCommits++;
                                    //   trans = null;
                                    trans = connection.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted);
                                }
                                // reset counter
                                tb = 0;
                            }
                        }
                        ct++;
                    }
                }
                if (tb > 0)
                {
                    if (commitToDB)
                    {
                        total += tb;
                        trans.Commit();
                    }
                    numCommits++;
                }
                UpdateStatus("Finished writing blocks to database ", 100.0);
            }
            catch (Exception ex)
            {
                UpdateStatus("Error writing blocks to database ", 0);
                mos.AddErrorMessage("Error writing block data at line "+linesRead+":\n" + ex.ToString());
                mos.finalErrorCode = ModelImportStatus.ERROR_WRITING_TO_DB;
            }
            finally
            {
                try { connection.Close(); }
                catch (Exception ex) {
                    mos.AddErrorMessage("Error closing conenction to database:\n"+ex.ToString());
                    mos.finalErrorCode = ModelImportStatus.ERROR_WRITING_TO_DB;
                }
            }

            mos.RecordsImported = total;
            mos.linesReadFromSource = linesRead;

            return uniqueDomains;   
        }


        /// <summary>
        /// Parse a text delimited line of data into items
        /// </summary>
        /// <param name="ln"></param>
        /// <param name="delimeter"></param>
        /// <returns></returns>
        private List<string> parseTestLine(string ln, char delimeter)
        {
            string[] items = ln.Split(new char[] { delimeter }, StringSplitOptions.None);
            return new List<string>(items);

        }

        internal List<BlockModelMetadata> SetBlockModelMetaData(Guid blockModelGUID, ImportDataMap testMap, string connString)
        {
            using (var entityObj = new NKDC(connString, null))
            {
                List<BlockModelMetadata> metaDataItems = new List<BlockModelMetadata>();

                foreach (ColumnMap cmap in testMap.columnMap)
                {
                    BlockModelMetadata metaData = new BlockModelMetadata();
                    metaData.BlockModelID = blockModelGUID;
                    metaData.BlockModelMetadataID = Guid.NewGuid();
                    metaData.IsColumnData = true;
                    string colName = cmap.sourceColumnName;
                    string columnValue = cmap.defaultValue;
                    Parameter param1 = new Parameter();
                    param1.ParameterName = cmap.targetColumnName;                   // source column
                    param1.ParameterType = "FieldName";
                    if (entityObj.BlockModelMetadatas.Where(f => f.BlockModelID == blockModelGUID && f.Parameter.Description == cmap.sourceColumnName).Any())
                        param1.Description = cmap.sourceColumnName = string.Format("{0}_{1}", cmap.sourceColumnName, Guid.NewGuid());
                    else
                        param1.Description = cmap.sourceColumnName;                                   // target column
                    param1.ParameterID = Guid.NewGuid();

                    if (cmap.sourceColumnName != null && cmap.sourceColumnName.ToLower().Contains("ppm"))
                    {
                        param1.UnitID = new Guid("E91773A4-2762-4EDE-8510-38F78FAF981D");// TODO: HACK - get the proper guid for the current unit type by querying database
                    }
                    else if (cmap.sourceColumnName != null && cmap.sourceColumnName.ToLower().Contains("pct"))
                    {
                        param1.UnitID = new Guid("AEDBBE0A-6A94-419F-8B43-A98CE942669A");// TODO: HACK - get the proper guid for the current unit type by querying database
                    }

                    metaData.BlockModelMetadataText = cmap.targetColumnName;
                    metaData.ParameterID = param1.ParameterID;
                    entityObj.Parameters.AddObject(param1);
                    entityObj.BlockModelMetadatas.AddObject(metaData);
                    metaDataItems.Add(metaData);
                    entityObj.SaveChanges();
                }
                return metaDataItems;
            }
        }




        internal ModelImportStatus UpdateBlockData(Stream bmStream, Guid guid, string colToInsertTo, string connString, char delimeter)
        {
            ModelImportStatus mos = new ModelImportStatus();
            // iterate through the data lines
            int ct = 1;
            int linesRead = 0;
            int total = 0;
            SqlConnection connection = null;
            List<string> uniqueDomains = new List<string>();
            // get a connection to the database
            try
            {
                
                connection = new SqlConnection(connString);
                connection.Open();
                int numCommits = 0;
                SqlTransaction trans;
                trans = connection.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted);
                List<SqlCommand> commands = new List<SqlCommand>();
                int tb = 0;
                int transactionBatchLimit = 500; ;

                // open the filestream and read the first line
                StreamReader sr = null;
                try
                {
                    sr = new StreamReader(bmStream);
                }
                catch (Exception ex)
                {
                    mos.AddErrorMessage("Error getting data stream for input model:\n" + ex.ToString());
                    mos.finalErrorCode = ModelImportStatus.ERROR_LOADING_FILE;
                }
                string line = null;
                float bct = 1;
                int repCount = 0;
                bool commitToDB = true;
                if (sr != null)
                {
                    while ((line = sr.ReadLine()) != null)
                    {
                        linesRead++;
                        repCount++;
                        bct++;

                        if (ct > 1)
                        {
                            List<string> items = parseTestLine(line, delimeter);

                            string statement1 = "UPDATE X_BlockModelBlock SET "+colToInsertTo+"="+items[3]+" WHERE BlockModelID=\'"+guid+"\' AND "+
                                                        "CentroidX=" + items[0] + " AND CentroidY=" + items[1] + " AND CentroidZ=" + items[2];


                            SqlCommand sqc = new SqlCommand(statement1, connection, trans);
                            if (commitToDB)
                            {
                                sqc.ExecuteNonQuery();
                            }
                            tb++;
                            if (tb == transactionBatchLimit)
                            {
                                // commit batch, then renew the transaction
                                if (commitToDB)
                                {
                                    trans.Commit();
                                    numCommits++;
                                    total += tb;
                                    //   trans = null;
                                    trans = connection.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted);
                                }
                                // reset counter
                                tb = 0;
                            }
                        }
                        ct++;
                    }
                }
                if (tb > 0)
                {
                    if (commitToDB)
                    {
                        trans.Commit();
                        total += tb;
                    }
                    numCommits++;
                }
               // UpdateStatus("Finished writing blocks to database ", 100.0);
            }
            catch (Exception ex)
            {
               // UpdateStatus("Error writing blocks to database ", 0);
                mos.AddErrorMessage("Error writing block data at line " + linesRead + ":\n" + ex.ToString());
                mos.finalErrorCode = ModelImportStatus.ERROR_WRITING_TO_DB;
            }
            finally
            {
                mos.RecordsImported = total;
                try { connection.Close(); }
                catch (Exception ex)
                {
                    mos.AddErrorMessage("Error closing conenction to database:\n" + ex.ToString());
                    mos.finalErrorCode = ModelImportStatus.ERROR_WRITING_TO_DB;
                }
            }


            mos.linesReadFromSource = linesRead;
            return mos;
            
        }

        internal void UpdateDomains(List<Tuple<string, string>> domains, Guid blockModelID)
        {
            if (domains != null)
            {
                string metaDataType = "Domains";
                string tableType = "BlockModel";
                string cont = GenUtils.ToJson(domains);
                SetMetaDataItem(blockModelID, metaDataType, tableType, cont);
            }
        }

        /// <summary>
        /// Set an item of meta data with the given types and cvalues
        /// </summary>
        /// <param name="blockModelID"></param>
        /// <param name="metaDataType"></param>
        /// <param name="tableType"></param>
        /// <param name="cont"></param>
        private static void SetMetaDataItem(Guid blockModelID, string metaDataType, string tableType, string cont)
        {
            try
            {
                using (var entityObj = new NKDC(BaseImportTools.XSTRING, null))
                {
                    MetaData dt = new MetaData();
                    dt.MetaDataID = Guid.NewGuid();
                    dt.MetaDataType = metaDataType;
                    dt.ContentToIndex = cont;

                    MetaDataRelation rel = new MetaDataRelation();
                    rel.MetaDataRelationID = Guid.NewGuid();
                    rel.MetaDataID = dt.MetaDataID;
                    rel.TableType = tableType;
                    rel.ReferenceID = blockModelID;

                    entityObj.MetaDatas.AddObject(dt);
                    entityObj.SaveChanges();
                    entityObj.MetaDataRelations.AddObject(rel);
                    entityObj.SaveChanges();
                }

            }
            catch (Exception ex) { 
                string msg = "Error setting domains list "+ex.ToString();
            }

        }


    }
}
