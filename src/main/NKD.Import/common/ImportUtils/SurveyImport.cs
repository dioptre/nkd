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
    public class SurveyImport
    {

        static bool commitToDB = true;


        internal void AddSurveyData(ModelImportStatus mos, Stream fileStream, FormatSpecification.ImportDataMap importMap, int batchSize, Action<string, double> UpdateStatus, int approxNumLines, string connectionString, Guid NKDProjectID, bool overwrite, bool checkForDuplicates)
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
                SurveyQueries sq = new SurveyQueries();

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
                    ColumnMap depthCmap = importMap.FindItemsByTargetName("Depth");

                    float percentComplete = 0;

                    int headerIDX = headerCmap.sourceColumnNumber;
                    if (sr != null)
                    {
                        while ((line = sr.ReadLine()) != null)
                        {

                            repCount++;

                            percentComplete = ((float)ct / approxNumLines) * 100.0f;

                            bct++;
                            linesRead++;
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
                                    mos.finalErrorCode = ModelImportStatus.DATA_CONSISTENCY_ERROR;
                                    mos.recordsFailed++;
                                    continue;
                                }

                                if (checkForDuplicates == true && depthCmap != null)
                                {
                                    // check for duplicate depths
                                    string d = items[depthCmap.sourceColumnNumber];
                                    decimal dt = 0;
                                    bool isParsed = decimal.TryParse(d, out dt);
                                    if (isParsed)
                                    {
                                        List<Guid> rr = sq.CheckForDuplicate(holeID, dt, secondaryConnection);
                                        //List<Guid> rr = sq.CheckForDuplicate(holeID, dt, entityObj);
                                        if (rr.Count > 0)
                                        {
                                            duplicateFound = true;

                                            if (!rejectedLines.ContainsKey(rr.First()))
                                            {
                                                rejectedLines.Add(rr.First(), items);
                                                mos.AddWarningMessage("Duplicate depth found in survey data for hole " + headerNameItem + " at depth " + d + " on line " + linesRead);
                                                UpdateStatus("Duplicate depth found in survey data for hole " + headerNameItem + " at depth " + d, percentComplete);
                                            }
                                            else
                                            {
                                                mos.AddWarningMessage("Duplicate depth found in survey data file for hole " + headerNameItem + " at depth " + d + " on line " + linesRead);
                                                UpdateStatus("Duplicate depth found in survey data file for hole " + headerNameItem + " at depth " + d, percentComplete);
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
                                            columnValue = newValue;
                                            if (newValue != null && newValue.Trim().Length > 0)
                                            {
                                                clauseParameters += "\'" + columnValue + "\',";
                                            }
                                            else
                                            {
                                                clauseParameters += "NULL,";
                                            }
                                        }
                                        else
                                        {
                                            if (cmap.importDataType.Equals(ImportDataMap.NUMERICDATATYPE))
                                            {
                                                if (columnValue.Equals("-") || columnValue.Equals(""))
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
                                                //if (columnValue.Equals("-"))
                                                //{
                                                //    if (cmap.defaultValue != null && cmap.defaultValue.Length > 0)
                                                //    {
                                                //        columnValue = cmap.defaultValue;
                                                //    }

                                                //}
                                                clauseParameters += "\'" + columnValue + "\',";
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

                                    }
                                    catch (Exception ex)
                                    {
                                        mos.AddErrorMessage("Failed to insert items on line " + linesRead + ".");
                                        UpdateStatus("Failed to insert items on line " + linesRead + ".", percentComplete);
                                        mos.recordsFailed++;
                                        mos.finalErrorCode = ModelImportStatus.ERROR_WRITING_TO_DB;
                                    }
                                }
                                UpdateStatus("Updating from line " + linesRead, percentComplete);
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
                    OverwriteSurveyRecord(mos, rejectedLines, importMap, connectionString, NKDProjectID, UpdateStatus, holeWarningMessages);
                }
                foreach (KeyValuePair<string, string> kvp in holeWarningMessages)
                {
                    string v = kvp.Value;
                    mos.AddWarningMessage(v);
                }

                mos.linesReadFromSource = linesRead;
            }
         
        }

        private void OverwriteSurveyRecord(ModelImportStatus mos, Dictionary<Guid, List<string>> rejectedLines, ImportDataMap importMap, string connectionString, Guid NKDProjectID, Action<string, double> UpdateStatus, Dictionary<string, string> holeWarningMessages)
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
                            columnValue = newValue;
                            columnValue = newValue;
                            if (newValue != null && newValue.Trim().Length > 0)
                            {
                                clauseValues += "\'" + columnValue + "\',";
                            }
                            else
                            {
                                clauseValues += "NULL,";
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
                                columnValue += "\'" + columnValue + "\',";
                            }

                            clauseValues += columnValue + ",";
                        }

                    }
                    #endregion
                    // now just a hack to remove the final coma from the query
                    clauseValues = clauseValues.Substring(0, clauseValues.Length - 1);

                    string commandText = statementPart1 + "SET " + clauseValues + " WHERE SurveyID=\'" + surveyGUID + "\';";
                    SqlCommand sqc = new SqlCommand(commandText, connection, trans);
                    string msg = "";
                    //holeWarningMessages.TryGetValue(headerNameItem, out msg);
                    msg = "Survey for hole " + headerNameItem + " (" + clauseValues + ") was overwritten with new data";
                    holeWarningMessages[headerNameItem] = msg;

                    numberOfHolesAdded++;
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
                UpdateStatus("Finished writing collars to database ", 100.0);
            }

            catch (Exception ex)
            {
                UpdateStatus("Error writing collars to database ", 0);
                mos.AddErrorMessage("Error writing collar data at line " + rejectedLines.Count + ":\n" + ex.ToString());
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
