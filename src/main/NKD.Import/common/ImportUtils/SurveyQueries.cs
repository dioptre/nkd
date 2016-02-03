using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NKD.Module.BusinessObjects;

namespace NKD.Import.ImportUtils
{
    public class SurveyQueries
    {
      


        internal List<Guid> CheckForDuplicate(Guid holeID, decimal depth, NKDC eo)
        {

            List<Guid> results = new List<Guid>();
            
            IQueryable<Survey> res = eo.Surveys.Where(c => c.HeaderID == holeID && c.Depth == depth);
            foreach (Survey xs in res) {

                
                results.Add(xs.SurveyID);
            }

            return results;
        }

        internal List<Guid> CheckForDuplicate(Guid holeID, decimal dt, System.Data.SqlClient.SqlConnection secondaryConnection)
        {
            string statement1 = "SELECT SurveyID FROM X_Survey WHERE HeaderID=\'" + holeID.ToString() + "\' AND Depth = " +dt  + ";";
            
            SqlCommand sqc = new SqlCommand(statement1, secondaryConnection);
            SqlDataReader reader = sqc.ExecuteReader();
            List<Guid> results = new List<Guid>();
           
            while (reader.Read())
            {
                string fkName = reader[0].ToString();
                results.Add(new Guid(fkName));
                
                
            }
            reader.Close();
            sqc.Dispose();
            return results;
        }
    }
}
