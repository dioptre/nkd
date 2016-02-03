using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace NKD.Reports
{
    public class Utils
    {

        public interface ITableReportDataFiller
        {
            void Fill(TableReport report);
        }

        public partial class TableReport : DevExpress.XtraReports.UI.XtraReport
        {
            public string ParameterString { get; set; }

            #region inner classes
            class TableAdapterDataFiller : ITableReportDataFiller
            {
                public void Fill(TableReport report)
                {
                    IDataAdapter ad = report.DataAdapter as IDataAdapter;
                    DataSet ds = report.DataSource as DataSet;
                    if (ad != null && ds != null)
                        ad.Fill(ds);
                }
            }
            #endregion

            ITableReportDataFiller dataFiller;

            public TableReport() : this(new TableAdapterDataFiller()) { }
            public TableReport(ITableReportDataFiller dataFiller)
            {
                this.dataFiller = dataFiller;
            }

            protected override void BeforeReportPrint()
            {
                dataFiller.Fill(this);
                base.BeforeReportPrint();
            }

        }

    }
}
