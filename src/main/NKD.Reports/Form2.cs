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
    public partial class Form2 : Form
    {
        private ReportStub r;
        public string Criteria = string.Empty;

        public Form2(string filterString)
        {
            r = new ReportStub();
            r.FilterString = filterString;
            InitializeComponent();
            
            r.CreateDocument();
            
            printControl1.PrintingSystem = r.PrintingSystem;

        }

        private void barSubItem1_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            FilterForm filter = new FilterForm(r.FilterString);
            DialogResult res = filter.ShowDialog();
            if (res == DialogResult.OK)
            {
                r.FilterString = filter.Criteria;
                r.CreateDocument();
                this.Criteria = filter.Criteria;
            }

        }
    }
}