using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using DevExpress.XtraReports.UI;

namespace NKD.Reports
{
    public partial class FilterForm : Form
    {
        private ReportStub report;
        public string Criteria;
        
        public FilterForm(string criteria)
        {
            InitializeComponent();
            report = new ReportStub();
            report.FilterString = criteria;
            report.CreateDocument();

            this.filterControl1.SourceControl = ReportStub.DataProvider.ReportAssaysResult().Tables[0];
            this.filterControl1.FilterString = criteria;
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            this.Criteria = filterControl1.FilterString;
            this.Close();
        }
    }
}