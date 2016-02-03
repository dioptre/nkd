
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using NKD.Import.FormatSpecificationIO;

namespace NKD.Import.Client.UI
{
    /// <summary>
    /// Interaction logic for MapTable.xaml
    /// </summary>
    public partial class MapTable : UserControl
    {

        public string bmPrimaryTableName = "X_BlockModelBlock";
        public string collarPrimaryTableName = "X_Header";
        public string geophyiscsPrimaryTableName = "X_Geophyiscs";
        public string assayPrimaryTableName = "X_AssayGroup";
        public string surveyPrimaryTableName = "X_Survey";
        public string lithoPrimaryTableName = "X_Lithology";
        public string coalqualPrimaryTableName = "X_AssayGroup";


        public static ObservableCollection<string> targetColumns { get; set; }
        public static ObservableCollection<string> sourceColumns { get; set; }
        public static ObservableCollection<string> unitTypesList { get; set; }
        public static ObservableCollection<string> dataTypeList { get; set; }

        public ObservableCollection<ColumnMap> cmaps = null;
        

        public MapTable()
        {
            sourceColumns = new ObservableCollection<string>();
            targetColumns = new ObservableCollection<string>();
            dataTypeList = new ObservableCollection<string>();
            unitTypesList = new ObservableCollection<string>();  

            dataTypeList.Add("NUMERIC" );
            dataTypeList.Add("TEXT" );

            unitTypesList.Add("");
            unitTypesList.Add(ImportDataMap.UNIT_PPM);
            unitTypesList.Add(ImportDataMap.UNIT_PCT);

            InitializeComponent();

        }

        internal void ResetView()
        {
            sourceColumns = new ObservableCollection<string>();
            targetColumns = new ObservableCollection<string>();
            dataTypeList = new ObservableCollection<string>();
            unitTypesList = new ObservableCollection<string>();

            dataTypeList.Add(ImportDataMap.NUMERICDATATYPE);
            dataTypeList.Add(ImportDataMap.TEXTDATATYPE);
            dataTypeList.Add(ImportDataMap.TIMESTAMPDATATYPE);
            

            unitTypesList.Add("");
            unitTypesList.Add(ImportDataMap.UNIT_PPM);
            unitTypesList.Add(ImportDataMap.UNIT_PCT);
            DataGridColumnMap.ItemsSource = null;
            cmaps = null;

        }

        private void DataGridCell_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DataGridCell cell = sender as DataGridCell;
            if (!cell.IsEditing)
            {
                // enables editing on single click
                if (!cell.IsFocused)
                    cell.Focus();
                if (!cell.IsSelected)
                    cell.IsSelected = true;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }



        internal void SetMap(ImportDataMap impMap)
        {
            ImportDataMap oldMap = null;
            if (DataGridColumnMap.ItemsSource != null)
            {
                oldMap = GenerateImportDataMap(impMap.mapOriginalDataFile, impMap.mapTargetPrimaryTable, impMap.inputDelimiter, impMap.MaxColumns);
            }


            DataGridColumnMap.ItemsSource = null;
            
            sourceColumns = new ObservableCollection<string>();
            targetColumns = new ObservableCollection<string>();
            foreach (ColumnMap mi in impMap.columnMap)
            {
                sourceColumns.Add(mi.sourceColumnName);
                targetColumns.Add(mi.targetColumnName);
                
                if (mi.importDataType == null) {
                    mi.importDataType = ImportDataMap.NUMERICDATATYPE;
                }else{
                    // try and get last used type
                    if (oldMap != null && oldMap.columnMap.Count > 0)
                    {
                        mi.importDataType = GetPreviousTypeFromMap(oldMap, mi);
                    }
                }
                 
                
                if (mi.defaultValue == null || mi.defaultValue.Trim().Length == 0) {
                    mi.defaultValue = GetPreviousDefaultFromMap(oldMap, mi);
                }
            }
            dataTypeList = new ObservableCollection<string>();
            dataTypeList.Add(ImportDataMap.NUMERICDATATYPE);
            dataTypeList.Add(ImportDataMap.TEXTDATATYPE);
            dataTypeList.Add(ImportDataMap.TIMESTAMPDATATYPE);

            DataGridColumnMap.ItemsSource = null;
            DataGridColumnMap.Items.Clear();

            unitTypesList = new ObservableCollection<string>();
            unitTypesList.Add("");
            unitTypesList.Add(ImportDataMap.UNIT_PCT);
            unitTypesList.Add(ImportDataMap.UNIT_PPM);
            


            cmaps = new ObservableCollection<ColumnMap>();
            foreach (ColumnMap cm in impMap.columnMap)
            {
                cmaps.Add(cm);

            }
            DataGridColumnMap.ItemsSource = cmaps;

            DataGridColumnMap.UpdateLayout();


        }

        private string GetPreviousTypeFromMap(ImportDataMap oldMap, ColumnMap mi)
        {

            string importDataType = mi.importDataType;
            if (oldMap != null)
            {
                foreach (ColumnMap cm in oldMap.columnMap)
                {
                    if (cm.sourceColumnName.Trim().Equals(mi.sourceColumnName.Trim()))
                    {
                        importDataType = cm.importDataType;
                        break;
                    }
                }
            }

            return importDataType;
        }

        private string GetPreviousDefaultFromMap(ImportDataMap oldMap, ColumnMap mi)
        {

            string defaultVal = "";
            if (oldMap != null)
            {
                foreach (ColumnMap cm in oldMap.columnMap)
                {
                    if (cm.sourceColumnName.Trim().Equals(mi.sourceColumnName.Trim()))
                    {
                        defaultVal = cm.defaultValue;
                        break;
                    }
                }
            }

            return defaultVal;
        }


        internal ImportDataMap GetImportDataMap(string fileName, string mapTargetPrimaryTable, char delimeter, int maxCols)
        {
            ImportDataMap idm = GenerateImportDataMap(fileName, mapTargetPrimaryTable, delimeter, maxCols);

            return idm;
        }



        internal void SaveFormat(string fileName, string mapTargetPrimaryTable, char delimeter, int maxCols)
        {
            ImportDataMap idm = GenerateImportDataMap(fileName, mapTargetPrimaryTable, delimeter, maxCols);
            ImportMapIO.SaveImportMap(idm, fileName);
        }

        private ImportDataMap GenerateImportDataMap(string fileName, string mapTargetPrimaryTable, char delimeter, int maxCols)
        {
            ObservableCollection<ColumnMap> inMapCols = (ObservableCollection<ColumnMap>)DataGridColumnMap.ItemsSource;

            string res = "";
            res += inMapCols.Count;

            ImportDataMap idm = new ImportDataMap();
            foreach (ColumnMap cm in inMapCols)
            {
                if (cm.sourceColumnName != null && cm.sourceColumnName.Trim().Length > 0)
                {
                    idm.columnMap.Add(cm);
                }
            }
            idm.dataStartLine = 2;
            idm.mapDate = System.DateTime.Now;
            idm.mapName = "";
            idm.mapTargetPrimaryTable = mapTargetPrimaryTable;
            idm.inputDelimiter = delimeter;
            idm.mapOriginalDataFile = fileName;
            idm.MaxColumns = maxCols;

            return idm;
        }

        internal ImportDataMap LoadFormatFile(string fileName)
        {
            ImportDataMap inMap = ImportMapIO.LoadImportMap(fileName);
            SetMap(inMap);
            return inMap;
        }

        /// <summary>
        /// 
        /// 
        /// </summary>
        /// <param name="NKDColumnName"></param>
        /// <returns></returns>
        internal int GetColumnMappedTo(string NKDColumnName)
        {
            int res = -1;

            foreach (ColumnMap cm in cmaps)
            {
                if (cm.targetColumnName.Trim().Equals(NKDColumnName.Trim()))
                {
                    res = cm.sourceColumnNumber;
                    break;
                }
            }
            return res;
        }



        
    }
}
