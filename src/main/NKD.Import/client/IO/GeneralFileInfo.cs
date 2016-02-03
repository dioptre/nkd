using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NKD.Import.Client.IO
{
    public class GeneralFileInfo
    {

        public int numLines { get; set;  }


        public void GeneralFileStats(string selectedFile)
        {
            try
            {
                Stream fileStreamTmp = new FileStream(selectedFile, FileMode.Open);

                StreamReader sr = null;
                FileStream fs = null;

                //fs = new FileStream(textInputDataFile, FileMode.Open, FileAccess.Read, FileShare.Read);
                sr = new StreamReader(fileStreamTmp);


                int lineCount = 0;
               
                if (sr != null)
                {
                    while ((sr.ReadLine()) != null)
                    {
                        lineCount++;
                    }
                }

                this.numLines = lineCount;
                fileStreamTmp.Close();
            }
            catch (Exception ex)
            {

            }
        }

    }
}
