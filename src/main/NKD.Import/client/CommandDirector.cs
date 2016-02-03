using NKD.Import.Client.IO;
using NKD.Import.Client.Processing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NKD.Import;
using NKD.Import.DataWrappers;
using NKD.Import.FormatSpecification;
using NKD.Import.LAS;
using System.Data.Objects.DataClasses;
using System.Data.SqlClient;
using System.Data;
using NKD.Module.BusinessObjects;
using Microsoft.Samples.EntityDataReader;

namespace NKD.Import.Client
{
    public class CommandDirector
    {

        private BackgroundWorker backgroundWorker = null;

        private static string defaultConnectionString = "Data Source=NKDDB;Initial Catalog=NKD;Integrated Security=True";
        private static string connectionString = null;
        //TODO: Override in interface if we need to support > 1 DB
        public static string ConnectionString
        {
            get
            {
                if (connectionString == null)
                {
                    try
                    {
                        var cs = System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                        connectionString = cs;
                    }
                    catch
                    {
                        connectionString = defaultConnectionString;
                    }
                }
                return connectionString;
            }
            set { connectionString = value; }
        }

        public CommandDirector() { }


        /// <summary>
        /// Carry out the block model import
        /// </summary>
        /// <param name="SelectedBMFile"></param>
        /// <param name="SelectedFormatBMFile"></param>
        /// <param name="importMap"></param>
        /// <param name="rawFileReader"></param>
        /// <returns></returns>
        internal bool DoBMImport(string SelectedBMFile, string SelectedFormatBMFile, ImportDataMap importMap, RawFileReader rawFileReader, string NKDProjectID, string modelAlias)
        {
            BaseImportTools bit = new BaseImportTools();
            int cxColumnID = importMap.GetColumnIDMappedTo("CentroidX");
            int cyColumnID = importMap.GetColumnIDMappedTo("CentroidY");
            int czColumnID = importMap.GetColumnIDMappedTo("CentroidZ");
            
            ColumnStats xOrigin = rawFileReader.GetDimensions(cxColumnID);
            ColumnStats yOrigin = rawFileReader.GetDimensions(cyColumnID);
            ColumnStats zOrigin = rawFileReader.GetDimensions(czColumnID);

            int approxNumLines = xOrigin.count;


            Stream bmFileStream = new FileStream(SelectedBMFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
           // Stream bmFileStream = new FileStream(SelectedBMFile, FileMode.Open);
            ModelImportStatus mos = new ModelImportStatus();
            Guid newModelGuid = Guid.NewGuid();
            
            Guid authorGuid = new Guid();
            List<string> status = bit.PerformBMImport(mos, newModelGuid, bmFileStream, null, importMap, xOrigin.min, yOrigin.min, zOrigin.min, backgroundWorker, approxNumLines, NKDProjectID, modelAlias, authorGuid, ConnectionString);
            return true;
        }

        internal void SetCurrentWorkerThread(System.ComponentModel.BackgroundWorker worker)
        {
            backgroundWorker = worker;
        }

        internal ModelImportStatus DoCollarImport(string SelectedFile, string SelectedFormatBMFile, ImportDataMap importMap, RawFileReader rawFileReader, Guid NKDProjectID, bool overwrite)
        {
          
            
            BaseImportTools bit = new BaseImportTools();
            // get the current collar names in this project
            List<CollarInfo> existingHoles = this.GetHolesForProject(NKDProjectID);


            List<string> existingHoleNames = new List<string>();
            foreach (CollarInfo ci in existingHoles)
            {
                existingHoleNames.Add(ci.Name);
            }

            ModelImportStatus mos = new ModelImportStatus();
            Stream fileStream = new FileStream(SelectedFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite); 
            //Stream fileStream = new FileStream(SelectedFile, FileMode.Open);
            bit.PerformCollarImport(mos, fileStream, null, importMap, this.backgroundWorker, NKDProjectID, ConnectionString, existingHoleNames, overwrite);
            return mos;
            
        }

        internal ModelImportStatus DoAssayImport(string SelectedFile, string SelectedFormatFile, ImportDataMap importMap, RawFileReader rawFileReader, Guid NKDProjectID, bool checkForDuplicates, bool doImportOverwrite)
        {
            BaseImportTools bit = new BaseImportTools();
            ModelImportStatus mos = new ModelImportStatus();
            
            GeneralFileInfo gfi = new GeneralFileInfo();
            gfi.GeneralFileStats(SelectedFile);
            int numLines = gfi.numLines;

            Stream fileStream = new FileStream(SelectedFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            //Stream fileStream = new FileStream(SelectedFile, FileMode.Open);
            bit.PerformAssayImport(mos, fileStream, null, importMap, this.backgroundWorker, NKDProjectID, ConnectionString, numLines, checkForDuplicates, doImportOverwrite);
            return mos;
        }

        internal ModelImportStatus DoCoalQualityImport(string SelectedFile, string SelectedFormatFile, ImportDataMap importMap, RawFileReader rawFileReader, Guid NKDProjectID, bool checkForDuplicates, bool doImportOverwrite)
        {
            BaseImportTools bit = new BaseImportTools();
            ModelImportStatus mos = new ModelImportStatus();

            GeneralFileInfo gfi = new GeneralFileInfo();
            gfi.GeneralFileStats(SelectedFile);
            int numLines = gfi.numLines;

            Stream fileStream = new FileStream(SelectedFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            //Stream fileStream = new FileStream(SelectedFile, FileMode.Open);
            bit.PerformCoalQualityImport(mos, fileStream, null, importMap, this.backgroundWorker, NKDProjectID, ConnectionString, numLines, checkForDuplicates, doImportOverwrite);
            return mos;
        }

        internal ModelImportStatus DoSurveyImport(string SelectedFile, string SelectedFormatFile, ImportDataMap importMap, RawFileReader rawFileReader, Guid NKDProjectID, bool doOverwrite, bool checkForDuplicates)
        {
            BaseImportTools bit = new BaseImportTools();
            ModelImportStatus mos = new ModelImportStatus();

            GeneralFileInfo gfi = new GeneralFileInfo();
            gfi.GeneralFileStats(SelectedFile);
            int numLines = gfi.numLines;

            Stream fileStream = new FileStream(SelectedFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            //Stream fileStream = new FileStream(SelectedFile, FileMode.Open);
            bit.PerformSurveyImport(mos, fileStream, null, importMap, this.backgroundWorker, NKDProjectID, ConnectionString, numLines, doOverwrite, checkForDuplicates);
            return mos;
        }

        internal Dictionary<Guid, string> GetProjectList()
        {
            BaseImportTools bit = new BaseImportTools();
            return bit.GetProjectsList();
        }

        internal List<CollarInfo> GetHolesForProject(Guid currentSelectedProject)
        {

            BaseImportTools bit = new BaseImportTools();
            List<CollarInfo> collarNameList = bit.FindCollarsInProject(currentSelectedProject);
            return collarNameList;
        }

        internal ModelImportStatus DoLithoImport(string SelectedFile, string SelectedFormatFile, ImportDataMap importMap, RawFileReader rawFileReader, Guid NKDProjectID, bool doOverwrite, bool checkForDuplicates)
        {
            BaseImportTools bit = new BaseImportTools();
            ModelImportStatus mos = new ModelImportStatus();

            GeneralFileInfo gfi = new GeneralFileInfo();
            gfi.GeneralFileStats(SelectedFile);
            int numLines = gfi.numLines;


            //Stream fileStream = new FileStream(SelectedFile, FileMode.Open);
            Stream fileStream = new FileStream(SelectedFile, FileMode.Open, FileAccess.Read , FileShare.ReadWrite);
            bit.PerformLithoImport(mos, fileStream, null, importMap, this.backgroundWorker, NKDProjectID, ConnectionString, numLines, doOverwrite, checkForDuplicates);
            return mos;
        }

        internal ModelImportStatus BatchImportLasFiles(string[] filePaths, Guid currentProjectID)
        {

            ModelImportStatus finalStatus = new ModelImportStatus();

            LASBatchImportTools ll = new LASBatchImportTools();
            List<string> messages = new List<string>();
            int importCount = 0;
            int failCount = 0;
            string report = "";
            Dictionary<string, ModelImportStatus> mosList = new Dictionary<string, ModelImportStatus>();
            bool reportStatus = false;
            if (this.backgroundWorker != null)
            {
                reportStatus = true;
            }
            

            int fileCount = filePaths.Length;
            int thisFileNum = 0;

            var dataDict = new Dictionary<string, List<object>>();

            foreach (string file in filePaths)
            {
                var data = new List<object>();
                double pct = ((double)thisFileNum / (double)fileCount) * 100.0;
                thisFileNum++; 
               
                
                if (reportStatus)
                {
                    backgroundWorker.ReportProgress((int)pct, "Processing las file "+thisFileNum+" of "+fileCount+", "+file);
                }

                ModelImportStatus mis = new ModelImportStatus();

                NKD.Import.Client.Processing.LASImport li = new NKD.Import.Client.Processing.LASImport();
                LASFile lf = li.GetLASFile(file, mis);
                if (lf == null)
                {
                    
                    mis.errorMessages.Add("Failed to load LAS file " + file);
                    mosList.Add(file, mis);
                    continue;
                }

                data = ll.ProcessLASFile(lf, file, mis, currentProjectID, this.backgroundWorker);
                string msg = "";
                if (msg != null)
                {
                    messages.Add(msg);
                    report += msg + "\n";
                    failCount++;
                }
                else
                {
                    importCount++;
                }

                mosList.Add(file, mis);
                dataDict.Add(file, data);

                //if (thisFileNum % 2 == 0 || (filePaths.Length-thisFileNum) < 1) //FIXME magic number, should look at used memory and make a choice on that
                //{
                    //insert into DB to avoid memory issues
                    //var subdict = dataDict;
                PushToDB(dataDict);    
                dataDict = new Dictionary<string, List<object>>();

                    //PushToDB(subdict);
                    //subdict = null;
                
                GC.Collect();
                GC.WaitForPendingFinalizers();
                //}
            }

            string finalReport = "Immport status:\nFiles imported:" + importCount + "\nFailed files:" + failCount + "\n\nMessages:\n";

            finalReport += report;
            int totRecordsAddedCount = 0;
            int totLinesReadCount = 0;
            foreach (KeyValuePair<string, ModelImportStatus> kvp in mosList) {
                string lfName = kvp.Key;
                ModelImportStatus ms = kvp.Value;
                totRecordsAddedCount += ms.recordsAdded;
                totLinesReadCount += ms.linesReadFromSource;
                if (ms.finalErrorCode != ModelImportStatus.OK) {
                    finalStatus.finalErrorCode = ModelImportStatus.GENERAL_LOAD_ERROR;
                }
                foreach (string m in ms.warningMessages) {
                    finalStatus.warningMessages.Add(m);
                }
                foreach (string m in ms.errorMessages)
                {
                    finalStatus.errorMessages.Add(m);
                }
            }
            finalStatus.linesReadFromSource = totLinesReadCount;
            finalStatus.recordsAdded = totRecordsAddedCount;

            return finalStatus;

        }

        private void PushToDB(Dictionary<string, List<object>> dataDict)
        {
            //push data to DB
            foreach (var f in dataDict)
            {
                foreach (object x1 in f.Value)
                {
                    string tableType = "";
                    var bulkCopy = new SqlBulkCopy(BaseImportTools.XSTRING,SqlBulkCopyOptions.UseInternalTransaction);
                    bulkCopy.BulkCopyTimeout = 0; //dangerous really should set a vaguely decent timeout, 0 means no timeout
                    if (x1.GetType() == typeof(List<FileData>))
                    {
                        tableType = "X_FileData";
                        var tableReader = (List<FileData>)x1;
                        bulkCopy.DestinationTableName = tableType;                     
                        //bulkCopy.WriteToServer(tableReader.AsDataReader("Size"));
                    }
                    else if (x1.GetType() == typeof(List<Geophysics>))
                    {
                        tableType = "X_Geophysics";
                        var tableReader = (List<Geophysics>)x1;
                        bulkCopy.DestinationTableName = tableType;
                        bulkCopy.WriteToServer(tableReader.AsDataReader());
                    }
                    else if (x1.GetType() == typeof(List<DictionaryUnit>))
                    {
                        tableType = "X_DictionaryUnit";
                        var tableReader = (List<DictionaryUnit>)x1;
                        bulkCopy.DestinationTableName = tableType;
                        //bulkCopy.WriteToServer(tableReader.AsDataReader());
                    }
                    else if (x1.GetType() == typeof(List<Parameter>))
                    {
                        tableType = "X_Parameter";
                        var tableReader = (List<Parameter>)x1;
                        bulkCopy.DestinationTableName = tableType;
                        bulkCopy.WriteToServer(tableReader.AsDataReader());
                    }
                    else if (x1.GetType() == typeof(List<GeophysicsMetadata>))
                    {
                        tableType = "X_GeophysicsMetadata";
                        var tableReader = (List<GeophysicsMetadata>)x1;
                        bulkCopy.DestinationTableName = tableType;
                        bulkCopy.WriteToServer(tableReader.AsDataReader());
                    }
                    else if (x1.GetType() == typeof(List<GeophysicsData>))
                    {
                        tableType = "X_GeophysicsData";
                        var tableReader = (List<GeophysicsData>)x1;
                        bulkCopy.DestinationTableName = tableType;
                        try
                        {
                            bulkCopy.WriteToServer(tableReader.AsDataReader());
                        }
                        catch (IOException iox)
                        {
                            Console.WriteLine(iox.ToString());
                        }
                    }
                }
            }
        }
        internal int GetNKDVersion()
        {
            BaseImportTools bit = new BaseImportTools();
            return bit.GetNKDVersion(connectionString);
        }
    }
}
