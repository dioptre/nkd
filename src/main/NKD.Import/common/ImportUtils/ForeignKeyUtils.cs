using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NKD.Import.ColumnSpecs;
using NKD.Import.FormatSpecification;

namespace NKD.Import.ImportUtils
{
    public static class ForeignKeyUtils
    {
        const int dictLen = 12; //i.e. X_Dictionary
        public static string FindFKValueInDictionary(string columnValue, ColumnMap cmap, SqlConnection connection, bool genNewFK)
        {
            // first check for an empty lookup - no point in doing an empty lookup or inserting blanks into the dictionaries
            if (columnValue == null || columnValue.Trim().Length == 0)
            {

                return null;
            }
            bool doStandardLookupFirst = false;
            string fkTable = cmap.fkRelationTable;
            string fkColumnKey = cmap.fkRelationColumn;
            string nameLookupColumnPrediction = "";
            string stdLookupColumnPrediction = "";
            List<string> results = new List<string>();
            // if the FK table is not a standard dictionary, have a go at finding enties in the FK table
            // using typical predicatable naming
            if (fkTable == null || !fkTable.StartsWith("Dictionary"))
            {
                string lookupColName = "Standard" + fkTable.Substring(dictLen) + "Name";
                //string lookupColName = fkTable.Substring("".Length) + "Name";
                string guid = FindFKValueInOther(columnValue, cmap, connection, genNewFK, lookupColName);
                results.Add(guid);

            }
            else
            {

                string partA = fkTable.Substring("Dictionary".Length);
                nameLookupColumnPrediction = "Custom" + partA + "Name";
                stdLookupColumnPrediction = "Standard" + partA + "Name";

              
                if (doStandardLookupFirst)
                {

                    string statement1 = "SELECT " + fkColumnKey + " FROM " + fkTable + " WHERE " + stdLookupColumnPrediction + " = \'" + columnValue + "\'";

                    SqlCommand sqc = new SqlCommand(statement1, connection);
                    SqlDataReader reader = sqc.ExecuteReader();

                    while (reader.Read())
                    {
                        string fkName = reader[0].ToString();
                        results.Add(fkName);
                    }
                    reader.Close();
                }

                if (results == null || results.Count == 0)
                {

                    string statement1 = "SELECT " + fkColumnKey + " FROM " + fkTable + " WHERE " + nameLookupColumnPrediction + " = \'" + columnValue + "\'";

                    SqlCommand sqc = new SqlCommand(statement1, connection);
                    SqlDataReader reader = sqc.ExecuteReader();
                    results = new List<string>();
                    while (reader.Read())
                    {
                        string fkName = reader[0].ToString();
                        results.Add(fkName);
                    }
                    reader.Close();
                }
                if (results.Count == 0 && genNewFK == true)
                {
                    // there is no matching entry in this dictionary.  Make a new entry
                    Guid gg = Guid.NewGuid();

                    string p1 = gg.ToString();
                    results.Add(p1);

                    char[] splitters = { '-' };
                    string[] items = p1.Split(splitters);
                    string p2 = "";
                    foreach (string it in items)
                    {
                        p2 += it;
                    }
                    //string stdValMock = p2;
                    p2 = columnValue + "-" + p2;
                    if (p2.Length > 15)
                    {
                        p2 = p2.Substring(0, 15);
                    }
                    string query = "INSERT INTO " + fkTable + " (" + fkColumnKey + "," + stdLookupColumnPrediction + "," + nameLookupColumnPrediction + ") VALUES " +
                                    " (\'" + gg.ToString() + "\',\'" + p2 + "\',\'" + columnValue + "\' )";

                    SqlCommand sqc2 = new SqlCommand(query, connection);
                    sqc2.ExecuteNonQuery();

                }
            }
            string res = null;
            if (results.Count > 0) {
                res = results.First();
            }
            return res;
        }


        public static string FindFKValueInOther(string lookupValue, ColumnMap cmap, SqlConnection connection, bool genNewFK, string lookupColName, Guid NKDProjectID)
        {

            string fkTable = cmap.fkRelationTable;
            string fkColumnKey = cmap.fkRelationColumn;

            if (lookupColName == null)
            {
                lookupColName = cmap.targetColumnName;
            }


            string statement1 = "SELECT " + fkColumnKey + " FROM " + fkTable + " WHERE " + lookupColName + " = \'" + lookupValue + "\' AND ProjectID = \'"+NKDProjectID.ToString()+"\'";

            SqlCommand sqc = new SqlCommand(statement1, connection);
            SqlDataReader reader = sqc.ExecuteReader();
            List<string> results = new List<string>();
            while (reader.Read())
            {
                string fkName = reader[0].ToString();
                results.Add(fkName);
            }
            reader.Close();

            if (results.Count == 0 && genNewFK == true)
            {
            //    // there is no matching entry in this dictionary.  Make a new entry
            //    Guid gg = Guid.NewGuid();

            //    string p1 = gg.ToString();
            //    results.Add(p1);

            //    char[] splitters = { '-' };
            //    string[] items = p1.Split(splitters);
            //    string p2 = "";
            //    foreach (string it in items)
            //    {
            //        p2 += it;
            //    }
            //    string stdValMock = p2;
            //    if (p2.Length > 15)
            //    {
            //        p2 = p2.Substring(0, 15);
            //    }
            //    string query = "INSERT INTO " + fkTable + " (" + fkColumnKey + "," + stdLookupColumnPrediction + "," + nameLookupColumnPrediction + ") VALUES " +
            //                    " (\'" + gg.ToString() + "\',\'" + p2 + "\',\'" + columnValue + "\' )";

            //    SqlCommand sqc2 = new SqlCommand(query, connection);
            //    sqc2.ExecuteNonQuery();

            }
            string res = null;
            if (results.Count > 0)
            {
                res = results.First();
            }
            return res;
        }

        public static string FindFKValueInOther(string lookupValue, ColumnMap cmap, SqlConnection connection, bool genNewFK, string lookupColName)
        {

            string fkTable = cmap.fkRelationTable;
            string fkColumnKey = cmap.fkRelationColumn;

            if (lookupColName == null)
            {
                lookupColName = cmap.targetColumnName;
            }


            string statement1 = "SELECT " + fkColumnKey + " FROM " + fkTable + " WHERE " + lookupColName + " = \'" + lookupValue +"\'";

            SqlCommand sqc = new SqlCommand(statement1, connection);
            SqlDataReader reader = sqc.ExecuteReader();
            List<string> results = new List<string>();
            while (reader.Read())
            {
                string fkName = reader[0].ToString();
                results.Add(fkName);
            }
            reader.Close();

            if (results.Count == 0 && genNewFK == true)
            {
                //    // there is no matching entry in this dictionary.  Make a new entry
                Guid gg = Guid.NewGuid();

                string p1 = gg.ToString();
                results.Add(p1);

                string query = "INSERT INTO " + fkTable + " (" + fkColumnKey + ","+lookupColName+") VALUES " +
                            " (\'" + gg.ToString() + "\',\'" + lookupValue + "\' )";

                SqlCommand sqc2 = new SqlCommand(query, connection);
                sqc2.ExecuteNonQuery();

            }
            string res = null;
            if (results.Count > 0)
            {
                res = results.First();
            }
            return res;
        }



        public static List<FKSpecification> QueryForeignKeyRelationships(string connString, string tableToQuery)
        {

            List<FKSpecification> fkList = new List<FKSpecification>();
            SqlConnection connection = null;
            try
            {

                connection = new SqlConnection(connString);
                connection.Open();
                string statement1 = "SELECT f.name AS ForeignKey, OBJECT_NAME(f.parent_object_id) AS TableName," +
                                    "COL_NAME(fc.parent_object_id,fc.parent_column_id) AS ColumnName,OBJECT_NAME " +
                                    "(f.referenced_object_id) AS ReferenceTableName, COL_NAME(fc.referenced_object_id," +
                                    "fc.referenced_column_id) AS ReferenceColumnName FROM sys.foreign_keys AS f " +
                                    "INNER JOIN sys.foreign_key_columns AS fc ON f.OBJECT_ID = fc.constraint_object_id " +
                                    "Where OBJECT_NAME(f.parent_object_id) = \'" + tableToQuery + "\';";

                SqlCommand sqc = new SqlCommand(statement1, connection);
                SqlDataReader reader = sqc.ExecuteReader();
                while (reader.Read())
                {

                    string fkName = reader[0].ToString();
                    string TableName = reader[1].ToString();
                    string ColName = reader[2].ToString();
                    string ReferencedTable = reader[3].ToString();
                    string ReferencedCol = reader[4].ToString();
                    FKSpecification fks = new FKSpecification() { parentColumnName = ColName, parentTableName = TableName, childColumnName = ReferencedCol, childTableName = ReferencedTable };
                    fkList.Add(fks);

                }

                connection.Close();

            }
            catch (Exception ex)
            {

            }
            return fkList;
        }


    }
}
