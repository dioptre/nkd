using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NKD.Import.LAS
{
    public class LASHeaderSection
    {

        Dictionary<string, string> unit = new Dictionary<string, string>();
        Dictionary<string, string> data = new Dictionary<string, string>();
        Dictionary<string, string> description = new Dictionary<string, string>();


        public LASHeaderSection() { }

        public void ParseDataLine(string lineData) {

            int idx = lineData.IndexOf('.');
            int endIdx = lineData.IndexOf(':');
            if (idx > 0 && endIdx > 0)
            {
                string mnemonic = lineData.Substring(0, idx).Trim();
                string descriptionVal = lineData.Substring(endIdx+1).Trim();
                string tmp = lineData.Substring(idx + 1, endIdx - idx - 1);
                string unitVal = "";
                string dataVal = "";
                
                // if a char exists in the first char, take that value till the next space as the unit


                int tidx = tmp.IndexOf(' ');
                if (tidx == 0)
                {
                    // no unit recorded
                    dataVal = tmp.Trim();
                }
                else {
                    dataVal = tmp.Substring(tidx + 1).Trim();
                    unitVal = tmp.Substring(0, tidx).Trim();
                }
                string vv = "";
                bool unitFound = unit.TryGetValue(mnemonic, out vv);
                if (!unitFound)
                {
                    unit.Add(mnemonic, unitVal);
                }
                else { 
                    // this means the mnemonic exists already.  This should not happen as it is a duplicate headers
                    // however it can happne on opccasion so it needs to be handled
                    //brute force try and append numbers to the mnemonic until we catch one that does not exists, and we will use that instaead
                    int counter = 1;
                    while (true) {
                        string newMnemonic = mnemonic + "("+counter+")";
                        bool containsKey = unit.ContainsKey(newMnemonic);
                        if (!containsKey) {
                            mnemonic = newMnemonic;
                            break;
                        }
                        counter++;
                    }
                    unit.Add(mnemonic, unitVal);

                }

                bool dataFound = data.TryGetValue(mnemonic, out vv);
                if (!dataFound)
                {
                    data.Add(mnemonic, dataVal);
                }

                bool descFound = data.TryGetValue(mnemonic, out vv);
                if (!descFound)
                {
                    description.Add(mnemonic, descriptionVal);
                }




            }


        }


        internal string GetDataValueFor(string mnemonic)
        {
            string vv = null;
            bool valFound = data.TryGetValue(mnemonic, out vv);
            return vv;
        }
        internal string GetUnitValueFor(string mnemonic)
        {
            string vv = null;
            bool valFound = unit.TryGetValue(mnemonic, out vv);
            return vv;
        }
        internal string GetDescriptionValueFor(string mnemonic)
        {
            string vv = null;
            bool valFound = unit.TryGetValue(mnemonic, out vv);
            return vv;
        }
        internal List<string> GetMnemonics()
        {
            List<string> ss = new List<string>();
            foreach (KeyValuePair<string, string> k in data) { 
                ss.Add(k.Key);
            }
            return ss;
        }
    }
}
