using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NKD.Import.Client.IO
{
    public class FormatLoadStatus
    {
        public static int LOAD_OK = 0;
        public static int LOAD_FAIL = 1;
        
        public static int MAPPING_ASSIGNEMNT_OK = 0;
        public static int MAPPING_ASSIGNEMNT_FAIL = 1;
        public static int MAPPING_ASSIGNEMNT_WARNING = 2;
        


        public int LoadStatus  {get; set; }
        public int MappingStatus { get; set; }        
        public string LoadMessage { get; set; }
        public string FileName{ get; set; }

        public List<string> WarningMessages = new List<string>();

        public FormatLoadStatus() {
            LoadStatus = -1;
            MappingStatus = -1;
        }


        public string MappingMessage { get; set; }

        internal string GenerateUserMessages()
        {

            string msg = "";
            if (LoadMessage != null)
            {
                msg += LoadMessage+"\n\n";
            }
            if (MappingMessage != null)
            {
                msg += MappingMessage+"\n\n";
            }
            foreach (string ss in WarningMessages) {
                msg += ss + "\n";
            }
            return msg;
        }
    }
}
