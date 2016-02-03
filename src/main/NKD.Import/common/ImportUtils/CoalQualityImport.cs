using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NKD.Module.BusinessObjects;
using NKD.Import.FormatSpecification;
using System.Text.RegularExpressions;
using EntityFramework.Extensions;

namespace NKD.Import.ImportUtils
{
    public class CoalQualityImport
    {
        int WorkflowProcedureSequenceNumber = 1;

        public CoalQualityImport() { }

        internal void AddCoalQualityData(ModelImportStatus mos, Stream fileStream, FormatSpecification.ImportDataMap importMap,
                                    int batchSize, Action<string, double> UpdateStatus, int approxNumLines, 
                                    string connectionString, Guid NKDProjectID, bool checkForDuplicates, bool doImportOverwrite)
        {
            WorkflowProcedureSequenceNumber = 1;
            Guid? lastHoleID = new Guid();
            string lastStage = "";
            decimal lastFromDepth = -999999;
            decimal lastToDepth = -999999;
            bool commitToDB = true;
            DateTime currentUpdateTimestamp = DateTime.UtcNow;
            // first set up an assay group object - we can do this through the edm
            using (var entityObj = new NKDC(connectionString, null))
            {
                Guid agGuid = Guid.NewGuid();
                AssayGroup ag = new AssayGroup();
                ag.AssayGroupID = agGuid;
                ag.ProjectID = NKDProjectID;
                ag.AssayGroupName = "Manual import";
                ag.Comment = "From file " + importMap.mapOriginalDataFile;
                ag.Entered = currentUpdateTimestamp;
                ag.VersionUpdated = currentUpdateTimestamp;
                entityObj.AssayGroups.AddObject(ag);
                if (commitToDB)
                {
                    entityObj.SaveChanges();
                }

                // set up the assay test columns - one of these for each test type
                Dictionary<ColumnMap, Guid> resultsColumns = new Dictionary<ColumnMap, Guid>();
                Dictionary<Guid, AssayGroupTest> assayGroups = new Dictionary<Guid, AssayGroupTest>();
                ColumnMap cmProgram = null;
                ColumnMap cmStage = null;
                ColumnMap cmSizeFraction = null;
                ColumnMap cmWashFraction = null;
                foreach (ColumnMap cim in importMap.columnMap)
                {
                    if (cim.targetColumnName.Trim().StartsWith("[RESULT"))
                    {
                        // this is a test category
                        resultsColumns.Add(cim, Guid.NewGuid());
                    }
                    else if (cim.targetColumnName.Trim().StartsWith("[PROGRAM"))
                    {
                        cmProgram = cim;
                    }
                    else if (cim.targetColumnName.Trim().StartsWith("[STAGE"))
                    {
                        cmStage = cim;
                    }
                    else if (cim.targetColumnName.Trim().StartsWith("[SIZE FRACTION"))
                    {
                        cmSizeFraction = cim;
                    }
                    else if (cim.targetColumnName.Trim().StartsWith("[WASH FRACTION"))
                    {
                        cmWashFraction = cim;
                    }
                    
                }
                UpdateStatus("Setting up assay tests ", 2);

                foreach (KeyValuePair<ColumnMap, Guid> kvp in resultsColumns)
                {
                    ColumnMap cm = kvp.Key;
                    Guid g = kvp.Value;
                    AssayGroupTest xt = new AssayGroupTest();

                    string ss1 = "";
                    if (cm.sourceColumnName != null && cm.sourceColumnName.Length > 15)
                    {
                        ss1 = cm.sourceColumnName.Substring(0, 16);
                    }
                    else
                    {
                        ss1 = cm.sourceColumnName;
                    }
                    Guid pid = FindParameter("AssayTypeName", cm.sourceColumnName);
                    xt.ParameterID = pid;
                    xt.AssayTestName = ss1;
                    xt.AssayGroupID = agGuid;
                    xt.AssayGroupTestID = g;
                    xt.VersionUpdated = currentUpdateTimestamp;
                    entityObj.AssayGroupTests.AddObject(xt);
                    assayGroups.Add(g, xt);
                    if (commitToDB)
                    {
                        entityObj.SaveChanges();
                    }
                }



                // iterate through the data lines
                int ct = 1;
                int linesRead = 0;
                SqlConnection connection = null;
                SqlConnection secondaryConnection = null;
                //List<string> uniqueDomains = new List<string>();
                // get a connection to the database
                try
                {
                    connection = new SqlConnection(connectionString);
                    connection.Open();

                    secondaryConnection = new SqlConnection(connectionString);
                    secondaryConnection.Open();
                    bool hasDuplicateIntervals = false;

                    SqlTransaction trans;
                    trans = connection.BeginTransaction();
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
                    float pct = 0;
                    float bct = 1;

                    // report every X records
                    int repCount = 0;
                    float fNumLines = (float)approxNumLines;


                    Dictionary<string, Guid> holeIDLookups = new Dictionary<string, Guid>();
                    Dictionary<string, int> columnIDX = new Dictionary<string, int>();
                    int fkLookupCount = 0;



                    BaseImportTools.PopulateCMapShortcut(importMap, columnIDX);
                    ColumnMap headerCmap = importMap.FindItemsByTargetName("HeaderID");
                    int seqNum = 1;
                    if (sr != null)
                    {
                        while ((line = sr.ReadLine()) != null)
                        {

                            repCount++;

                            pct = ((float)linesRead / (float)approxNumLines) * 100.0f;
                            bct++;
                            linesRead++;
                            if (ct >= importMap.dataStartLine)
                            {

                                // digest a row of input data 
                                List<string> items = BaseImportTools.ParseTestLine(line, importMap.inputDelimiter);


                                Guid holeID = new Guid();
                                Decimal fromDepth = new Decimal(-9999999999);
                                Decimal toDepth = new Decimal(-9999999999);
                                string sampleNumber = null;
                                string sampleName = null;
                                string labBatchNumber = null;
                                string labsampleNumber = null;

                                // find mapped values by name
                                int idxVal = 0;
                                // -- Get the hole ID foreign key relation
                                bool foundEntry = columnIDX.TryGetValue("HeaderID", out idxVal);
                                bool foundHole = false;
                                string holeName = "";
                                if (foundEntry)
                                {

                                    string lookupByName = "HoleName";
                                    string lookupValue = items[idxVal];
                                    holeName = lookupValue;
                                    bool lv = holeIDLookups.ContainsKey(lookupValue);
                                    if (!lv)
                                    {
                                        string headerGUID = ForeignKeyUtils.FindFKValueInOther(lookupValue, headerCmap, secondaryConnection, false, lookupByName, NKDProjectID);
                                        if (headerGUID == null)
                                        {
                                            // this means we have not found the specified records in the header table
                                            // Report on issue and skip line


                                        }
                                        else
                                        {
                                            foundHole = true;
                                            holeID = new Guid(headerGUID);
                                            holeIDLookups.Add(lookupValue, holeID);
                                            fkLookupCount++;
                                        }
                                    }
                                    else
                                    {
                                        holeIDLookups.TryGetValue(lookupValue, out holeID);
                                        foundHole = true;
                                    }


                                }

                                if (!foundHole)
                                {

                                    mos.AddErrorMessage("Failed to find hole " + holeName + ".  Skipping record at line " + linesRead + ".");
                                    mos.finalErrorCode = ModelImportStatus.DATA_CONSISTENCY_ERROR;
                                    mos.recordsFailed++;
                                    continue;
                                }
                                else
                                {
                                    bool hasFrom = false;
                                    idxVal = 0;
                                    foundEntry = columnIDX.TryGetValue("FromDepth", out idxVal);
                                    if (foundEntry)
                                    {
                                        string ii = items[idxVal];
                                        Decimal val = 0;
                                        bool isOk = Decimal.TryParse(ii, out val);
                                        if (isOk)
                                        {
                                            fromDepth = val;
                                            hasFrom = true;
                                        }
                                    }

                                    bool hasTo = false;
                                    idxVal = 0;
                                    foundEntry = columnIDX.TryGetValue("ToDepth", out idxVal);
                                    if (foundEntry)
                                    {
                                        string ii = items[idxVal];
                                        Decimal val = 0;
                                        bool isOk = Decimal.TryParse(ii, out val);
                                        if (isOk)
                                        {
                                            toDepth = val;
                                            hasTo = true;
                                        }
                                    }

                                    

                                 

                                    idxVal = 0;
                                    foundEntry = columnIDX.TryGetValue("SampleID", out idxVal);
                                    if (foundEntry)
                                    {
                                        string ii = items[idxVal];
                                        sampleNumber = ii;

                                    }
                                    idxVal = 0;
                                    foundEntry = columnIDX.TryGetValue("SampleName", out idxVal);
                                    if (foundEntry)
                                    {
                                        string ii = items[idxVal];
                                        sampleName = ii;

                                    }
                                    idxVal = 0;
                                    foundEntry = columnIDX.TryGetValue("LabSampleName", out idxVal);
                                    if (foundEntry)
                                    {
                                        string ii = items[idxVal];
                                        labsampleNumber = ii;

                                    }

                                    idxVal = 0;
                                    foundEntry = columnIDX.TryGetValue("LabBatchNumber", out idxVal);
                                    if (foundEntry)
                                    {
                                        string ii = items[idxVal];
                                        labBatchNumber = ii;
                                    }

                                    // Now iddentify the  program, Stage, Size fraction and wash fraction

                                    // get the program text
                                    string programType = null;
                                    if (cmProgram != null)
                                    {
                                        programType = items[cmProgram.sourceColumnNumber];
                                    }
                                    string stage = null;
                                    if (cmStage != null)
                                    {
                                        stage = items[cmStage.sourceColumnNumber];
                                    }
                                    string sizeFraction = null;
                                    if (cmSizeFraction != null)
                                    {
                                        sizeFraction = items[cmSizeFraction.sourceColumnNumber];
                                    }
                                    string washFraction = null;
                                    if (cmWashFraction != null)
                                    {
                                        washFraction = items[cmWashFraction.sourceColumnNumber];
                                    }

                                    IQueryable<AssayGroupSubsample> toUpdate = null;
                                    bool isDuplicate = false;
                                    var washID = (from o in entityObj.Parameters where o.ParameterType=="AssayPrecondition" && o.ParameterName=="Wash fraction" select o.ParameterID).FirstOrDefault();
                                    var sizeID = (from o in entityObj.Parameters where o.ParameterType == "AssayPrecondition" && o.ParameterName == "Size fraction" select o.ParameterID).FirstOrDefault();
                                    if (checkForDuplicates)
                                    {
                                        if (hasFrom && hasTo)
                                        {
                                            // here we need to check that not duplicated
                                            toUpdate =
                                                (from o in entityObj.AssayGroupSubsamples
                                                 where
                                                    o.OriginalSample.HeaderID == holeID
                                                    && o.OriginalSample.FromDepth == fromDepth
                                                    && o.OriginalSample.ToDepth == toDepth
                                                    && o.AssayGroupWorkflowProcedure.WorkflowStateName == stage
                                                    && o.AssayGroupWorkflowProcedure.AssayGroupWorkflow.WorkflowName == programType
                                                    && (sizeFraction.Trim() == "" || o.AssayGroupSubsamplePrecondition.Any(f => f.PreconditionName == sizeFraction && f.PreconditionParameterID==sizeID))
                                                    && (washFraction.Trim() == "" || o.AssayGroupSubsamplePrecondition.Any(f => f.PreconditionName == washFraction && f.PreconditionParameterID==washID))
                                                 select o);


                                            if (toUpdate.Any())
                                            {
                                                isDuplicate = true;
                                            }
                                        }
                                        if (isDuplicate)
                                        {
                                            hasDuplicateIntervals = true;
                                            mos.AddWarningMessage("Duplicate interval for hole " + holeName + " at depth " + fromDepth + " to " + toDepth);
                                            UpdateStatus("Duplicate interval at " + holeName + " " + fromDepth + ", " + toDepth, pct);
                                            if (!doImportOverwrite)
                                            {
                                                mos.recordsFailed++;
                                                continue;
                                            }
                                            else
                                            {
                                                foreach (var upd in toUpdate)
                                                    upd.Sequence = seqNum;
                                            }
                                        }
                                    }

                                    Sample xs = null;
                                    if (isDuplicate == true)
                                    {
                                        xs = toUpdate.First().OriginalSample;
                                    }
                                    else
                                    {
                                        xs = (from o in entityObj.Samples where o.HeaderID==holeID && o.FromDepth==fromDepth && o.ToDepth==toDepth select o).FirstOrDefault();
                                        if (xs == null)
                                        {
                                            xs = new Sample();
                                            xs.SampleID = Guid.NewGuid();
                                            xs.SampleName = sampleName;
                                            xs.SampleNumber = sampleNumber;
                                            xs.FromDepth = fromDepth;
                                            xs.ToDepth = toDepth;
                                            xs.HeaderID = holeID;
                                            xs.VersionUpdated = currentUpdateTimestamp;
                                            entityObj.Samples.AddObject(xs);
                                        }
                                    }
                              

                                    // see if the interfal has changed, wherby we will need to reset the sequence ID
                                    if (holeID != lastHoleID)
                                    {
                                        if (fromDepth != lastFromDepth && toDepth != lastToDepth)
                                        {
                                            // new interval
                                            WorkflowProcedureSequenceNumber = 1;
                                        }
                                        
                                    }
                                    if (!stage.Trim().Equals(lastStage))
                                    {
                                        WorkflowProcedureSequenceNumber = 1;
                                    }
                                    lastHoleID = holeID;
                                    lastFromDepth = fromDepth;
                                    lastToDepth = toDepth;
                                    lastStage = stage;
                                    if (!isDuplicate)
                                    {
                                        AssayGroupWorkflow agWorkflowProgram = GetAssayGroupWorkflow(entityObj, programType, agGuid);
                                        AssayGroupWorkflowProcedure agWorkflowStage = GetAssayGroupWorkflowProcedure(entityObj, stage, agWorkflowProgram);
                                        AssayGroupSubsample agSS = new AssayGroupSubsample();
                                        agSS.AssayGroupID = agGuid;
                                        agSS.FromDepth = fromDepth;
                                        agSS.ToDepth = toDepth;
                                        agSS.Sequence = seqNum;
                                        agSS.AssayGroupSubsampleID = Guid.NewGuid();
                                        agSS.SampleAntecedentID = xs.SampleID;
                                        agSS.OriginalSample = xs;
                                        agSS.AssayGroupWorkflowProcedureID = agWorkflowStage.AssayGroupWorkflowProcedureID;
                                        agSS.AssayGroupWorkflowProcedure = agWorkflowStage;
                                        entityObj.AssayGroupSubsamples.AddObject(agSS);
                                        entityObj.SaveChanges();
                                        AssayGroupSubsamplePrecondition agSizeFraction = GetAssayGroupPrecondition(entityObj, sizeFraction, "Size fraction", agSS.AssayGroupSubsampleID);

                                        AssayGroupSubsamplePrecondition agWashFraction = GetAssayGroupPrecondition(entityObj, washFraction, "Wash fraction", agSS.AssayGroupSubsampleID);
                                        toUpdate = (new[] { agSS }).AsQueryable();
                                        
                                    }
                                    if (isDuplicate)
                                        entityObj.SaveChanges();
                                    foreach (var upd in toUpdate.ToList())
                                    {
                                        // now pick out all the mapped values
                                        // iterate over all [ASSAY RESULT] columns
                                        foreach (KeyValuePair<ColumnMap, Guid> kvp in resultsColumns)
                                        {
                                            ColumnMap cm = kvp.Key;
                                            Guid g = kvp.Value; // this is the AssayGroupTestID
                                            AssayGroupTestResult testResult = null;
                                            Decimal result = default(decimal);
                                            string resultText = null;
                                            bool parsedOK = false;
                                            if (items.Count >= cm.sourceColumnNumber)
                                            {
                                                parsedOK = Decimal.TryParse(items[cm.sourceColumnNumber], out result);
                                                resultText = items[cm.sourceColumnNumber];
                                            }
                                            else
                                            {
                                                mos.AddWarningMessage("Line " + linesRead + " contains too few columns to read " + cm.sourceColumnName);
                                            }
                                            if (string.IsNullOrWhiteSpace(resultText))
                                                continue;
                                            if (!isDuplicate)
                                            {
                                                testResult = new AssayGroupTestResult();
                                                testResult.AssayGroupSubsampleID = upd.AssayGroupSubsampleID;
                                                testResult.AssayGroupTestResultID = Guid.NewGuid();
                                                testResult.AssayGroupTestID = g;
                                                testResult.SampleID = xs.SampleID;
                                                testResult.LabBatchNumber = labBatchNumber;
                                                entityObj.AssayGroupTestResults.AddObject(testResult);
                                                testResult.VersionUpdated = currentUpdateTimestamp;
                                                if (parsedOK)
                                                    testResult.LabResult = result;
                                                testResult.LabResultText = resultText;
                                                //testResult.LabSampleNumber = labsampleNumber;
                                                mos.recordsAdded++;
                                            }
                                            else
                                            {
                                                var tempRes = (parsedOK) ? result : default(decimal?);
                                                entityObj.AssayGroupTestResults.Where(f=>
                                                    f.AssayGroupSubsampleID == upd.AssayGroupSubsampleID
                                                    && f.AssayGroupTest.Parameter.ParameterName == cm.sourceColumnName
                                                    )
                                                    .Update((f) => new AssayGroupTestResult
                                                    {
                                                        LabResult = tempRes,
                                                        LabResultText = resultText,
                                                        VersionUpdated = currentUpdateTimestamp                                   
                                                    });
                                                mos.recordsUpdated++;
                                            }
            
                                        }
                                     
                                     
                                    }


                                    seqNum++;                                    
                                    tb++;
                                }
                            }

                            if (commitToDB)
                            {

                                if (tb == transactionBatchLimit)
                                {
                                    entityObj.SaveChanges();

                                    UpdateStatus("Writing assays to DB (" + ct + " entries)", pct);
                                    tb = 0;
                                }
                            }
                            ct++;                            
                        }
                        entityObj.SaveChanges();

                    }
                    if (hasDuplicateIntervals)
                    {
                        mos.finalErrorCode = ModelImportStatus.DATA_CONSISTENCY_ERROR;
                    }
                    string numFKLookups = "FK lookups " + fkLookupCount;
                    mos.linesReadFromSource = ct - 1;
                    UpdateStatus("Finished writing coal quality data to database.", 0);
                }
                catch (Exception ex)
                {
                    UpdateStatus("Error writing qualities to database ", 0);
                    mos.AddErrorMessage("Error writing data at line " + linesRead + ":\n" + ex.ToString());
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


                mos.linesReadFromSource = linesRead;

            }
        }

        //private AssayGroupSubsample GetAssayGroupSubSample(NKDC entityObj, Guid agGuid, Guid? workflowID, Sample originalSample, int seqNum)
        //{
            
            

            
        //    return agw;
        //}

        private AssayGroupSubsamplePrecondition GetAssayGroupPrecondition(NKDC entityObj, string preconditionName, string preconditionType, Guid ssGuid)
        {
            AssayGroupSubsamplePrecondition agw = null;
            //IQueryable<AssayGroupSubsamplePrecondition> res = entityObj.AssayGroupSubsamplePreconditions.Where(c => c.PreconditionName.Trim().Equals(preconditionName.Trim()) && c.AssayGroupSubsampleID == ssGuid);
            //foreach (AssayGroupSubsamplePrecondition xx in res)
            //{
            //    agw = xx;
            //}
            if (agw == null)
            {
                agw = new AssayGroupSubsamplePrecondition();
                if (!string.IsNullOrWhiteSpace(preconditionName))
                    agw.PreconditionName = string.Format("{0}", preconditionName).Trim();
                else 
                    return null;
                agw.AssayGroupSubsampleID = ssGuid;
                agw.AssayGroupSubsamplePreconditionID = Guid.NewGuid();
                //TODO - make this more efficient by storing the Parameters in a dicitonary so lookup is fast rather than 
                // hitting the DB for every record
                Guid gParam = this.FindParameter("AssayPrecondition", preconditionType);
                agw.PreconditionParameterID = gParam;
                //agw.PreconditionParameterID = new Guid("6f49ded6-fe9b-487f-be48-eb8c88d9beef"); //Sixe mm TODO FIX

                //+32 bigger than 32 (Size fractions)
                //-32 smaller than 32
                //+16 bigger than 16
                //-16+8 smaller than 16, bigger than 8

                //F1.45 Floatation (density floats)
                //F1.45
                //S1.70-F1.80
                //S1.80-F2.00
                //S2.00 (sinks)

                //P2 Froth duration (30 sec)
                //P3
                if (agw.PreconditionName.Length > 0) //Density
                {
                    var isNumber = new Regex(RegexUtils.REGEX_IS_NUMBER);
                    string[] numbers = agw.PreconditionName.Split( new char[] {'+', '-', ' '}, StringSplitOptions.RemoveEmptyEntries);
                    string number;
                    if (numbers.Length == 1 && isNumber.IsMatch(agw.PreconditionName))
                    {
                        agw.Precondition = Convert.ToDecimal(agw.PreconditionName);
                        if (agw.Precondition < 0m)
                            agw.PreconditionMaximum = agw.Precondition;
                        else
                            agw.PreconditionMinimum = agw.Precondition;
                    }
                    else if (numbers.Length == 2 && isNumber.IsMatch(string.Join("", numbers)))
                    {                        
                        if (agw.PreconditionName[0] == '-')
                        {
                            agw.PreconditionMaximum = Convert.ToDecimal(numbers[0]);
                            agw.PreconditionMinimum = Convert.ToDecimal(numbers[1]);
                        }
                        else
                        {
                            agw.PreconditionMaximum = Convert.ToDecimal(numbers[1]);
                            agw.PreconditionMinimum = Convert.ToDecimal(numbers[0]);
                        }
                    }
                    else if (agw.PreconditionName.Length > 1 && agw.PreconditionName[0] == '<' && isNumber.IsMatch(number = agw.PreconditionName.Substring(1)))
                    {
                        agw.PreconditionMaximum = Convert.ToDecimal(number);
                    }
                    else if (agw.PreconditionName.Length > 1 && agw.PreconditionName[0] == '>' && isNumber.IsMatch(number = agw.PreconditionName.Substring(1)))
                    {
                        agw.PreconditionMinimum = Convert.ToDecimal(number);
                    }
                    else if (preconditionType == "Wash fraction") //Todo hack
                    {
                        var sinkFloat = new Regex(RegexUtils.REGEX_IS_SINKFLOAT).Match(agw.PreconditionName.ToUpper());
                        var s = sinkFloat.Groups["sink"].Value;
                        var f = sinkFloat.Groups["float"].Value;
                        if (isNumber.IsMatch(s) || isNumber.IsMatch(f)) //check S-F
                        {
                            if (!string.IsNullOrWhiteSpace(f))
                                agw.PreconditionMaximum = Convert.ToDecimal(f);
                            if (!string.IsNullOrWhiteSpace(s))
                                agw.PreconditionMinimum = Convert.ToDecimal(s);

                        }
                        else if (!string.IsNullOrWhiteSpace((f=new Regex(RegexUtils.REGEX_IS_CUMULATIVEFLOAT).Match(agw.PreconditionName.ToUpper()).Groups["cumulative"].Value)))
                        {
                            agw.PreconditionMaximum = Convert.ToDecimal(f);
                        }
                        else
                        {
                            switch (agw.PreconditionName.ToUpper()) // Default to SI units (s) time //Todo should separate columns HACK! Inherited from XLS
                            {
                                case "P1": //5sec intervals for 120 seconds
                                    agw.PreconditionMaximum = 120m;
                                    break;
                                case "P2": //15s
                                    agw.PreconditionMinimum = 120m;
                                    agw.PreconditionMaximum = 135m;
                                    break;
                                case "P3": //30s
                                    agw.PreconditionMinimum = 135m;
                                    agw.PreconditionMaximum = 150m;
                                    break;
                                case "P4": //60s
                                    agw.PreconditionMinimum = 150m;
                                    agw.PreconditionMaximum = 210m;
                                    break;
                                case "P5": //90s
                                    agw.PreconditionMinimum = 210m;
                                    agw.PreconditionMaximum = 300m;
                                    break;
                                case "T1": //Recovery from P1
                                    agw.PreconditionMaximum = 120m;
                                    break;
                                case "T2": //Recovery from P5
                                    agw.PreconditionMinimum = 120m;
                                    agw.PreconditionMaximum = 300m;
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
                

                


                entityObj.AssayGroupSubsamplePreconditions.AddObject(agw);
                entityObj.SaveChanges();

            }
            return agw;
        }

        private AssayGroupWorkflow GetAssayGroupWorkflow(NKDC entityObj, string programType, Guid? assayGroupID)
        {
            AssayGroupWorkflow agw = null;
            IQueryable<AssayGroupWorkflow> res = entityObj.AssayGroupWorkflows.Where(c => c.WorkflowName.Trim().Equals(programType.Trim()) && c.AssayGroupID == assayGroupID);
            foreach (AssayGroupWorkflow xx in res)
            {
                agw = xx;
            }
            if (agw == null) {
                agw = new AssayGroupWorkflow();
                agw.AssayGroupID = assayGroupID;
                agw.AssayGroupWorkflowID = Guid.NewGuid();
                agw.WorkflowName = programType;                
                entityObj.AssayGroupWorkflows.AddObject(agw);
                entityObj.SaveChanges();

            }
            return agw;
        }

        private AssayGroupWorkflowProcedure GetAssayGroupWorkflowProcedure(NKDC entityObj, string stage, AssayGroupWorkflow assayGroupWorkflow)
        {
            AssayGroupWorkflowProcedure agw = null;
            //IQueryable<AssayGroupWorkflowProcedure> res = entityObj.AssayGroupWorkflowProcedures.Where(c => c.WorkflowStateName.Trim().Equals(stage.Trim()) && c.AssayGroupWorkflowID == assayGroupWorkflow.AssayGroupWorkflowID);
            //foreach (AssayGroupWorkflowProcedure xx in res)
            //{
            //    agw = xx;
            //}
            //if (agw == null)
            //{
                
                agw = new AssayGroupWorkflowProcedure();
                agw.AssayGroupWorkflowID = assayGroupWorkflow.AssayGroupWorkflowID;
                agw.AssayGroupWorkflow = assayGroupWorkflow;
                agw.AssayGroupWorkflowProcedureID = Guid.NewGuid();
                agw.WorkflowStateName = stage;
                agw.Sequence = WorkflowProcedureSequenceNumber;
                
                WorkflowProcedureSequenceNumber++;
                entityObj.AssayGroupWorkflowProcedures.AddObject(agw);
                entityObj.SaveChanges();

            //}
            return agw;
        }

        private AssayGroupTest FindExistingAssayGroupTest(string p)
        {
            using (var entityObj = new NKDC(BaseImportTools.XSTRING, null))
            {
                AssayGroupTest resAssGroup = null;
                IQueryable<AssayGroupTest> res = entityObj.AssayGroupTests.Where(c => c.AssayTestName.Trim().Equals(p.Trim()));
                foreach (AssayGroupTest xx in res)
                {
                    resAssGroup = xx;
                }
                return resAssGroup;
            }

        }


      

        private Guid FindParameter(string typeName, string pName)
        {
            Guid pid = new Guid();
            Parameter xp = new Parameter();

            using (var entityObj = new NKDC(BaseImportTools.XSTRING, null))
            {
                bool found = false;
                IQueryable<Parameter> res = entityObj.Parameters.Where(c => c.ParameterType.Equals(typeName) && c.ParameterName.Equals(pName));
                foreach (Parameter xx in res)
                {
                    found = true;
                    pid = xx.ParameterID;
                    break;
                }
                if (!found)
                {
                    Parameter pp = new Parameter();
                    pid = Guid.NewGuid();
                    pp.ParameterID = pid;
                    pp.ParameterType = typeName;
                    pp.ParameterName = pName;
                    pp.Description = pName;
                    pp.VersionUpdated = DateTime.UtcNow;
                    entityObj.Parameters.AddObject(pp);
                    entityObj.SaveChanges();
                }

                return pid;
            }
        }


    }
}
