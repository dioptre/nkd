using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NKD.Import.LAS;

namespace NKD.Import.Client.Processing
{
    public class LAS2CSV
    {
        public LAS2CSV(string _exportCSVFileName) {

            exportCSVFileName = _exportCSVFileName;
            string path = exportCSVFileName;
            using (StreamWriter sw = File.AppendText(path))
            {
                string toWrite = "P_HOLEID, DEPTH, GAMMA, LINESPEED, TOOL";
                sw.WriteLine(toWrite);
            }
        }


        public string exportCSVFileName { get; set; }


        public string AddLasFileToFile(LASFile lasFile, string origFilename, char delimeter) {

            string res = null;
            // open filestream on append mode

            string path = exportCSVFileName;
            using (StreamWriter sw = File.AppendText(path))
            {

                // get the pre holeID from the filename

                
                int li = origFilename.LastIndexOf("\\");
                string tempHoleIDa = origFilename.Substring(li);
                li = tempHoleIDa.LastIndexOf(".");
                string tempHoleID = tempHoleIDa.Substring(1, li-1);
                bool process = true;
                if (tempHoleID.ToLower().StartsWith("wkr")) { 
                    // do not process - this is a regiuonal file
                    res = "Not processing " + origFilename + ". Reason: REGIONAL FILE";
                    process = false;
                }
                else if (tempHoleID.ToLower().Contains("cal"))
                {
                    // do not process - this is a regiuonal file
                    res = "Not processing " + origFilename + ". Reason: CAL FILE";
                    process = false;
                }


                foreach (string header in lasFile.columnHeaders)
                {

                }
                if (process)
                {
                    foreach (LASDataRow ldr in lasFile.dataRows)
                    {
                        string toWrite = tempHoleID + delimeter + ldr.depth;

                        foreach (double dd in ldr.rowData)
                        {
                            toWrite += delimeter + dd;
                        }
                        // add an extra delimeter to accomodate those files with line speed present
                        if (ldr.records == 1)
                        {
                            toWrite += delimeter;
                        }
                        toWrite += delimeter + lasFile.columnHeaders[0];
                        sw.WriteLine(toWrite);
                    }
                }

            }
            return res;
        
        }
    }
}
