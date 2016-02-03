using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NKD.Import.LAS;

namespace NKD.Import.Client.Processing
{
    public class LASImport
    {

        public LASImport() { }

        public LASFile GetLASFile(string inputFilename,ModelImportStatus mis ) {
            LASFile lf = null;
            try
            {
                LASFileReader lfr = new LASFileReader();
                int columnOffset = 0;
                int errorCode = 0;
                 lf = lfr.ReadLASFile(inputFilename, columnOffset, out errorCode);
                
                if (errorCode == 0)
                {
                    mis.finalErrorCode = ModelImportStatus.OK;
                    string res = "";
                    foreach (string nc in lf.columnHeaders)
                    {
                        res += nc + ", ";
                    }
                    mis.linesReadFromSource = lf.dataRows.Count;
  
                    // Display error mesasges if required
                    if (lf.errorDetails != null && lf.errorDetails.Count > 0)
                    {
                        string messageBoxText = "The file (" + inputFilename + ") was loaded, but issues were noted as follows:";
                        mis.AddWarningMessage(messageBoxText);
                        foreach (string ed in lf.errorDetails)
                        {
                            string ss = "\n" + ed;
                            mis.AddWarningMessage(ss);
                        }
                        
                        
                       

                    }

                }
                else
                {

                    string messageBoxText = "The file (" + inputFilename + ") could not be loaded.  Please check that the file is " +
                                            "accessible, is not open in another application and is in the correct format.";
                    mis.AddErrorMessage(messageBoxText);
                    string errorCodeDetails = LASErrorCodes.LookupCode(errorCode);
                    if (lf != null && lf.errorDetails != null)
                    {
                        foreach (string ed in lf.errorDetails)
                        {
                            string ss = "\n" + ed;
                            mis.AddErrorMessage(ss);
                        }
                    }
                    
                    mis.finalErrorCode = ModelImportStatus.ERROR_LOADING_FILE;
                    string caption = "Error loading file " + inputFilename;
                    
                }

            }
            catch (Exception ex)
            {

            }
            return lf;
        }
    }
}
