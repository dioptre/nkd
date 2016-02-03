using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NKD.Module.BusinessObjects;

namespace NKD.Import.ImportUtils
{

    /// <summary>
    /// Helper class for assay importing.
    /// </summary>
    public class AssayQueries
    {

        Dictionary<Guid, IQueryable<AssayGroupTestResult>> resultsCache1 = new Dictionary<Guid, IQueryable<AssayGroupTestResult>>();
        Dictionary<Guid, IQueryable<AssayGroupTest>> resultsCache2 = new Dictionary<Guid, IQueryable<AssayGroupTest>>();


        internal List<Sample> CheckForDuplicate(Guid holeID, decimal? fromDepth, decimal? toDepth)
        {
            using (var entityObj = new NKDC(BaseImportTools.XSTRING, null))
            {
                List<Sample> resultList = new List<Sample>();
                bool found = false;
                IQueryable<Sample> res = entityObj.Samples.Where(c => c.HeaderID == holeID && c.FromDepth == fromDepth && c.ToDepth == toDepth);
                if (res != null && res.Count() > 0)
                {
                    found = true;
                }
                foreach (Sample xx in res)
                {
                    found = true;
                    resultList.Add(xx);
                    break;
                }
                return resultList;
            }
        }

        internal List<AssayGroupTestResult> GetDuplicateResult(Dictionary<Guid, AssayGroupTest> assayGroups, Guid sampleID, Guid assayGroupTestID)
        {
            using (var entityObj = new NKDC(BaseImportTools.XSTRING, null))
            {
                List<AssayGroupTestResult> resultData = new List<AssayGroupTestResult>();

                AssayGroupTest xx = assayGroups[assayGroupTestID];
                string testName = xx.AssayTestName;
                Guid testParamID = (Guid)xx.ParameterID;


                IQueryable<AssayGroupTestResult> res = entityObj.AssayGroupTestResults.Where(c => c.SampleID == sampleID);
                bool foundDupe = false;
                foreach (AssayGroupTestResult xx2 in res)
                {

                    IQueryable<AssayGroupTest> res2 = entityObj.AssayGroupTests.Where(c => c.AssayGroupID == xx2.AssayGroupTestID);
                    foreach (AssayGroupTest agt in res2)
                    {
                        if (agt.ParameterID == testParamID)
                        {
                            foundDupe = true;
                            break;
                        }
                    }
                    if (foundDupe)
                    {
                        resultData.Add(xx2);
                        break;
                    }

                }
                return resultData;
            }
        }

        internal List<AssayGroupTestResult> GetDuplicateResult(Guid sampleID, string columnName)
        {
            using (var entityObj = new NKDC(BaseImportTools.XSTRING, null))
            {
                List<AssayGroupTestResult> resultData = new List<AssayGroupTestResult>();
                bool foundDupe = false;
                IQueryable<AssayGroupTestResult> res = null;
                if (resultsCache1.ContainsKey(sampleID))
                {
                    res = resultsCache1[sampleID];
                }
                else
                {
                    res = entityObj.AssayGroupTestResults.Where(c => c.SampleID == sampleID);
                    resultsCache1.Add(sampleID, res);
                }



                foreach (AssayGroupTestResult xx2 in res)
                {
                    Guid assayGroupTestOfSample = xx2.AssayGroupTestID;
                    // now query the assay groups tests for this sample
                    IQueryable<AssayGroupTest> res2 = null;
                    if (resultsCache2.ContainsKey(assayGroupTestOfSample))
                    {
                        res2 = resultsCache2[assayGroupTestOfSample];
                    }
                    else
                    {
                        res2 = entityObj.AssayGroupTests.Where(c => c.AssayGroupTestID == assayGroupTestOfSample);
                        resultsCache2.Add(assayGroupTestOfSample, res2);
                    }
                    foreach (AssayGroupTest agt in res2)
                    {

                        // these are teh assay test groups
                        if (agt.AssayTestName.Trim().Equals(columnName))
                        {
                            foundDupe = true;
                            break;
                        }
                    }

                    if (foundDupe)
                    {
                        resultData.Add(xx2);
                        break;
                    }

                }

                return resultData;
            }
        }
    }


}
