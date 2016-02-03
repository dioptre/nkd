using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NKD.Import.LAS
{
    /// <summary>
    /// A list of error codes and descitpions associated with this application
    /// @Author Nick Anderson
    /// </summary>
    public class LASErrorCodes
    {
        // i/o errors
        public static int STREAM_OPEN_FAILED                = -1;
        public static int ERROR_PARSING_LAS_HEADERS         = -2;
        public static int NO_DATA_ROWS_LOADED               = -3;
        public static int ERROR_READING_LAS_DATA_SECTION    = -4;
        public static int PARSE_INDEERROR                 = -5;
        public static int CSV_PARSE_INDEERROR             = -6;
        public static int CSV_PARSE_ERROR                   = -7;
        public static int CSV_DEPTH_FIELD_PARSE_ERROR       = -8;
        public static int CSV_STREAM_ERROR                  = -9;
        public static int CSV_FILE_NOT_FOUND                = -10;
        public static int CSV_HEADER_PARSE_FAILED           = -11;
        public static int INSUFFICIENT_DATA_COLUMNS         = -12;
        public static int MAERRORS_ENCOUNTERED            = -13;

        public static Dictionary<int, string> descriptions = new Dictionary<int, string>();

        static  LASErrorCodes() {

            descriptions.Add(STREAM_OPEN_FAILED, "Failed to open file");
            descriptions.Add(ERROR_PARSING_LAS_HEADERS, "Error parsing headers");
            descriptions.Add(NO_DATA_ROWS_LOADED, "No data lines were loaded");
            descriptions.Add(ERROR_READING_LAS_DATA_SECTION, "Error reading the ASCII data section of the LAS file");
            descriptions.Add(PARSE_INDEERROR, "Error decoding data line");
            descriptions.Add(CSV_PARSE_INDEERROR, "Error decoding data line - not enough data items");

            descriptions.Add(CSV_PARSE_ERROR, "Error decoding data line");
            descriptions.Add(CSV_DEPTH_FIELD_PARSE_ERROR, "Unable to find depth field");
            descriptions.Add(CSV_STREAM_ERROR, "Unable to open CSV file");
            descriptions.Add(CSV_FILE_NOT_FOUND, "File not found");
            descriptions.Add(CSV_HEADER_PARSE_FAILED, "Failed to decode CSV header line");
            descriptions.Add(INSUFFICIENT_DATA_COLUMNS, "Not enough data columns");

            descriptions.Add(MAERRORS_ENCOUNTERED, "Maximum number of errors encountered.\nThe file you are loading has many errors.\nPlease check the file and try again.\n\nFor help and guidance on acceptable file types, pease click the '?' button above"); 

            
        }


        public static string LookupCode(int errorCode)
        {
            string res = "";
            descriptions.TryGetValue(errorCode, out res) ;
            return res;
        }



     
    }
}
