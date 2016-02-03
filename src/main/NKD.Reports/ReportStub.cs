using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;
using System.Linq;
using System.Collections.Generic;
using DevExpress.XtraReports.Parameters;
using System.Data;
using System.Transactions;
using System.Data.SqlClient;

namespace NKD.Reports
{
    public partial class ReportStub : Utils.TableReport , IReport
    {
        public ReportStub(): base (new DataProvider())
        {
            InitializeComponent();
        }

        public ReportStub(Utils.ITableReportDataFiller filler)
            : base(filler)
        {
            
            
            InitializeComponent();
            this.DataSourceSchema = DataProvider.ReportAssaysResult().GetXmlSchema();
            this.BeginUpdate();
            this.EndUpdate();
            //Name = ReportNames.TableReport;
            //DisplayName = ReportNames.TableReport;
        }

        public Dictionary<string, string> Filter
        {
            get
            {
                return null;
            }
            set { throw new NotImplementedException(); }
        }

        protected override void BeforeReportPrint()
        {
            base.BeforeReportPrint();
        }
        
        public class DataProvider : Utils.ITableReportDataFiller
        {
            public DataProvider()
                : base()
            {
            }
          
            public static DataSet ReportAssaysResult()
            {

                DataSet ds = new DataSet("ReportResult");

                using (new TransactionScope(TransactionScopeOption.Suppress))
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString);
                    //////Get Curves
                    cmd.CommandText = "[X_SP_GetAssaysByWorkflow]";
                    //cmd.CommandText = "[dbo].[X_SP_GetAssays]";
                    cmd.CommandType = CommandType.StoredProcedure;

                    var parm1 = cmd.CreateParameter();
                    parm1.ParameterName = "@assay_group_projectid";
                    parm1.DbType = DbType.Guid;
                    parm1.Value = new Guid("31B4CCEC-A72C-4F30-A13A-32B48762FDD9");
                    //cmd.Parameters.Add(parm1);

                    try
                    {
                        //Let's actually run the queries
                        cmd.Connection.Open();
                        cmd.CommandTimeout = 600; //10 mins
                        var reader = cmd.ExecuteReader();
                        ds.Load(reader, LoadOption.OverwriteChanges, "t");
                    }
                    finally
                    {
                        cmd.Connection.Close();
                    }                    
                    return ds;
                }
            }

            public void Fill(Utils.TableReport report)
            {
                //var o = _r;
                var ds = ReportAssaysResult();
                report.DataSource = ds;
                //report.Parameters["ParameterProjectID"].Value = o.ProjectID;
                //report.Parameters["ParameterUserName"].Value = o.ReportExecutedByUserName;
              
                //var tr1 = new XRTableRow { Name = "tr1" };
                //tr1.Cells.AddRange((from k in ds.Tables[0].Columns.Cast<DataColumn>() select new XRTableCell { Name = k.ColumnName, Text = k.ColumnName }).ToArray());
                //var h = new XRTable { Name = "h", LocationF = new DevExpress.Utils.PointFloat(0F, 0F), SizeF = new System.Drawing.SizeF(600.0000F, 25F), KeepTogether = true };
                //h.Rows.AddRange(new DevExpress.XtraReports.UI.XRTableRow[] { tr1 });
                //report.Bands["Detail"].Controls.Add(h);
              
                //var tr2 = new XRTableRow { Name = "tr2" };
                //tr2.Cells.AddRange((from k in ds.Tables[0].Columns.Cast<DataColumn>() select new XRTableCell { Name = string.Format("{0}_data", k.ColumnName), Text = string.Format("[{0}]",k.ColumnName) }).ToArray());
                //var t = new XRTable { Name = "t", LocationF = new DevExpress.Utils.PointFloat(0F, 0F), SizeF = new System.Drawing.SizeF(600.0000F, 25F), KeepTogether= true };
                //t.Rows.AddRange(new DevExpress.XtraReports.UI.XRTableRow[] { tr2 });                
                //((DetailReportBand)report.Bands["DetailReport"]).Bands["DetailBand"].Controls.Add(t);
              
            }
        }

    }
}
