using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NKD.Import.FormatSpecification;
using NKD.Import.ColumnSpecs;
using NKD.Import.Client.IO;
using NKD.Import.Client.DataRecords;

namespace NKD.Import.Client.UI
{
    /// <summary>
    /// Interaction logic for ImportPreview.xaml
    /// </summary>
    public partial class ImportPreview : UserControl
    {
        public event EventHandler ColumnsMapped;


        public ImportPreview()
        {
            isFileLoaded = false;
            targetMainDataType = "X_BlockModel";
            InitializeComponent();
            TestGrid();
        }

        List<ColumnMetaInfo> bmRequiredFields = new List<ColumnMetaInfo>();
        List<ColumnMetaInfo> bmOptionalFields = new List<ColumnMetaInfo>();
        
        Dictionary<string, int> columnNames = new Dictionary<string, int>();
        bool firstLineIsHeader = true;
        List<DataRecords.RawDataRow> dtTMP = null;


        void TestGrid()
        {

        

        }

        public void ResetMapping(){

            
            IList<DataGridColumn> columns = PreviewGrid.Columns;
            
            foreach (DataGridColumn dgc in columns)
            {
                ComboBox tmp = (ComboBox)dgc.Header;
                tmp.SelectedValue = ""; ;
                
            }
        }

        public void SetPreviewType(string val) {
            gpl.Children.Clear();
            gpl.RowDefinitions.Clear();
            int row = 0;
            foreach (ColumnMetaInfo bmf in bmRequiredFields)
            {

                RowDefinition rr = new RowDefinition();
                rr.Height = new GridLength(25);
                gpl.RowDefinitions.Add(rr);
                SolidColorBrush scb = Brushes.Red;
                if (bmf.hasFK) {
                    scb = Brushes.Gold;
                }
                CreateDynamicLabel(row,bmf.columnName, scb);


                row++;
            }
            foreach (ColumnMetaInfo bmf in bmOptionalFields)
            {
                RowDefinition rr = new RowDefinition();
                rr.Height = new GridLength(25);
                gpl.RowDefinitions.Add(rr);
                SolidColorBrush scb = Brushes.Red;
                if (bmf.hasFK)
                {
                    scb = Brushes.Gold;

                    CreateDynamicLabel(row, bmf.columnName, Brushes.Orange);
                    row++;
                }

            }
        }

        private void CreateDynamicLabel(int row, string bmf, SolidColorBrush col)
        {
            Label lab = new Label();
            lab.Content = bmf;
            lab.Foreground = col;
            lab.SetValue(Label.FontWeightProperty, FontWeights.Bold);
            Grid.SetRow(lab, row);
            gpl.Children.Add(lab);
        }

        internal void ResetTable(List<DataRecords.RawDataRow> dt, bool _firstLineIsHeader)
        {
            dtTMP = dt;
            // get the number of columns to display
            //int maxCols = 0;
            firstLineIsHeader = _firstLineIsHeader;
            /*foreach (RawDataRow r in dt)
            {
                int ct = r.dataItems.Count;
                
                maxCols = Math.Max(ct, maxCols);
            }*/

            var counter = dt.Max(x => x.dataItems.Count());

            columnNames = new Dictionary<string, int>();

            RawDataRow rdr = dt[0];
            List<string> newBMFieldList = new List<string>();

            foreach (ColumnMetaInfo ss in bmRequiredFields)
            {
                newBMFieldList.Add(ss.columnName);
            }
            foreach (ColumnMetaInfo ss in bmOptionalFields)
            {
                newBMFieldList.Add(ss.columnName);
            }


            for (int i = 0; i < counter; i++)
            {
                bool errorinColumn = false;
                var col1 = new DataGridTextColumn();

                ComboBox dropDown = new ComboBox();
                dropDown.Width = 80;
                dropDown.HorizontalAlignment = HorizontalAlignment.Stretch;
                List<string> cli = new List<string>();
                cli.Add(rdr.dataItems[i]);
                cli.Add("");
                List<string> fields = new List<string>();
                fields = newBMFieldList;
                // add in the optional fields
                foreach (string req in fields)
                {
                    cli.Add("->" + req + " (" + rdr.dataItems[i] + ")");
                }
                try
                {
                    columnNames.Add(rdr.dataItems[i], i);
                }
                catch (Exception ex)
                {
                    if (ex is ArgumentException) //i.e. column names are probably not unique, drop out with a more useful error message
                    {
                        if (!errorinColumn)
                        MessageBox.Show(String.Format("Column {0}, \"{1}\" in file: {2} is a duplicate, please ensure all column headings are unique, removing column for import. There may be other errors (showing only this one).", i, rdr.dataItems[i],
                            ((NKD.Import.Client.MainWindow)(((System.Delegate)(this.ColumnsMapped)).Target)).SelectedFile));
                        errorinColumn = true;
                    }
                }

                if (!errorinColumn)
                {
                    dropDown.SelectionChanged += new SelectionChangedEventHandler(dropDown_SelectionChanged);
                    dropDown.ItemsSource = cli;
                    dropDown.SelectedValue = "";

                    col1.Header = dropDown;

                    col1.Binding = new Binding("dataItems[" + i + "]");
                    PreviewGrid.Columns.Add(col1);
                }
            }

            PreviewGrid.ItemsSource = dt;
            isFileLoaded = true;
        }



        public int MaxColumns { get { return PreviewGrid.Columns.Count; } }

        internal string QueryColumnAssignments()
        {
            string res = "";
            int colNum = 0;
            IList<DataGridColumn> columns = PreviewGrid.Columns;
            foreach (DataGridColumn dgc in columns) {
                ComboBox tmp = (ComboBox)dgc.Header;
                string ss = (string)tmp.SelectedValue;
                if (ss != null && ss.StartsWith("->") == true) {
                    int idx = ss.IndexOf('(');
                    string s1 = ss.Substring(0, idx);
                    int idx2 = ss.IndexOf(')');
                    string s2 = ss.Substring(idx + 1, (idx2 - idx)-1);
                    res += "\nMap column "+colNum +" \'"+s2+"\' to " + s1 ;    
                }
                colNum++;
            }

            return res;
        }

        private void buttonAutoFillHeaders_Click(object sender, RoutedEventArgs e)
        {

            
            foreach (KeyValuePair<string, int> keyVal in columnNames) {
                string kv = keyVal.Key;
                int v = keyVal.Value;
                // now get the comboBox fopr the column in question
                DataGridColumn dgc = PreviewGrid.Columns[v];
                ComboBox tmp = (ComboBox)dgc.Header;
                foreach (ColumnMetaInfo vv in bmRequiredFields) {
                    SearchHeaderComboFor(kv, tmp, vv.columnName, "->"+vv.columnName+" (");
                }
                foreach (ColumnMetaInfo vv in bmOptionalFields)
                {
                    SearchHeaderComboFor(kv, tmp, vv.columnName, "->" + vv.columnName + " (");
                }

            }
            dropDown_SelectionChanged(null, null);
            //ColumnsMapped(this, new EventArgs());
        }

        private static void SearchHeaderComboFor(string kv, ComboBox tmp, string dataMineMnemonic, string nkdAlias)
        {
            if (kv.Equals(dataMineMnemonic))
            {
                foreach (string s in tmp.Items)
                {
                    if (s.StartsWith(nkdAlias))
                    {
                        tmp.SelectedValue = s;
                    }
                }
            }
        }

        internal void QueryColumnAssignments(Definitions.ModelColumnDefinitions columnDefs)
        {
            string res = "";
            int colNum = 0;
            IList<DataGridColumn> columns = PreviewGrid.Columns;
            Dictionary<string, int> defs = new Dictionary<string, int>();
            foreach (DataGridColumn dgc in columns)
            {
                ComboBox tmp = (ComboBox)dgc.Header;
                string ss = (string)tmp.SelectedValue;
                if (ss != null && ss.StartsWith("->") == true)
                {
                    int idx = ss.IndexOf('(');
                    string s1 = ss.Substring(0, idx);
                    int idx2 = ss.IndexOf(')');
                    string s2 = ss.Substring(idx + 1, (idx2 - idx) - 1);
                    res += "\nMap column " + colNum + " \'" + s2 + "\' to " + s1;
                    try
                    {
                        defs.Add(s1.Substring(2).Trim(), colNum);
                    }
                    catch (Exception ex) { 
                        // duplicate added 
                    }
                }
                colNum++;
            }

            

        }


        private int GetValFromDict(Dictionary<string, int> defs, string lookupVal) {
            int val;
            bool present = defs.TryGetValue(lookupVal, out val);
            if (!present)
            {
                if (!string.IsNullOrWhiteSpace(lookupVal) && lookupVal.IndexOf("[") == 0 && lookupVal.IndexOf("]") > 0)
                {
                    var find = lookupVal.Substring(0, lookupVal.Length - 1);
                    if (defs.Any(f => f.Key.IndexOf(find) > -1))
                        val = defs.First(f => f.Key.IndexOf(find) > -1).Value;
                    else
                        val = -1;
                }
                else
                    val = -1;
            }            
            return val;
        }

        private void dropDown_SelectionChanged(object sender, RoutedEventArgs args)
        {
            GetColumnDefs();
            ColumnsMapped(this, new EventArgs());
        }

        private ImportDataMap GetColumnDefs()
        {
            ImportDataMap impMap = new ImportDataMap();
            
            // get the selected headers
            string res = "";
            int colNum = 0;
            IList<DataGridColumn> columns = PreviewGrid.Columns;
            Dictionary<string, int> defs = new Dictionary<string, int>();
            int incrementor1 = 1;
            foreach (DataGridColumn dgc in columns)
            {
                ComboBox tmp = (ComboBox)dgc.Header;
                string ss = (string)tmp.SelectedValue;
                if (ss != null && ss.StartsWith("->") == true)
                {
                    int idx = ss.IndexOf('(');
                    string s1a = ss.Substring(2, idx-2);
                    string s1 = ss.Substring(0, idx);

                    int idx2 = ss.LastIndexOf(')');
                    string s2 = ss.Substring(idx + 1, (idx2 - idx) - 1);
                    res += "\nMap column " + colNum + " \'" + s2 + "\' to " + s1;
                    try
                    {
                        string colVal = s1.Substring(2).Trim();
                        if (colVal.StartsWith("[")) {
                            int lv = colVal.Trim().Length - 1;
                            string sx1 = colVal.Substring(0, lv);
                            sx1 += " " + incrementor1 + "]";
                            incrementor1++;
                            colVal = sx1;
                        }
                        defs.Add(colVal, colNum);
                        // get the specified type form DB for this column
                        string dbType = LookupColumnDBType(targetMainDataType, s1a, bmRequiredFields);

                        impMap.columnMap.Add(new ColumnMap(s2, colNum, targetMainDataType, s1a, dbType, "", "", ImportDataMap.UNIT_NONE));
                    }
                    catch (Exception ex)
                    {
                        // duplicate added 
                    }
                }
                colNum++;
            }


            // now search through the definitions and see which items are mapped

            gpl.Children.Clear();
            int ct = 0;
            foreach (ColumnMetaInfo rf in bmRequiredFields)
            {
                
                int col = GetValFromDict(defs, rf.columnName);
                SolidColorBrush scb = Brushes.Red;

                //if (rf.hasFK) {
                //    scb = Brushes.Gold;
                //}
                if (rf.isMandatory)
                {

                    GenerateLabel(ct, rf.columnName, col, scb);
                    ct++;
                }
            }

            foreach (ColumnMetaInfo rf in bmOptionalFields)
            {
                
                int col = GetValFromDict(defs, rf.columnName);
                SolidColorBrush scb = Brushes.Orange;

                //if (rf.hasFK)
                //{
                //    scb = Brushes.Gold;
                //}
                GenerateLabel(ct, rf.columnName, col, scb);
                ct++;
            }

            // now seartch list to perform assignmnets
           
                //columnDefs.bmX = GetValFromDict(defs, "X");
                //columnDefs.bmY = GetValFromDict(defs, "Y");
                //columnDefs.bmZ = GetValFromDict(defs, "Z");
                //columnDefs.bmXINC = GetValFromDict(defs, "X width");
                //columnDefs.bmYINC = GetValFromDict(defs, "Y width");
                //columnDefs.bmZINC = GetValFromDict(defs, "Z width");
                //columnDefs.bmZone = GetValFromDict(defs, "Domain");
                //columnDefs.bmDensity = GetValFromDict(defs, "Density");
                //columnDefs.bmGradeAttributes = new int[1];
                //columnDefs.bmGradeAttributes[0] = GetValFromDict(defs, "Variable");

                //int ct = 0;
                //foreach (string rf in bmRequiredFields)
                //{
                //    GenerateLabel(ct, rf, -1);
                //    ct++;
                //}
                //GenerateLabel(0, "X",columnDefs.bmX);
                //GenerateLabel(1, "Y", columnDefs.bmY);
                //GenerateLabel(2, "Z", columnDefs.bmZ);
                
                //GenerateLabel(3, "X width", columnDefs.bmXINC);
                //GenerateLabel(4, "Y width", columnDefs.bmYINC);
                //GenerateLabel(5, "Z width", columnDefs.bmZINC);
                //GenerateLabel(6, "Domain", columnDefs.bmZone);
                //GenerateLabel(7, "Variable", columnDefs.bmGradeAttributes[0]);

                //GenerateLabel(8, "Density", columnDefs.bmDensity);

            
            return impMap;

        }

        private string LookupColumnDBType(string targetMainDataType, string columnName, List<ColumnMetaInfo> bmRequiredFields)
        {
            string foundType = "";
            foreach (ColumnMetaInfo cmi in bmRequiredFields) {
                if (cmi.columnName.Trim().Equals(columnName.Trim())) {
                    foundType = cmi.dbTypeName;
                }
            }
            return foundType;
        }

        private bool CheckIsMapped(string rf, Dictionary<string, int> defs)
        {
            throw new NotImplementedException();
        }

        private void GenerateLabel(int rowPos, string labelName, int checkVal, SolidColorBrush failColour)
        {
            if (checkVal == -1)
            {
                CreateDynamicLabel(rowPos, labelName, failColour);
            }
            else
            {
                CreateDynamicLabel(rowPos, labelName, Brushes.Green);
            }
        }



        internal bool AreMandatoryColumnsMapped()
        {
            // ModelColumnDefinitions columnDefs = GetColumnDefs();
            bool complete = true;
            
            return complete;
        }

        internal string GetColumnNameForVariable(int i)
        {
            string res = "";
            foreach (KeyValuePair<string, int> kv in this.columnNames) {
                if (i == kv.Value) {
                    res = kv.Key;
                    break;
                }
            }
            return res;
        }

        internal void ResetData()
        {
            columnNames.Clear();
            PreviewGrid.Columns.Clear();
            PreviewGrid.ItemsSource = null;
            gpl.Children.Clear();
            gpl.RowDefinitions.Clear();
            bmRequiredFields = new List<ColumnMetaInfo>();
            bmOptionalFields = new List<ColumnMetaInfo>();
            
        }

        internal void SetMandatoryMappingColumns(List<ColumnMetaInfo> _mandatoryColumns)
        {
            bmRequiredFields = _mandatoryColumns.GroupBy(f => f.columnName).Select(g=>g.First()).ToList();
        }

        internal void SetOptionalMappingColumns(List<ColumnMetaInfo> _optionalColumns)
        {
            bmOptionalFields = _optionalColumns;
        }



        internal ImportDataMap GenerateColumnMap()
        {
            ImportDataMap impMap = GetColumnDefs();
           
            return impMap;
        }

        public bool isFileLoaded { get; set; }

        public string targetMainDataType { get; set; }

        internal FormatLoadStatus SetMappingFromImportDataMap(ImportDataMap idm)
        {
            FormatLoadStatus fms = new FormatLoadStatus();
            if (idm != null) {
                fms.LoadStatus = FormatLoadStatus.LOAD_OK;
            }
            int mapCount = 0;
            foreach(ColumnMap cm in idm.columnMap){
                string sourceColName = cm.sourceColumnName;
                string targetMappingName = cm.targetColumnName;
                int sourceColNum = cm.sourceColumnNumber;
                DataGridColumn dgc = PreviewGrid.Columns[sourceColNum];
                ComboBox tmp = (ComboBox)dgc.Header;
                int i = 0;
                bool foundMatch = false;
                foreach (string ss in tmp.Items) {
                    if (i > 1)
                    {
                        int idx = ss.IndexOf('(');
                        string s1a = ss.Substring(2, idx - 2);
                        string s1 = ss.Substring(0, idx);

                        int idx2 = ss.IndexOf(')');
                        string s2 = ss.Substring(idx + 1, (idx2 - idx) - 1);

                        if (s1a.Equals(targetMappingName))
                        {
                            tmp.SelectedIndex = i;
                            mapCount++;
                            foundMatch = true;
                            break;
                        }
                    }
                    i++;
                }
                if(foundMatch == false){
                    fms.WarningMessages.Add("No match for column "+sourceColName+" mapped to "+targetMappingName);
                }

            }

            if (mapCount != idm.columnMap.Count)
            {
                fms.MappingStatus = FormatLoadStatus.MAPPING_ASSIGNEMNT_WARNING;
                fms.MappingMessage = "Failed to apply all items from saved map to currentluy loaded data file.";
            }
            else
            {
                fms.MappingStatus = FormatLoadStatus.MAPPING_ASSIGNEMNT_OK;
            }
           
            dropDown_SelectionChanged(null, null);
            return fms;
        }
    }


      

}
