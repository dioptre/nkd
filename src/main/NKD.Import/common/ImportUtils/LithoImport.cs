using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data.SqlClient;
using NKD.Import.FormatSpecification;
using NKD.Module.BusinessObjects;

namespace NKD.Import.ImportUtils
{
    public class LithoImport
    {

        static bool commitToDB = true;

        /// <summary>
        /// Add new litho data.  If interval exists already, 
        /// then an overwrite request is made, the data stored internally then updated.
        /// </summary>
        /// <param name="mos"></param>
        /// <param name="fileStream"></param>
        /// <param name="importMap"></param>
        /// <param name="batchSize"></param>
        /// <param name="UpdateStatus"></param>
        /// <param name="approxNumLines"></param>
        /// <param name="connectionString"></param>
        /// <param name="NKDProjectID"></param>
        /// <param name="overwrite"></param>
        /// <param name="checkForDuplicates"></param>
        internal void AddLithoData(ModelImportStatus mos, Stream fileStream, FormatSpecification.ImportDataMap importMap, int batchSize, Action<string, double> UpdateStatus, int approxNumLines, string connectionString, Guid NKDProjectID, bool overwrite, bool checkForDuplicates)
        {           
            
            bool duplicateFound = false;
            // iterate through the data lines
            int ct = 1;
            int linesRead = 0;
            SqlConnection connection = null;
            SqlConnection secondaryConnection = null;

            Dictionary<Guid, List<string>> rejectedLines = new Dictionary<Guid, List<string>>();
            Dictionary<string, string> holeWarningMessages = new Dictionary<string, string>();
            using (var entityObj = new NKDC(connectionString, null))
            {
                //entityObj.Configuration.AutoDetectChangesEnabled = false;
                LithoQueries lq = new LithoQueries();


                // this loop makes sure that any guids are properly types so that a text string for that guid can be passed into the query
                foreach (ColumnMap cmap in importMap.columnMap)
                {
                    bool isFKColumn = cmap.hasFKRelation;
                    if (isFKColumn)
                    {
                        cmap.importDataType = ImportDataMap.TEXTDATATYPE;
                    }
                }

                // get a connection to the database
                try
                {

                    connection = new SqlConnection(connectionString);
                    connection.Open();

                    secondaryConnection = new SqlConnection(connectionString);
                    secondaryConnection.Open();


                    int numCommits = 0;
                    SqlTransaction trans;
                    //trans = connection.BeginTransaction(System.Data.IsolationLevel.Snapshot);
                    List<SqlCommand> commands = new List<SqlCommand>();
                    int tb = 0;
                    int transactionBatchLimit = batchSize;

                    // open the filestream and read the first line
                    StreamReader sr = null;
                    try
                    {
                        sr = new StreamReader(fileStream);
                    }
                    catch (Exception ex)
                    {
                        mos.AddErrorMessage("Error getting data stream for input data:\n" + ex.ToString());
                        mos.finalErrorCode = ModelImportStatus.ERROR_LOADING_FILE;
                    }
                    string line = null;
                    float bct = 1;

                    // report every X blocks
                    int repCount = 0;
                    //int reportOnBlock = 1000;
                    float fNumLines = (float)approxNumLines;


                    // get the column containing the hole name 

                    Dictionary<string, Guid> holeIDLookups = new Dictionary<string, Guid>();

                    int numberOfHolesAdded = 0;
                    ColumnMap headerCmap = importMap.FindItemsByTargetName("HeaderID");
                    ColumnMap depthFromCmap = importMap.FindItemsByTargetName("FromDepth");
                    ColumnMap depthToCmap = importMap.FindItemsByTargetName("ToDepth");
                    float progress = 0;

                    int headerIDX = headerCmap.sourceColumnNumber;
                    if (sr != null)
                    {
                        while ((line = sr.ReadLine()) != null)
                        {

                            repCount++;
                            bct++;
                            progress = ((float)bct / fNumLines) * 100.0f;
                            linesRead++;
                            if (line != null && line.Trim().Length == 0)
                            {
                                mos.AddWarningMessage("Empty data line found at line " + linesRead + ".");
                                continue;
                            }

                            if (ct >= importMap.dataStartLine)
                            {

                                string statementPart1 = "INSERT INTO " + importMap.mapTargetPrimaryTable + " ";
                                string clauseValues = "";
                                string clauseParameters = "";

                                List<string> items = parseTestLine(line, importMap.inputDelimiter);
                                // using the column map, pick out the hole name field and see if it is in the database already
                                string headerNameItem = items[headerIDX];
                                // check if this holename is a duplicate in the file



                                bool foundHole = false;



                                Guid holeID = new Guid();
                                bool lv = holeIDLookups.ContainsKey(headerNameItem);
                                if (!lv)
                                {
                                    string headerGUID = ForeignKeyUtils.FindFKValueInOther(headerNameItem, headerCmap, secondaryConnection, false, "HoleName", NKDProjectID);
                                    if (headerGUID == null)
                                    {
                                        // this means we have not found the specified records in the header table
                                        // Report on issue and skip line

                                    }
                                    else
                                    {
                                        foundHole = true;
                                        holeID = new Guid(headerGUID);
                                        holeIDLookups.Add(headerNameItem, holeID);

                                    }
                                }
                                else
                                {
                                    holeIDLookups.TryGetValue(headerNameItem, out holeID);
                                    foundHole = true;
                                }



                                if (!foundHole)
                                {

                                    mos.AddWarningMessage("Failed to find hole " + headerNameItem + ".  Skipping record at line " + linesRead + ".");
                                    UpdateStatus("Failed to find hole " + headerNameItem, progress);
                                    mos.finalErrorCode = ModelImportStatus.DATA_CONSISTENCY_ERROR;
                                    mos.recordsFailed++;
                                    continue;
                                }

                                if (checkForDuplicates == true && depthFromCmap != null && depthToCmap != null)
                                {
                                    // check for duplicate depths
                                    string d_from = items[depthFromCmap.sourceColumnNumber];
                                    string d_to = items[depthToCmap.sourceColumnNumber];
                                    decimal dtFrom = 0;
                                    decimal dtTo = 0;
                                    bool isParsedA = decimal.TryParse(d_from, out dtFrom);
                                    bool isParsedB = decimal.TryParse(d_to, out dtTo);
                                    if (isParsedA && isParsedB)
                                    {
                                        List<Guid> rr = lq.CheckForDuplicate(holeID, dtFrom, dtTo, secondaryConnection);
                                        //List<Guid> rr = sq.CheckForDuplicate(holeID, dt, entityObj);
                                        if (rr.Count > 0)
                                        {
                                            duplicateFound = true;

                                            if (!rejectedLines.ContainsKey(rr.First()))
                                            {
                                                rejectedLines.Add(rr.First(), items);
                                                mos.AddWarningMessage("Duplicate interval found in lithology data for hole " + headerNameItem + " at depth from " + dtFrom + " to " + dtTo + " on line " + linesRead);
                                                UpdateStatus("Dulicate found in data at  " + headerNameItem + " " + dtFrom + " to " + dtTo, progress);
                                            }
                                            else
                                            {
                                                mos.AddWarningMessage("Duplicate interval found in lithology data file for hole " + headerNameItem + " at depth " + dtFrom + " to " + dtTo + " on line " + linesRead);
                                                UpdateStatus("Dulicate found in file at  " + headerNameItem + " on line " + linesRead, progress);
                                                rejectedLines[rr.First()] = items;
                                            }
                                            if (!overwrite)
                                            {
                                                mos.recordsFailed++;
                                            }
                                            continue;

                                        }
                                    }
                                }


                                #region mappsearch
                                // now pick out all the mapped values
                                foreach (ColumnMap cmap in importMap.columnMap)
                                {

                                    if (cmap.targetColumnName.Trim().Equals("HeaderID"))
                                    {
                                        string targetCol = cmap.targetColumnName;
                                        string targetTable = cmap.targetColumnTable;
                                        clauseValues += "" + targetTable + "." + targetCol + ",";
                                        clauseParameters += "\'" + holeID.ToString() + "\',";

                                    }

                                    else
                                    {
                                        bool isFKColumn = cmap.hasFKRelation;

                                        int colID = cmap.sourceColumnNumber;
                                        string columnValue = cmap.defaultValue;
                                        if (colID >= 0)
                                        {
                                            columnValue = items[colID];
                                        }

                                        string targetCol = cmap.targetColumnName;
                                        string targetTable = cmap.targetColumnTable;
                                        clauseValues += "" + targetTable + "." + targetCol + ",";


                                        if (isFKColumn)
                                        {
                                            // go and search for the appropriate value from the foreign key table
                                            string newValue = ForeignKeyUtils.FindFKValueInDictionary(columnValue, cmap, secondaryConnection, true);
                                            if (newValue != null && newValue.Trim().Length > 0)
                                            {
                                                columnValue = "\'" + newValue + "\'";
                                            }
                                            else
                                            {
                                                columnValue = "NULL";
                                            }
                                            clauseParameters += columnValue + ",";
                                        }
                                        else
                                        {

                                            if (cmap.importDataType.Equals(ImportDataMap.NUMERICDATATYPE))
                                            {
                                                if (columnValue.Equals("-") || columnValue.Trim().Equals(""))
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
                                                if (cmap.targetColumnName.Trim().Equals("Description"))
                                                {
                                                    if (columnValue != null && columnValue.Length > 254)
                                                    {
                                                        columnValue = columnValue.Substring(0, 254);
                                                        mos.AddWarningMessage("Description too long, truncated to 255 characters.  Line " + linesRead);

                                                    }
                                                    clauseParameters += "\'" + columnValue.Replace("\'","\'\'") + "\',";
                                                }
                                                else
                                                {

                                                    if (columnValue.Equals("-") || columnValue.Equals(""))
                                                    {
                                                        if (cmap.defaultValue != null && cmap.defaultValue.Length > 0)
                                                        {
                                                            columnValue = cmap.defaultValue;
                                                        }


                                                        clauseParameters += "\'" + columnValue.Replace("\'", "\'\'") + "\',";
                                                    }
                                                    else
                                                    {
                                                        clauseParameters += "\'" + columnValue.Replace("\'", "\'\'") + "\',";
                                                    }
                                                }
                                                //if (columnValue.Equals("-"))
                                                //{
                                                //    if (cmap.defaultValue != null && cmap.defaultValue.Length > 0)
                                                //    {
                                                //        columnValue = cmap.defaultValue;
                                                //    }

                                                //}

                                            }
                                        }
                                    }
                                }
                                #endregion
                                // now just a hack to remove the final coma from the query
                                clauseParameters = clauseParameters.Substring(0, clauseParameters.Length - 1);
                                clauseValues = clauseValues.Substring(0, clauseValues.Length - 1);

                                string commandText = statementPart1 + "(" + clauseValues + ") VALUES (" + clauseParameters + ")";
                                //SqlCommand sqc = new SqlCommand(commandText, connection, trans);
                                SqlCommand sqc = new SqlCommand(commandText, connection);

                                numberOfHolesAdded++;
                                if (commitToDB)
                                {
                                    try
                                    {
                                        sqc.ExecuteNonQuery();
                                        UpdateStatus("Adding record from line " + linesRead, progress);
                                    }
                                    catch (Exception ex)
                                    {
                                        mos.AddErrorMessage("Failed to insert items on line " + linesRead + ".");
                                        UpdateStatus("Failed to insert items on line " + linesRead, progress);
                                        mos.recordsFailed++;
                                        mos.finalErrorCode = ModelImportStatus.ERROR_WRITING_TO_DB;
                                    }
                                }
                                tb++;
                                //if (tb == transactionBatchLimit)
                                //{
                                //    // commit batch, then renew the transaction
                                //    if (commitToDB)
                                //    {
                                //        trans.Commit();
                                //        numCommits++;
                                //        //   trans = null;
                                //        trans = connection.BeginTransaction(System.Data.IsolationLevel.Snapshot);
                                //    }
                                //    // reset counter
                                //    tb = 0;
                                //}
                            }
                            ct++;
                        }
                    }
                    if (tb > 0)
                    {
                        //if (commitToDB)
                        //{
                        //    trans.Commit();
                        //}
                        numCommits++;
                    }
                    mos.recordsAdded = numberOfHolesAdded;
                    UpdateStatus("Finished writing records to database ", 100.0);
                }
                catch (Exception ex)
                {
                    UpdateStatus("Error writing records to database ", 0);
                    mos.AddErrorMessage("Error writing records data at line " + linesRead + ":\n" + ex.ToString());
                    mos.finalErrorCode = ModelImportStatus.ERROR_WRITING_TO_DB;
                }
                finally
                {
                    try
                    {
                        connection.Close();
                        secondaryConnection.Close();
                        fileStream.Close();
                    }
                    catch (Exception ex)
                    {
                        mos.AddErrorMessage("Error closing conenction to database:\n" + ex.ToString());
                        mos.finalErrorCode = ModelImportStatus.ERROR_WRITING_TO_DB;
                    }
                }


                if (duplicateFound == true && overwrite == true)
                {
                    UpdateStatus("Begin overwrite of duplicated data ", 0);
                    OverwriteLithoRecord(mos, rejectedLines, importMap, connectionString, NKDProjectID, UpdateStatus, holeWarningMessages);
                }
                foreach (KeyValuePair<string, string> kvp in holeWarningMessages)
                {
                    string v = kvp.Value;
                    mos.AddWarningMessage(v);
                }

                mos.linesReadFromSource = linesRead;
            }
         
        }

        private void OverwriteLithoRecord(ModelImportStatus mos, Dictionary<Guid, List<string>> rejectedLines, ImportDataMap importMap, string connectionString, Guid NKDProjectID, Action<string, double> UpdateStatus, Dictionary<string, string> holeWarningMessages)
        {
            SqlConnection connection = null;
            SqlConnection secondaryConnection = null;
            try
            {
                connection = new SqlConnection(connectionString);
                connection.Open();
                secondaryConnection = new SqlConnection(connectionString);
                secondaryConnection.Open();
                int numCommits = 0;
                SqlTransaction trans;
                trans = connection.BeginTransaction();
                List<SqlCommand> commands = new List<SqlCommand>();
                int tb = 0;
                int transactionBatchLimit = 10;
                // open the filestream and read the first line              
                float bct = 1;
                // report every X blocks
                int repCount = 0;
                //int reportOnBlock = 1000;
                float fNumLines = (float)rejectedLines.Count();

                float progress = 0;
                // get the column containing the hole name 
                ColumnMap cmapHeader = importMap.FindItemsByTargetName("HeaderID");

                int headerIDX = cmapHeader.sourceColumnNumber;
                int numberOfHolesAdded = 0;
                int linesRead = 0;
                int ct = 1;

                // get all fo the header IDs in one go before we try the insert

                Dictionary<string, Guid> holeIDLookups = CollarQueries.FindHeaderGuidsForProject(NKDProjectID);


                foreach (KeyValuePair<Guid,List<string>> kvp in  rejectedLines){
                    Guid surveyGUID = kvp.Key;
                    List<string> columnData = kvp.Value;


                    linesRead++;
                    repCount++;
                    bct++;
                    progress = ((float)bct / fNumLines) * 100.0f;

                    string statementPart1 = "UPDATE " + importMap.mapTargetPrimaryTable + " ";
                    string clauseValues = "";


                    // using the column map, pick out the hole name field and see if it is in the database already
                    string headerNameItem = columnData[headerIDX];
                    string headerGUID = "";
                    bool lv = holeIDLookups.ContainsKey(headerNameItem);
                    if (!lv)
                    {
                        // oops - no hole ID with this name - should not happen though!!
                    }
                    else
                    {
                        Guid holeGuid = new Guid();
                        holeIDLookups.TryGetValue(headerNameItem, out holeGuid);
                        headerGUID = holeGuid.ToString();
                    }

                    #region mappsearch
                    // now pick out all the mapped values
                    foreach (ColumnMap cmap in importMap.columnMap)
                    {
                        bool isFKColumn = cmap.hasFKRelation;
                        int colID = cmap.sourceColumnNumber;
                        string columnValue = cmap.defaultValue;
                        if (colID >= 0)
                        {
                            columnValue = columnData[colID];
                        }

                        string targetCol = cmap.targetColumnName;
                        // ignore mapped hole name and project ID columns
                        if (targetCol.Trim().Equals("HeaderID"))
                        {
                            continue;
                        }
                        string targetTable = cmap.targetColumnTable;

                        clauseValues += "" + targetTable + "." + targetCol + "=";


                        if (isFKColumn)
                        {
                            // go and search for the appropriate value from the foreign key table
                            string newValue = ForeignKeyUtils.FindFKValueInDictionary(columnValue, cmap, secondaryConnection, true);
                            if (newValue != null && newValue.Trim().Length > 0)
                            {
                                columnValue = "\'" + newValue + "\'";
                            }
                            else {
                                columnValue = "NULL";
                            }
                        }
                        else
                        {

                            if (cmap.importDataType.Equals(ImportDataMap.NUMERICDATATYPE))
                            {
                                if (columnValue.Equals("-") || columnValue.Trim().Length == 0)
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

                            }
                            else
                            {
                                if (cmap.targetColumnName.Trim().Equals("Description"))
                                {
                                    if (columnValue != null && columnValue.Length > 254)
                                    {
                                        columnValue = columnValue.Substring(0, 254);
                                        mos.AddWarningMessage("Description too long, truncated to 255 characters.  Line " + linesRead);

                                    }
                                    columnValue = "\'" + columnValue + "\'";

                                }
                                else {
                                    if (columnValue.Equals("-") || columnValue.Equals(""))
                                    {
                                        if (cmap.defaultValue != null && cmap.defaultValue.Length > 0)
                                        {
                                            columnValue = cmap.defaultValue;
                                        }


                                        columnValue = "\'" + columnValue + "\'";
                                    }
                                    else
                                    {
                                        columnValue = "\'" + columnValue + "\'";
                                    }
                                    
                                }
                                
                            }
                        }
                        clauseValues += columnValue+",";

                    }
                    #endregion
                    // now just a hack to remove the final coma from the query
                    clauseValues = clauseValues.Substring(0, clauseValues.Length - 1);

                    string commandText = statementPart1 + "SET " + clauseValues + " WHERE LithologyID=\'" + surveyGUID + "\';";
                    SqlCommand sqc = new SqlCommand(commandText, connection, trans);
                    string msg = "";
                    //holeWarningMessages.TryGetValue(headerNameItem, out msg);
                    msg = "Litho for hole " + headerNameItem + " was overwritten with new data";
                    holeWarningMessages[headerNameItem] = msg;

                    numberOfHolesAdded++;
                    if (commitToDB)
                    {
                        sqc.ExecuteNonQuery();
                    }
                    UpdateStatus("Updating " + headerNameItem, progress); 
                    tb++;
                    if (tb == transactionBatchLimit)
                    {
                        // commit batch, then renew the transaction
                        if (commitToDB)
                        {
                            trans.Commit();
                            numCommits++;
                            //   trans = null;
                            trans = connection.BeginTransaction();
                        }
                        // reset counter
                        tb = 0;
                    }

                    ct++;
                }

                if (tb > 0)
                {
                    if (commitToDB)
                    {
                        trans.Commit();
                    }
                    numCommits++;
                }
                mos.recordsUpdated = numberOfHolesAdded;
                UpdateStatus("Finished updating lithology records", 100.0);
            }

            catch (Exception ex)
            {
                UpdateStatus("Error writing lithogy to database ", 0);
                mos.AddErrorMessage("Error writing lithogy data at line " + rejectedLines.Count + ":\n" + ex.ToString());
                mos.finalErrorCode = ModelImportStatus.ERROR_WRITING_TO_DB;
            }
            finally
            {
                try
                {
                    connection.Close();
                    secondaryConnection.Close();


                }
                catch (Exception ex)
                {
                    mos.AddErrorMessage("Error closing conenction to database:\n" + ex.ToString());
                    mos.finalErrorCode = ModelImportStatus.ERROR_WRITING_TO_DB;
                }
            }

        }

       

        /// <summary>
        /// Find the Guid for the given value in the foreign table.  If it does not exist, create it.
        /// </summary>
        /// <param name="columnValue"></param>
        /// <param name="cmap"></param>
        /// <param name="connection"></param>
        /// <returns></returns>



        private List<string> parseTestLine(string ln, char delimeter)
        {
            string[] items = ln.Split(new char[] { delimeter }, StringSplitOptions.None);
            return new List<string>(items);

        }


        
    }
}
