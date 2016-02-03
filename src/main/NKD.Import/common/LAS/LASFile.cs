using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NKD.Import.LAS;

namespace NKD.Import.LAS
{
   public  class LASFile
    {
        public List<LASDataRow> dataRows;

        public List<string> columnHeaders;

        public LASHeaderSection WellInfoHeaders = new LASHeaderSection();
        public LASHeaderSection CurveHeaders = new LASHeaderSection();
        public LASHeaderSection VersionHeaders = new LASHeaderSection();
        public LASHeaderSection ParameterHeaders = new LASHeaderSection();

        //public string filename { get; set; }

        public double nullValue = -99999.0;
        public double versionValue = 2.0;
        public string versionWrap = "NO";
        public string filePath = "";
        
        public string FileName()
        {
            string fname = "";

            if (this.filePath != "")
            {
                fname = this.filePath.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
            }
            return fname;
        }

        public List<string> errorDetails { get; set; }

        internal void AddWellSectionHeaderLine(string line)
        {
            WellInfoHeaders.ParseDataLine(line);
        }

        internal void AddVersionSectionHeaderLine(string line)
        {
            VersionHeaders.ParseDataLine(line);
        }

        internal void AddCurveSectionHeaderLine(string line)
        {
            CurveHeaders.ParseDataLine(line);
        }
    
        internal void AddParameterSectionHeaderLine(string line)
        {
 	        ParameterHeaders.ParseDataLine(line);
        }

        internal string LookupWellHeaderSection(string p)
        {

            return WellInfoHeaders.GetDataValueFor(p); 
        }
    }
}
