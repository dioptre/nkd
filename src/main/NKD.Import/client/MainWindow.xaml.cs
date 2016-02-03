using NKD.Import.Client.DataRecords;
using NKD.Import.Client.Definitions;
using NKD.Import.Client.IO;
using NKD.Import.Client.Processing;
using NKD.Import.Client.UI;
using NKD.Import.Client.UICommands;
using NKD.Import.Client.UIUtils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Sql;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NKD.Import;
using NKD.Import.ColumnSpecs;
using NKD.Import.DataWrappers;
using NKD.Import.FormatSpecification;
using NKD.Import.LAS;

namespace NKD.Import.Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {
        public string titleText = "NKD Importer ";
        public string releaseDate = "27/02/2014";
        public string versionNumber = "1.0";
        // direct UI commands to processes
        CommandDirector commandDirector = new CommandDirector();

        BackgroundWorker workerLoadData;
        BackgroundWorker workerBMDataImport;
        BackgroundWorker workerAssayDataImport;
        BackgroundWorker workerCoalQualityDataImport;
        BackgroundWorker workerSurveyDataImport;
        BackgroundWorker workerLithoDataImport;
        BackgroundWorker workerLASBatchDataImport;

        ModelColumnDefinitions columnDefs;

        ModelImportStatus latestImportUpdateStatus = null;

        //public List<ColumnMetaInfo> currentDBFieldsMetaInfo { get; set; }
        public List<ColumnMetaInfo> bmDBFields { get; set; }
        public List<ColumnMetaInfo> collarDBFields { get; set; }
        public List<ColumnMetaInfo> assayDBFields { get; set; }
        public List<ColumnMetaInfo> coalQualityDBFields { get; set; }
        public List<ColumnMetaInfo> lasDBFields { get; set; }
        public List<ColumnMetaInfo> surveyDBFields { get; set; }
        public List<ColumnMetaInfo> lithoDBFields { get; set; }


        public static RoutedCommand OpenBM = new RoutedCommand();
        public static RoutedCommand OpenFormat = new RoutedCommand();
        public static RoutedCommand SaveFormat = new RoutedCommand();
        public static RoutedCommand BMImport = new RoutedCommand();
        public static RoutedCommand OpenLAS = new RoutedCommand();
        public static RoutedCommand OpenLASFormat = new RoutedCommand();
        public static RoutedCommand SaveLASFormat = new RoutedCommand();
        public static RoutedCommand LASImport = new RoutedCommand();


        public static RoutedCommand OpenCollar = new RoutedCommand();
        public static RoutedCommand OpenCollarFormat = new RoutedCommand();
        public static RoutedCommand SaveCollarFormat = new RoutedCommand();
        public static RoutedCommand CollarImport = new RoutedCommand();

        public static RoutedCommand OpenAssay = new RoutedCommand();
        public static RoutedCommand OpenAssayFormat = new RoutedCommand();
        public static RoutedCommand SaveAssayFormat = new RoutedCommand();
        public static RoutedCommand AssayImport = new RoutedCommand();

        public static RoutedCommand OpenCoalQuality = new RoutedCommand();
        public static RoutedCommand OpenCoalQualityFormat = new RoutedCommand();
        public static RoutedCommand SaveCoalQualityFormat = new RoutedCommand();
        public static RoutedCommand CoalQualityImport = new RoutedCommand();

        public static RoutedCommand OpenSurvey = new RoutedCommand();
        public static RoutedCommand OpenSurveyFormat = new RoutedCommand();
        public static RoutedCommand SaveSurveyFormat = new RoutedCommand();
        public static RoutedCommand SurveyImport = new RoutedCommand();

        public static RoutedCommand OpenLitho = new RoutedCommand();
        public static RoutedCommand OpenLithoFormat = new RoutedCommand();
        public static RoutedCommand SaveLithoFormat = new RoutedCommand();
        public static RoutedCommand LithoImport = new RoutedCommand();

        public static RoutedCommand CollarPreview = new RoutedCommand();

        public static RoutedCommand BatchImportLAS = new RoutedCommand();


        private string FileDescription = "Data files (.csv, .txt)|*.csv;*.txt|All files (*.*)|*.*";
        private string FileExtension = "*.csv";

        private string LASFileDescription = "LAS files (.las)|*.las;|All files (*.*)|*.*";
        private string LASFileExtension = "*.las";

        private string FormatFileExtension = "*.xml";
        private string FormatFileDescription = "Format description files (*.xml)|*.xml";
        private Guid NKDProjectID;
        string SelectedFormatFile { get; set; }
        int SelectedImportType { get; set; }
        bool doImportOverwrite = false;

        public MainWindow()
        {
            CommandBinding cb1 = new CommandBinding(OpenBM, OpenBMExecuted, OpenBMCanExecute);
            CommandBinding cb2 = new CommandBinding(OpenFormat, OpenFormatExecuted, OpenBMFormatCanExecute);
            CommandBinding cb3 = new CommandBinding(SaveFormat, SaveFormatExecuted, SaveFormatCanExecute);
            CommandBinding cb4 = new CommandBinding(BMImport, BMImportExecuted, BMImportCanExecute);


            CommandBinding cb5 = new CommandBinding(OpenLAS, OpenLASExecuted, OpenLASCanExecute);
            CommandBinding cb6 = new CommandBinding(OpenLASFormat, OpenLASFormatExecuted, OpenLASFormatCanExecute);
            CommandBinding cb7 = new CommandBinding(SaveLASFormat, SaveLASFormatExecuted, SaveLASFormatCanExecute);
            CommandBinding cb8 = new CommandBinding(LASImport, LASImportExecuted, LASImportCanExecute);


            CommandBinding cb9 = new CommandBinding(OpenCollar, OpenCollarExecuted, OpenCollarCanExecute);
            CommandBinding cb10 = new CommandBinding(OpenCollarFormat, OpenCollarFormatExecuted, OpenCollarFormatCanExecute);
            CommandBinding cb11 = new CommandBinding(SaveCollarFormat, SaveCollarFormatExecuted, SaveCollarFormatCanExecute);
            CommandBinding cb12 = new CommandBinding(CollarImport, CollarImportExecuted, CollarImportCanExecute);

            CommandBinding cb13 = new CommandBinding(OpenAssay, OpenAssayExecuted, OpenAssayCanExecute);
            CommandBinding cb14 = new CommandBinding(OpenAssayFormat, OpenAssayFormatExecuted, OpenAssayFormatCanExecute);
            CommandBinding cb15 = new CommandBinding(SaveAssayFormat, SaveAssayFormatExecuted, SaveAssayFormatCanExecute);
            CommandBinding cb16 = new CommandBinding(AssayImport, AssayImportExecuted, AssayImportCanExecute);

            CommandBinding cb17 = new CommandBinding(OpenSurvey, OpenSurveyExecuted, OpenSurveyCanExecute);
            CommandBinding cb18 = new CommandBinding(OpenSurveyFormat, OpenSurveyFormatExecuted, OpenSurveyFormatCanExecute);
            CommandBinding cb19 = new CommandBinding(SaveSurveyFormat, SaveSurveyFormatExecuted, SaveSurveyFormatCanExecute);
            CommandBinding cb20 = new CommandBinding(SurveyImport, SurveyImportExecuted, SurveyImportCanExecute);



            CommandBinding cb21 = new CommandBinding(CollarPreview, CollarPreviewExecuted, CollarPreviewCanExecute);

            CommandBinding cb22 = new CommandBinding(OpenLitho, OpenLithoExecuted, OpenLithoCanExecute);
            CommandBinding cb23 = new CommandBinding(OpenLithoFormat, OpenLithoFormatExecuted, OpenLithoFormatCanExecute);
            CommandBinding cb24 = new CommandBinding(SaveLithoFormat, SaveLithoFormatExecuted, SaveLithoFormatCanExecute);
            CommandBinding cb25 = new CommandBinding(LithoImport, LithoImportExecuted, LithoImportCanExecute);

            CommandBinding cb26 = new CommandBinding(BatchImportLAS, BatchImportLASExecuted, BatchImportLASCanExecute);

            CommandBinding cb27 = new CommandBinding(OpenCoalQuality, OpenCoalQualityExecuted, OpenCoalQualityCanExecute);
            CommandBinding cb28 = new CommandBinding(OpenCoalQualityFormat, OpenCoalQualityFormatExecuted, OpenCoalQualityFormatCanExecute);
            CommandBinding cb29 = new CommandBinding(SaveCoalQualityFormat, SaveCoalQualityFormatExecuted, SaveCoalQualityFormatCanExecute);
            CommandBinding cb30 = new CommandBinding(CoalQualityImport, CoalQualityImportExecuted, CoalQualityImportCanExecute);


            this.CommandBindings.Add(cb1);
            this.CommandBindings.Add(cb2);
            this.CommandBindings.Add(cb3);
            this.CommandBindings.Add(cb4);
            this.CommandBindings.Add(cb5);
            this.CommandBindings.Add(cb6);
            this.CommandBindings.Add(cb7);
            this.CommandBindings.Add(cb8);

            this.CommandBindings.Add(cb9);
            this.CommandBindings.Add(cb10);
            this.CommandBindings.Add(cb11);
            this.CommandBindings.Add(cb12);

            this.CommandBindings.Add(cb13);
            this.CommandBindings.Add(cb14);
            this.CommandBindings.Add(cb15);
            this.CommandBindings.Add(cb16);

            this.CommandBindings.Add(cb17);
            this.CommandBindings.Add(cb18);
            this.CommandBindings.Add(cb19);
            this.CommandBindings.Add(cb20);
            this.CommandBindings.Add(cb21);

            this.CommandBindings.Add(cb22);
            this.CommandBindings.Add(cb23);
            this.CommandBindings.Add(cb24);
            this.CommandBindings.Add(cb25);

            this.CommandBindings.Add(cb26);

            this.CommandBindings.Add(cb27);
            this.CommandBindings.Add(cb28);
            this.CommandBindings.Add(cb29);
            this.CommandBindings.Add(cb30);

            InitializeComponent();

            Dictionary<RibbonButton, RoutedCommand> commandMapping = new Dictionary<RibbonButton, RoutedCommand>();

            commandMapping.Add(ButtonOpenBM, OpenBM);
            commandMapping.Add(ButtonOpenFormat, OpenFormat);
            commandMapping.Add(ButtonSaveFormat, SaveFormat);
            commandMapping.Add(ButtonImportBM, BMImport);

            commandMapping.Add(ButtonOpenLAS, OpenLAS);
            commandMapping.Add(ButtonOpenLASFormat, OpenLASFormat);
            commandMapping.Add(ButtonSaveLASFormat, SaveLASFormat);
            commandMapping.Add(ButtonImportLAS, LASImport);

            commandMapping.Add(ButtonOpenCollar, OpenCollar);
            commandMapping.Add(ButtonOpenCollarFormat, OpenCollarFormat);
            commandMapping.Add(ButtonSaveCollarFormat, SaveCollarFormat);
            commandMapping.Add(ButtonImportCollar, CollarImport);
            commandMapping.Add(ButtonOpenAssay, OpenAssay);
            commandMapping.Add(ButtonOpenAssayFormat, OpenAssayFormat);
            commandMapping.Add(ButtonSaveAssayFormat, SaveAssayFormat);
            commandMapping.Add(ButtonImportAssay, AssayImport);
            commandMapping.Add(ButtonOpenSurvey, OpenSurvey);
            commandMapping.Add(ButtonOpenSurveyFormat, OpenSurveyFormat);
            commandMapping.Add(ButtonSaveSurveyFormat, SaveSurveyFormat);
            commandMapping.Add(ButtonImportSurvey, SurveyImport);
            commandMapping.Add(ButtonCollarPreview, CollarPreview);
            commandMapping.Add(ButtonOpenLitho, OpenLitho);
            commandMapping.Add(ButtonOpenLithoFormat, OpenLithoFormat);
            commandMapping.Add(ButtonSaveLithoFormat, SaveLithoFormat);
            commandMapping.Add(ButtonImportLitho, LithoImport);
            commandMapping.Add(ButtonImportBatchLAS, BatchImportLAS);


            commandMapping.Add(ButtonOpenCoalQuality, OpenCoalQuality);
            commandMapping.Add(ButtonOpenCoalQualityFormat, OpenCoalQualityFormat);
            commandMapping.Add(ButtonSaveCoalQualityFormat, SaveCoalQualityFormat);
            commandMapping.Add(ButtonImportCoalQuality, CoalQualityImport);

            AssignEventsToButtons(commandMapping);

            ImportDataPreview.targetMainDataType = MapConfigTable.collarPrimaryTableName;
            // LocateSqlInstances();
            InitialiseDatabaseColumnHeadings();

            try
            {
                string constr = System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                string catalog = constr.Split(';').Where(x => x.ToLower().StartsWith("initial catalog=")).FirstOrDefault().Split('=').LastOrDefault();

                if (System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed)
                {
                    System.Deployment.Application.ApplicationDeployment ad = System.Deployment.Application.ApplicationDeployment.CurrentDeployment;
                    LabelVersion.Content = ad.CurrentVersion.ToString();
                    LabelDB.Content = !string.IsNullOrWhiteSpace(catalog) ? catalog : "NKD???";
                }
                else //we arnt network deployed or we are not published yet!
                {
                    LabelVersion.Content = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                    LabelDB.Content = !string.IsNullOrWhiteSpace(catalog) ? catalog : "NKD???";
                }

            }
            catch (Exception exc1) { }
            SelectedImportType = -1;
        }




        /// <summary>
        /// Locate instances of available database servers
        /// </summary>
        public static void LocateSqlInstances()
        {
            //For now let's shutdown if can't connect
            try
            {
                var connection = new System.Data.SqlClient.SqlConnection(CommandDirector.ConnectionString);
                connection.Open();
                connection.Close();
            }
            catch
            {
                MessageBox.Show("Download and install NKD client if not already installed.", "Couldn't connect to data source.");
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }

            //TODO:
            using (DataTable sqlSources = SqlDataSourceEnumerator.Instance.GetDataSources())
            {
                foreach (DataRow source in sqlSources.Rows)
                {
                    string servername;
                    string instanceName = source["InstanceName"].ToString();

                    if (!string.IsNullOrEmpty(instanceName))
                    {
                        servername = source["InstanceName"] + "\\" + source["ServerName"];
                    }
                    else
                    {
                        servername = "" + source["ServerName"];
                    }
                    Console.WriteLine(" Server Name:{0}", servername);
                    Console.WriteLine("     Version:{0}", source["Version"]);
                    Console.WriteLine();

                }

            }
        }


        private void LithoImportCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.SelectedImportType == GeneralParameters.LITHO)
            {
                e.CanExecute = true;
            }
            else
            {
                e.CanExecute = false;
            }
            e.Handled = true;
        }



        private void SaveLithoFormatCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.SelectedImportType == GeneralParameters.LITHO)
            {
                e.CanExecute = true;
            }
            else
            {
                e.CanExecute = false;
            }
            e.Handled = true;
        }

        private void SaveLithoFormatExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            SaveFormatFile((SelectedFile.ToLower().IndexOf(".csv") > -1) ? ',' : '\t', ImportDataPreview.MaxColumns);
        }

        private void OpenLithoFormatCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.SelectedImportType == GeneralParameters.LITHO)
            {
                e.CanExecute = true;
            }
            else
            {
                e.CanExecute = false;
            }
            e.Handled = true;
            e.Handled = true;
        }

        private void OpenLithoFormatExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFormatDataFile();
        }

        private void OpenLithoCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void OpenLithoExecuted(object sender, ExecutedRoutedEventArgs e)
        {

            string fileName = UIFileUtils.ShowOpenFileChoose(FileExtension, FileDescription, SelectedFile);
            if (fileName == null)
            {
                //  SelectedInputFile.Text = "";
                LabelLoadedFile.Content = "no file loaded";
            }
            else
            {
                ResetUI();
                SelectedImportType = GeneralParameters.LITHO;
                SelectedFile = fileName;
                // FileSelected(fileName);

                if (fileName != null && fileName.Trim().Length > 0)
                {
                    LoadTextDataForPreview(fileName);

                }
                LabelLoadedFile.Content = fileName;
                this.Title = titleText + " - " + fileName;
                SetRibbonEnabledStatus(GeneralParameters.LITHO);
            }
            e.Handled = true;
        }



        private void CollarPreviewCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.SelectedImportType == GeneralParameters.COLLAR)
            {
                e.CanExecute = true;
            }
            else
            {
                e.CanExecute = false;
            }
            e.Handled = true;
        }

        private void CollarPreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {

            ViewProjectHoles();

            e.Handled = true;
        }

        private void SurveyImportCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.SelectedImportType == GeneralParameters.SURVEY)
            {
                e.CanExecute = true;
            }
            else
            {
                e.CanExecute = false;
            }
            e.Handled = true;
            e.Handled = true;
        }

        private void SurveyImportExecuted(object sender, ExecutedRoutedEventArgs e)
        {

            if (ComboBoxProjectList.SelectedValue == null)
            {
                ComboBoxProjectList.BorderBrush = Brushes.Red;
                MessageBox.Show("You must select a project before importing");
                return;
            }
            Guid gg = (Guid)ComboBoxProjectList.SelectedValue;
            NKDProjectID = gg;
            ImportDataMap importMap = MapConfigTable.GetImportDataMap(SelectedFile, MapConfigTable.surveyPrimaryTableName, (SelectedFile.ToLower().IndexOf(".csv") > -1) ? ',' : '\t', ImportDataPreview.MaxColumns);
            // add into map details of which columns are foreign keys

            if (surveyDBFields != null)
            {
                importMap.UpdateWithFKInof(surveyDBFields);
            }
            doDuplicateCheck = (bool)checkBoxDupeCheck.IsChecked;
            doImportOverwrite = (bool)checkBoxOverwrite.IsChecked;
            // get the selected project ID
            NKDProjectID = gg;

            workerSurveyDataImport = new BackgroundWorker();
            workerSurveyDataImport.WorkerReportsProgress = true;
            workerSurveyDataImport.WorkerSupportsCancellation = false;
            workerSurveyDataImport.DoWork += bw_DoSurveyImportWork;
            // Method to call when Progress has changed
            workerSurveyDataImport.ProgressChanged += bw_ProgressChanged;
            // Method to run after BackgroundWorker has completed?
            workerSurveyDataImport.RunWorkerCompleted += bw_SurveyImportRunWorkerCompleted;


            workerSurveyDataImport.RunWorkerAsync(importMap);
            e.Handled = true;
        }



        private void LithoImportExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (ComboBoxProjectList.SelectedValue == null)
            {
                ComboBoxProjectList.BorderBrush = Brushes.Red;
                MessageBox.Show("You must select a project before importing");
                return;
            }
            Guid gg = (Guid)ComboBoxProjectList.SelectedValue;
            NKDProjectID = gg;
            ImportDataMap importMap = MapConfigTable.GetImportDataMap(SelectedFile, MapConfigTable.lithoPrimaryTableName, (SelectedFile.ToLower().IndexOf(".csv") > -1) ? ',' : '\t', ImportDataPreview.MaxColumns);
            // add into map details of which columns are foreign keys

            if (lithoDBFields != null)
            {
                importMap.UpdateWithFKInof(lithoDBFields);
            }
            doDuplicateCheck = (bool)checkBoxDupeCheck.IsChecked;
            doImportOverwrite = (bool)checkBoxOverwrite.IsChecked;
            // get the selected project ID
            NKDProjectID = gg;

            workerLithoDataImport = new BackgroundWorker();
            workerLithoDataImport.WorkerReportsProgress = true;
            workerLithoDataImport.WorkerSupportsCancellation = false;
            workerLithoDataImport.DoWork += bw_DoLithoImportWork;
            // Method to call when Progress has changed
            workerLithoDataImport.ProgressChanged += bw_ProgressChanged;
            // Method to run after BackgroundWorker has completed?
            workerLithoDataImport.RunWorkerCompleted += bw_LithoImportRunWorkerCompleted;


            workerLithoDataImport.RunWorkerAsync(importMap);
            e.Handled = true;
        }

        private void bw_DoLithoImportWork(object sender, DoWorkEventArgs e)
        {

            ImportDataMap importMap = (ImportDataMap)e.Argument;
            commandDirector.SetCurrentWorkerThread(workerLithoDataImport);
            var rawFileReader = new RawFileReader((SelectedFile.ToLower().IndexOf(".csv") > -1) ? ',' : '\t');
            ModelImportStatus status = commandDirector.DoLithoImport(SelectedFile, SelectedFormatFile, importMap, rawFileReader, NKDProjectID, doImportOverwrite, this.doDuplicateCheck);
            latestImportUpdateStatus = status;
            workerLithoDataImport.ReportProgress((int)0, "");

        }




        private void SaveSurveyFormatCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.SelectedImportType == GeneralParameters.SURVEY)
            {
                e.CanExecute = true;
            }
            else
            {
                e.CanExecute = false;
            }
            e.Handled = true;
            e.Handled = true;
        }

        private void SaveSurveyFormatExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            SaveFormatFile((SelectedFile.ToLower().IndexOf(".csv") > -1) ? ',' : '\t', ImportDataPreview.MaxColumns);
            e.Handled = true;
        }

        private void OpenSurveyFormatCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.SelectedImportType == GeneralParameters.SURVEY)
            {
                e.CanExecute = true;
            }
            else
            {
                e.CanExecute = false;
            }
            e.Handled = true;
            e.Handled = true;
        }

        private void OpenSurveyFormatExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFormatDataFile();
            e.Handled = true;
        }

        private void OpenSurveyCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void OpenSurveyExecuted(object sender, ExecutedRoutedEventArgs e)
        {

            string fileName = UIFileUtils.ShowOpenFileChoose(FileExtension, FileDescription, SelectedFile);
            if (fileName == null)
            {
                //  SelectedInputFile.Text = "";
                LabelLoadedFile.Content = "no file loaded";
            }
            else
            {
                ResetUI();
                SelectedImportType = GeneralParameters.SURVEY;
                SelectedFile = fileName;
                // FileSelected(fileName);

                if (fileName != null && fileName.Trim().Length > 0)
                {
                    LoadTextDataForPreview(fileName);

                }
                LabelLoadedFile.Content = fileName;
                this.Title = titleText + " - " + fileName;
                SetRibbonEnabledStatus(GeneralParameters.SURVEY);
            }
            e.Handled = true;
        }

        public void ResetFormatData()
        {
            ImportDataPreview.ResetMapping();
            MapConfigTable.ResetView();
        }

        public void ResetUI()
        {
            ModelColumnDefinitions columnDefs = new ModelColumnDefinitions();
            latestImportUpdateStatus = null;
            SelectedFormatFile = "";
            LabelLoadedFile.Content = "no file loaded";
            //currentDBFieldsMetaInfo = new List<ColumnMetaInfo>();
            MapConfigTable.ResetView();
            ImportDataPreview.ResetData();
            this.SelectedImportType = -1;
            ReSetRibbonEnabledStatus(true);

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void myWin_Loaded(object sender, RoutedEventArgs e)
        {
            // set up initial data 
            try
            {

                int version = commandDirector.GetNKDVersion(); ;
                Dictionary<Guid, string> projects = commandDirector.GetProjectList();

                ComboBoxProjectList.ItemsSource = projects;


            }
            catch (Exception ex) { }
        }


        private void AssignEventsToButtons(Dictionary<RibbonButton, RoutedCommand> commandMapping)
        {
            int i = 1;
            foreach (KeyValuePair<RibbonButton, RoutedCommand> kvp in commandMapping)
            {
                AssignCommandToButton(kvp.Key, kvp.Value);
                i++;
            }


        }

        private void AssignCommandToButton(RibbonButton theButton, RoutedCommand theCommand)
        {
            try
            {
                theButton.Command = theCommand;
            }
            catch (Exception ex)
            {
                string err = ex.ToString();
            }
        }



        private void AssayImportCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.SelectedImportType == GeneralParameters.ASSAY)
            {
                e.CanExecute = true;
            }
            else
            {
                e.CanExecute = false;
            }
            e.Handled = true;
        }

        private void AssayImportExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (ComboBoxProjectList.SelectedValue == null)
            {
                ComboBoxProjectList.BorderBrush = Brushes.Red;
                MessageBox.Show("You must select a project before importing");
                return;
            }
            doDuplicateCheck = (bool)checkBoxDupeCheck.IsChecked;
            doImportOverwrite = (bool)checkBoxOverwrite.IsChecked;
            Guid gg = (Guid)ComboBoxProjectList.SelectedValue;
            NKDProjectID = gg;
            ImportDataMap importMap = MapConfigTable.GetImportDataMap(SelectedFile, MapConfigTable.assayPrimaryTableName, (SelectedFile.ToLower().IndexOf(".csv") > -1) ? ',' : '\t', ImportDataPreview.MaxColumns);
            // add into map details of which columns are foreign keys

            if (assayDBFields != null)
            {
                importMap.UpdateWithFKInof(assayDBFields);
            }

            // get the selected project ID
            NKDProjectID = gg;

            workerAssayDataImport = new BackgroundWorker();
            workerAssayDataImport.WorkerReportsProgress = true;
            workerAssayDataImport.WorkerSupportsCancellation = false;
            workerAssayDataImport.DoWork += bw_DoAssayImportWork;
            // Method to call when Progress has changed
            workerAssayDataImport.ProgressChanged += bw_ProgressChanged;
            // Method to run after BackgroundWorker has completed?
            workerAssayDataImport.RunWorkerCompleted += bw_AssayImportRunWorkerCompleted;


            workerAssayDataImport.RunWorkerAsync(importMap);

            e.Handled = true;
        }


        private void bw_DoAssayImportWork(object sender, DoWorkEventArgs e)
        {
            ImportDataMap importMap = (ImportDataMap)e.Argument;
            commandDirector.SetCurrentWorkerThread(workerAssayDataImport);
            var rawFileReader = new RawFileReader((SelectedFile.ToLower().IndexOf(".csv") > -1) ? ',' : '\t');
            ModelImportStatus status = commandDirector.DoAssayImport(SelectedFile, SelectedFormatFile, importMap, rawFileReader,
                NKDProjectID, doDuplicateCheck, doImportOverwrite);
            latestImportUpdateStatus = status;
        }

        /// <summary>
        /// The import of assays is complete
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bw_AssayImportRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (latestImportUpdateStatus != null)
            {
                ImportStatusWindow ii = new ImportStatusWindow();
                ii.SetData(latestImportUpdateStatus);
                ii.ShowDialog();
            }

        }


        private void bw_DoSurveyImportWork(object sender, DoWorkEventArgs e)
        {
            ImportDataMap importMap = (ImportDataMap)e.Argument;
            commandDirector.SetCurrentWorkerThread(workerSurveyDataImport);
            var rawFileReader = new RawFileReader((SelectedFile.ToLower().IndexOf(".csv") > -1) ? ',' : '\t');
            ModelImportStatus status = commandDirector.DoSurveyImport(SelectedFile, SelectedFormatFile, importMap, rawFileReader, NKDProjectID, doImportOverwrite, this.doDuplicateCheck);
            latestImportUpdateStatus = status;
            workerSurveyDataImport.ReportProgress((int)0, "");
        }

        private void bw_SurveyImportRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ImportStatusWindow ii = new ImportStatusWindow();
            ii.SetData(latestImportUpdateStatus);
            ii.ShowDialog();
        }


        private void bw_LithoImportRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ImportStatusWindow ii = new ImportStatusWindow();
            ii.SetData(latestImportUpdateStatus);
            ii.ShowDialog();
        }

        private void SaveAssayFormatCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.SelectedImportType == GeneralParameters.ASSAY)
            {
                e.CanExecute = true;
            }
            else
            {
                e.CanExecute = false;
            }
            e.Handled = true;
        }

        private void SaveAssayFormatExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            SaveFormatFile((SelectedFile.ToLower().IndexOf(".csv") > -1) ? ',' : '\t', ImportDataPreview.MaxColumns);
            e.Handled = true;
        }

        private void OpenAssayFormatCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {

            if (this.SelectedImportType == GeneralParameters.ASSAY)
            {
                e.CanExecute = true;
            }
            else
            {
                e.CanExecute = false;
            }
            e.Handled = true;
        }

        private void OpenAssayFormatExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            SelectedImportType = GeneralParameters.ASSAY;
            OpenFormatDataFile();
            e.Handled = true;
        }

        private void OpenAssayCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {

            e.CanExecute = true;
            e.Handled = true;
        }

        private void OpenAssayExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            string fileName = UIFileUtils.ShowOpenFileChoose(FileExtension, FileDescription, SelectedFile);
            if (fileName == null)
            {
                LabelLoadedFile.Content = "no file loaded";
            }
            else
            {
                ResetUI();
                SelectedImportType = GeneralParameters.ASSAY;
                SelectedFile = fileName;
                if (fileName != null && fileName.Trim().Length > 0)
                {
                    LoadTextDataForPreview(fileName);

                }
                LabelLoadedFile.Content = fileName;
                this.Title = titleText + " - " + fileName;

                SetRibbonEnabledStatus(GeneralParameters.ASSAY);

            }

            e.Handled = true;
        }


        //-------------------------
        private void SaveCoalQualityFormatCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.SelectedImportType == GeneralParameters.COAL_QUALITY)
            {
                e.CanExecute = true;
            }
            else
            {
                e.CanExecute = false;
            }
            e.Handled = true;
        }

        private void SaveCoalQualityFormatExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            SaveFormatFile((SelectedFile.ToLower().IndexOf(".csv") > -1) ? ',' : '\t', ImportDataPreview.MaxColumns);
            e.Handled = true;
        }

        private void OpenCoalQualityFormatCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {

            if (this.SelectedImportType == GeneralParameters.COAL_QUALITY)
            {
                e.CanExecute = true;
            }
            else
            {
                e.CanExecute = false;
            }
            e.Handled = true;
        }

        private void OpenCoalQualityFormatExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            SelectedImportType = GeneralParameters.COAL_QUALITY;
            OpenFormatDataFile();
            e.Handled = true;
        }

        private void OpenCoalQualityCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {

            e.CanExecute = true;
            e.Handled = true;
        }

        private void OpenCoalQualityExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            string fileName = UIFileUtils.ShowOpenFileChoose(FileExtension, FileDescription, SelectedFile);
            if (fileName == null)
            {
                LabelLoadedFile.Content = "no file loaded";
            }
            else
            {
                ResetUI();
                SelectedImportType = GeneralParameters.COAL_QUALITY;
                SelectedFile = fileName;
                if (fileName != null && fileName.Trim().Length > 0)
                {
                    LoadTextDataForPreview(fileName);

                }
                LabelLoadedFile.Content = fileName;
                this.Title = titleText + " - " + fileName;

                SetRibbonEnabledStatus(GeneralParameters.COAL_QUALITY);

            }

            e.Handled = true;
        }




        private void CoalQualityImportCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.SelectedImportType == GeneralParameters.COAL_QUALITY)
            {
                e.CanExecute = true;
            }
            else
            {
                e.CanExecute = false;
            }
            e.Handled = true;
        }

        private void CoalQualityImportExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (ComboBoxProjectList.SelectedValue == null)
            {
                ComboBoxProjectList.BorderBrush = Brushes.Red;
                MessageBox.Show("You must select a project before importing");
                return;
            }
            doDuplicateCheck = (bool)checkBoxDupeCheck.IsChecked;
            doImportOverwrite = (bool)checkBoxOverwrite.IsChecked;
            Guid gg = (Guid)ComboBoxProjectList.SelectedValue;
            NKDProjectID = gg;
            ImportDataMap importMap = MapConfigTable.GetImportDataMap(SelectedFile, MapConfigTable.assayPrimaryTableName, (SelectedFile.ToLower().IndexOf(".csv") > -1) ? ',' : '\t', ImportDataPreview.MaxColumns);
            // add into map details of which columns are foreign keys

            if (coalQualityDBFields != null)
            {
                importMap.UpdateWithFKInof(coalQualityDBFields);
            }

            // get the selected project ID
            NKDProjectID = gg;

            workerCoalQualityDataImport = new BackgroundWorker();

            workerCoalQualityDataImport.WorkerReportsProgress = true;
            workerCoalQualityDataImport.WorkerSupportsCancellation = false;
            workerCoalQualityDataImport.DoWork += bw_DoCoalQualityImportWork;
            // Method to call when Progress has changed
            workerCoalQualityDataImport.ProgressChanged += bw_ProgressChanged;
            // Method to run after BackgroundWorker has completed?
            workerCoalQualityDataImport.RunWorkerCompleted += bw_CoalQualityImportRunWorkerCompleted;


            workerCoalQualityDataImport.RunWorkerAsync(importMap);

            e.Handled = true;
        }


        private void bw_DoCoalQualityImportWork(object sender, DoWorkEventArgs e)
        {
            ImportDataMap importMap = (ImportDataMap)e.Argument;
            commandDirector.SetCurrentWorkerThread(workerCoalQualityDataImport);
            var rawFileReader = new RawFileReader((SelectedFile.ToLower().IndexOf(".csv") > -1) ? ',' : '\t');
            ModelImportStatus status = commandDirector.DoCoalQualityImport(SelectedFile, SelectedFormatFile, importMap, rawFileReader,
                NKDProjectID, doDuplicateCheck, doImportOverwrite);
            latestImportUpdateStatus = status;
        }

        /// <summary>
        /// The import of assays is complete
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bw_CoalQualityImportRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (latestImportUpdateStatus != null)
            {
                ImportStatusWindow ii = new ImportStatusWindow();
                ii.SetData(latestImportUpdateStatus);
                ii.ShowDialog();
            }

        }

        //-------------------------



        /// <summary>
        /// initiate a connection to the database to collect fieldnames from he block model table 
        /// </summary>
        private List<ColumnMetaInfo> GetBMFieldsFromNKD()
        {
            BaseImportTools bit = new BaseImportTools();
            List<string> ls = bit.GetBMColumns();
            List<ColumnMetaInfo> cmi = new List<ColumnMetaInfo>();
            foreach (string s in ls)
            {

                cmi.Add(new ColumnMetaInfo() { columnName = s, hasFK = false, isMandatory = true });

            }



            return cmi;

        }

        private List<ColumnMetaInfo> GetCollarFieldsFromNKD()
        {
            BaseImportTools bit = new BaseImportTools();


            List<ColumnMetaInfo> ls = bit.GetCollarColumns(CommandDirector.ConnectionString);

            return ls;
        }


        private List<ColumnMetaInfo> GetAssayFieldsFromNKD()
        {
            BaseImportTools bit = new BaseImportTools();
            List<ColumnMetaInfo> ls = bit.GetAssayColumns(CommandDirector.ConnectionString);

            return ls;
        }

        private List<ColumnMetaInfo> GetCoalQualityDBFieldsFromNKD()
        {
            BaseImportTools bit = new BaseImportTools();
            List<ColumnMetaInfo> ls = bit.GetCoalQualityColumns(CommandDirector.ConnectionString);

            return ls;
        }

        private void LoadTextDataForPreview(string inputFilename)
        {
            IOResults ares = new IOResults();
            // talk to the database to get the column names 
            List<ColumnMetaInfo> dbFields = null;
            if (SelectedImportType == GeneralParameters.BLOCKMODEL)
            {
                dbFields = bmDBFields;
            }
            else if (SelectedImportType == GeneralParameters.COLLAR)
            {
                dbFields = collarDBFields;
            }
            else if (SelectedImportType == GeneralParameters.ASSAY)
            {
                dbFields = assayDBFields;
            }
            else if (SelectedImportType == GeneralParameters.COAL_QUALITY)
            {
                dbFields = coalQualityDBFields;
            }
            else if (SelectedImportType == GeneralParameters.SURVEY)
            {
                dbFields = surveyDBFields;
            }
            else if (SelectedImportType == GeneralParameters.LITHO)
            {
                dbFields = lithoDBFields;
            }
            else
            {

            }
            ImportDataPreview.SetMandatoryMappingColumns(dbFields);
            ImportDataPreview.SetPreviewType("MODEL");

            bool firstLineIsHeader = true;
            var rawFileReader = new RawFileReader((SelectedFile.ToLower().IndexOf(".csv") > -1) ? ',' : '\t');
            List<RawDataRow> dt = rawFileReader.LoadRawDataForPreview(inputFilename, ares);
            if (inputFilename.ToLower().EndsWith("las"))
            {
                ImportDataPreview.ResetTable(dt, false);

            }
            else
            {
                ImportDataPreview.ResetTable(dt, firstLineIsHeader);

            }
        }

        private List<ColumnMetaInfo> GetDatabaseFiledNames()
        {
            List<ColumnMetaInfo> dbFields = new List<ColumnMetaInfo>();
            if (SelectedImportType == GeneralParameters.BLOCKMODEL)
            {
                dbFields = GetBMFieldsFromNKD();
                bmDBFields = dbFields;
            }
            else if (SelectedImportType == GeneralParameters.COLLAR)
            {
                dbFields = GetCollarFieldsFromNKD();
                collarDBFields = dbFields;
            }
            else if (SelectedImportType == GeneralParameters.ASSAY)
            {
                dbFields = GetAssayFieldsFromNKD();
                assayDBFields = dbFields;
            }
            else if (SelectedImportType == GeneralParameters.LAS)
            {
                dbFields = GetGeophysicsFieldsFromNKD();
                lasDBFields = dbFields;
            }
            return dbFields;
        }

        private void InitialiseDatabaseColumnHeadings()
        {

            bmDBFields = GetBMFieldsFromNKD();
            collarDBFields = GetCollarFieldsFromNKD();
            assayDBFields = GetAssayFieldsFromNKD();
            coalQualityDBFields = GetCoalQualityDBFieldsFromNKD();
            lasDBFields = GetGeophysicsFieldsFromNKD();
            surveyDBFields = GetSurveyFieldsFromNKD();
            lithoDBFields = GetLithoFieldsFromNKD();

        }





        private void LoadGeophysiscsTextDataForPreview(string inputFilename)
        {
            IOResults ares = new IOResults();

            List<ColumnMetaInfo> dbFields = GetGeophysicsFieldsFromNKD();
            // talk to the database to get the column names 

            ImportDataPreview.SetMandatoryMappingColumns(dbFields);
            ImportDataPreview.SetPreviewType("GEOPHYISCS");

            bool firstLineIsHeader = true;

            if (inputFilename.ToLower().EndsWith("las"))
            {
                LASFileReader lfr = new LASFileReader();
                int errCode = 0;
                LASFile fl = lfr.ReadLASFile(inputFilename, 0, out errCode);

                List<RawDataRow> dt = new List<RawDataRow>();
                RawDataRow rdh = new RawDataRow();
                rdh.dataItems = new List<string>();
                rdh.dataItems.Add("Depth");
                foreach (string ss in fl.columnHeaders)
                {
                    rdh.dataItems.Add(ss);
                }

                dt.Add(rdh);
                foreach (LASDataRow ldr in fl.dataRows)
                {
                    RawDataRow rd = new RawDataRow();
                    rd.dataItems.Add("" + ldr.depth);
                    foreach (double d in ldr.rowData)
                    {
                        rd.dataItems.Add("" + d);
                    }
                    dt.Add(rd);
                }

                ImportDataPreview.ResetTable(dt, true);

            }
            else
            {
                var rawFileReader = new RawFileReader(',');
                List<RawDataRow> dt = rawFileReader.LoadRawDataForPreview(inputFilename, ares);
                ImportDataPreview.ResetTable(dt, firstLineIsHeader);

            }
        }

        private void FileSelected(string fn)
        {
            if (fn != null && fn.Trim().Length > 0)
            {
                LoadTextDataForPreview(fn);
                // parse the block model file
                workerLoadData = new BackgroundWorker();
                workerLoadData.WorkerReportsProgress = true;
                workerLoadData.WorkerSupportsCancellation = true;
                workerLoadData.DoWork += bw_DoWork;
                // Method to call when Progress has changed
                workerLoadData.ProgressChanged += bw_ProgressChanged;
                // Method to run after BackgroundWorker has completed?
                workerLoadData.RunWorkerCompleted += bw_RunWorkerCompleted;
                workerLoadData.RunWorkerAsync(fn);
                this.Title = titleText + " - " + fn;

            }

        }

        private void GeophysicsFileSelected(string fn)
        {
            //string fn =  this.SelectedBMFile;// FileSelector.GetSelectedFilename();
            if (fn != null && fn.Trim().Length > 0)
            {
                LoadGeophysiscsTextDataForPreview(fn);
                this.Title = titleText + " - " + fn;
            }

        }


        void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            workerLoadData.ReportProgress(0);
            string fileToLoad = (string)e.Argument;
            LoadFileForPreview(fileToLoad);
            workerLoadData.ReportProgress(50);
        }

        private void LoadFileForPreview(string fileToLoad)
        {
            IOResults ares = new IOResults();

            bool firstLineIsHeader = true;// (bool)dataEntryForm.checkBoxModelFirstRowHeader.IsChecked;
            var rawFileReader = new RawFileReader((fileToLoad.ToLower().IndexOf(".csv") > -1) ? ',' : '\t');
            List<RawDataRow> dt = rawFileReader.LoadRawDataForPreview(fileToLoad, ares);
            rawFileReader.PerformColumnLoad(fileToLoad, ares, rawFileReader.MaxCols, firstLineIsHeader, workerLoadData);
            string ss = rawFileReader.GetColumnStats();
            List<string> res = rawFileReader.DetermineColumnDataTypes();
            columnDefs = new ModelColumnDefinitions();
            // collect column assignments here
            rawFileReader.SetColumnDefinitions(columnDefs);
        }

        void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBar1.Value = 0;
            progressBar1.IsIndeterminate = false;
            StatusLabel.Content = "Input loaded";
            dataLoadComplete = true;
            BlockDimensionsControl.Reset();

        }

        void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            string msg = (string)e.UserState;
            StatusLabel.Content = e.ProgressPercentage + "% " + msg;
            progressBar1.Value = e.ProgressPercentage;
        }

        public bool dataLoadComplete { get; set; }

        /// <summary>
        /// Column are mapped
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BlockModelPreview_ColumnsMapped(object sender, EventArgs e)
        {
            // set the  primary data type
            RibbonSelectionChanged(null, null);

            // collect a column map and present it in the column map table 
            ImportDataMap impMap = ImportDataPreview.GenerateColumnMap();
            if (impMap != null)
            {
                MapConfigTable.SetMap(impMap);
                // try and guess various propoerties of the model
                PresetDimensionData(impMap);
            }
        }

        private void PresetDimensionData(ImportDataMap impMap)
        {
            var rawFileReader = new RawFileReader((SelectedFile.ToLower().IndexOf(".csv") > -1) ? ',' : '\t');
            int cxColumnID = impMap.GetColumnIDMappedTo("CentroidX");
            int cyColumnID = impMap.GetColumnIDMappedTo("CentroidY");
            int czColumnID = impMap.GetColumnIDMappedTo("CentroidZ");

            int xincColumnID = impMap.GetColumnIDMappedTo("LegthX");
            int yincColumnID = impMap.GetColumnIDMappedTo("LengthY");
            int zincColumnID = impMap.GetColumnIDMappedTo("LengthZ");
            PhysicalDimensions pd = new PhysicalDimensions();
            if (cxColumnID > -1)
            {
                ColumnStats xOrigin = rawFileReader.GetDimensions(cxColumnID);
                pd.originX = xOrigin.min;
            }
            if (cyColumnID > -1)
            {
                ColumnStats yOrigin = rawFileReader.GetDimensions(cyColumnID);
                pd.originY = yOrigin.min;
            }
            if (czColumnID > -1)
            {
                ColumnStats zOrigin = rawFileReader.GetDimensions(czColumnID);
                pd.originZ = zOrigin.min;
            }
            if (xincColumnID > -1)
            {
                ColumnStats xInc = rawFileReader.GetDimensions(xincColumnID);
                pd.blockXWidth = xInc.max;
            }
            if (yincColumnID > -1)
            {
                ColumnStats yInc = rawFileReader.GetDimensions(yincColumnID);
                pd.blockYWidth = yInc.max;
            }
            if (zincColumnID > -1)
            {
                ColumnStats zInc = rawFileReader.GetDimensions(zincColumnID);
                pd.blockZWidth = zInc.max;
            }
            BlockDimensionsControl.SetBlockDimensions(pd);
        }


        ////////////////////////////////////
        ///////////////////////////////////
        // COMMANDS



        private void OpenBMCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void OpenBMExecuted(object sender, ExecutedRoutedEventArgs e)
        {


            string fileName = UIFileUtils.ShowOpenFileChoose(FileExtension, FileDescription, SelectedFile);
            if (fileName == null)
            {
                //  SelectedInputFile.Text = "";
                LabelLoadedFile.Content = "no file loaded";
            }
            else
            {
                ResetUI();
                SelectedImportType = GeneralParameters.BLOCKMODEL;
                SelectedFile = fileName;
                FileSelected(fileName);
                LabelLoadedFile.Content = fileName;
                SetRibbonEnabledStatus(GeneralParameters.BLOCKMODEL);
            }

            e.Handled = true;
        }

        private void OpenBMFormatCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.SelectedImportType == GeneralParameters.BLOCKMODEL)
            {
                e.CanExecute = true;
            }
            else
            {
                e.CanExecute = false;
            }
            e.Handled = true;
        }

        private void OpenFormatExecuted(object sender, ExecutedRoutedEventArgs e)
        {

            OpenFormatDataFile();
            e.Handled = true;
        }

        public void OpenFormatDataFile()
        {
            string fileName = UIFileUtils.ShowOpenFileChoose(FormatFileExtension, FormatFileDescription, SelectedFormatFile);
            if (fileName == null)
            {
                LabelLoadedFile.Content = "no file loaded";
            }
            else
            {
                ResetFormatData();
                SelectedFormatFile = fileName;
                ImportDataMap idm = MapConfigTable.LoadFormatFile(fileName);
                try
                {
                    FormatLoadStatus fms = ImportDataPreview.SetMappingFromImportDataMap(idm);
                    if (fms.LoadStatus == FormatLoadStatus.LOAD_OK)
                    {
                        if (fms.MappingStatus != FormatLoadStatus.MAPPING_ASSIGNEMNT_OK)
                        {

                            string messages = fms.GenerateUserMessages();
                            MessageBox.Show(messages, "Warnings during loading mapping file");

                        }
                    }                   
                    PresetDimensionData(idm);
                }
                catch (Exception ex)
                {

                    MessageBox.Show("There was an error applying the selected format file to the data.\nMake sure the format file you select is appropriate for hte input data.");

                }
            }


        }


        private void SaveFormatCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.SelectedImportType == GeneralParameters.BLOCKMODEL)
            {
                e.CanExecute = true;
            }
            else
            {
                e.CanExecute = false;
            }
            e.Handled = true;
        }

        private void SaveFormatExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            SaveFormatFile((SelectedFile.ToLower().IndexOf(".csv") > -1) ? ',' : '\t', ImportDataPreview.MaxColumns);
            e.Handled = true;
        }

        private void SaveFormatFile(char delimiter, int maxCols)
        {
            string fileName = UIFileUtils.ShowSaveFileChoose(FormatFileExtension, FormatFileDescription, SelectedFormatFile);
            if (fileName == null)
            {

            }
            else
            {
                SelectedFormatFile = fileName;
                MapConfigTable.SaveFormat(fileName, MapConfigTable.bmPrimaryTableName, delimiter, maxCols);
            }

        }


        private void BMImportCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.SelectedImportType == GeneralParameters.BLOCKMODEL)
            {
                e.CanExecute = true;
            }
            else
            {
                e.CanExecute = false;
            }
            e.Handled = true;
        }

        private void BMImportExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            ImportDataMap importMap = MapConfigTable.GetImportDataMap(SelectedFile, MapConfigTable.bmPrimaryTableName, (SelectedFile.ToLower().IndexOf(".csv") > -1) ? ',' : '\t', ImportDataPreview.MaxColumns);
            Guid gg = (Guid)ComboBoxProjectList.SelectedValue;
            // get the selected project ID
            NKDProjectID = gg;
            blockModellName = textBoxModelName.Text;
            workerBMDataImport = new BackgroundWorker();
            workerBMDataImport.WorkerReportsProgress = true;
            workerBMDataImport.WorkerSupportsCancellation = false;
            workerBMDataImport.DoWork += bw_DoBMImportWork;
            // Method to call when Progress has changed
            workerBMDataImport.ProgressChanged += bw_BMImportProgressChanged;
            // Method to run after BackgroundWorker has completed?
            workerBMDataImport.RunWorkerCompleted += bw_BMImportRunWorkerCompleted;
            workerBMDataImport.RunWorkerAsync(importMap);
            e.Handled = true;

        }

        private void bw_DoBMImportWork(object sender, DoWorkEventArgs e)
        {
            ImportDataMap importMap = (ImportDataMap)e.Argument;
            commandDirector.SetCurrentWorkerThread(workerBMDataImport);
            var rawFileReader = new RawFileReader((SelectedFile.ToLower().IndexOf(".csv") > -1) ? ',' : '\t');
            bool status = commandDirector.DoBMImport(SelectedFile, SelectedFormatFile, importMap, rawFileReader, NKDProjectID.ToString(), blockModellName);

        }

        private void bw_BMImportRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            progressBar1.Value = 0;
            StatusLabel.Content = "Block model load complete";

        }

        private void bw_BMImportProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            string msg = (string)e.UserState;
            StatusLabel.Content = msg;
            int pp = e.ProgressPercentage;
            progressBar1.Value = pp;
        }

        private void HelpCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void HelpExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBox.Show("Help.");
            e.Handled = true;
        }


        private void LASImportCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.SelectedImportType == GeneralParameters.LAS)
            {
                e.CanExecute = true;
            }
            else
            {
                e.CanExecute = false;
            }
            e.Handled = true;
        }

        private void BatchImportLASCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void BatchImportLASExecuted(object sender, ExecutedRoutedEventArgs e)
        {

            string[] ss = fileListUIControlLAS.RetrieveFileList();
            if (ComboBoxProjectList.SelectedValue == null)
            {
                ComboBoxProjectList.BorderBrush = Brushes.Red;
                MessageBox.Show("You must select a project before importing");
                return;
            }
            if (ss == null || ss.Length == 0)
            {
                fileListUIControlLAS.BorderBrush = Brushes.Red;
                MessageBox.Show("You must select some LAS files before importing");
                return;

            }
            fileListUIControlLAS.BorderBrush = Brushes.Black;
            Guid gg = (Guid)ComboBoxProjectList.SelectedValue;
            // get the selected project ID
            NKDProjectID = gg;


            workerLASBatchDataImport = new BackgroundWorker();
            workerLASBatchDataImport.WorkerReportsProgress = true;
            workerLASBatchDataImport.WorkerSupportsCancellation = false;
            workerLASBatchDataImport.DoWork += bw_DoLASBatchImportWork;
            // Method to call when Progress has changed
            workerLASBatchDataImport.ProgressChanged += bw_ProgressChanged;
            // Method to run after BackgroundWorker has completed?
            workerLASBatchDataImport.RunWorkerCompleted += bw_LASBatchImportRunWorkerCompleted;


            workerLASBatchDataImport.RunWorkerAsync(ss);
            e.Handled = true;
        }

        private void bw_LASBatchImportRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ImportStatusWindow ii = new ImportStatusWindow();
            ii.SetData(latestImportUpdateStatus);
            ii.ShowDialog();
        }

        private void bw_DoLASBatchImportWork(object sender, DoWorkEventArgs e)
        {
            string[] ss = (string[])e.Argument;
            commandDirector.SetCurrentWorkerThread(workerLASBatchDataImport);
            ModelImportStatus status = commandDirector.BatchImportLasFiles(ss, NKDProjectID);
            latestImportUpdateStatus = status;
            workerLASBatchDataImport.ReportProgress((int)0, "");
        }

        private void LASImportExecuted(object sender, ExecutedRoutedEventArgs e)
        {

            //string pathValue = "";
            //string[] filePaths = Directory.GetFiles(@pathValue, "*.LAS");


            //string outFile = "";

            //LASBatchImportTools ll = new LASBatchImportTools();
            //List<string> messages = new List<string>();
            //int importCount = 0;
            //int failCount = 0;
            //string report = "";
            //foreach (string file in filePaths) {

            //    NKD.Import.Client.Processing.LASImport li = new NKD.Import.Client.Processing.LASImport();
            //    LASFile lf = li.GetLASFile(file);


            //    string msg = ll.ProcessLASFile(lf, file);
            //    if (msg != null)
            //    {
            //        messages.Add(msg);
            //        report += msg + "\n";
            //        failCount++;

            //    }
            //    else {
            //        importCount++;
            //    }

            //}

            //string finalReport = "Immport status:\nFiles imported:" + importCount + "\nFailed files:" + failCount + "\n\nMessages:\n";

            //finalReport += report; 

            int x = 0;



        }

        private void SaveLASFormatCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.SelectedImportType == GeneralParameters.LAS)
            {
                e.CanExecute = true;
            }
            else
            {
                e.CanExecute = false;
            }
            e.Handled = true;
        }

        private void SaveLASFormatExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            SaveFormatFile((SelectedFile.ToLower().IndexOf(".csv") > -1) ? ',' : '\t', ImportDataPreview.MaxColumns);

            e.Handled = true;
        }

        private void OpenLASFormatCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.SelectedImportType == GeneralParameters.LAS)
            {
                e.CanExecute = true;
            }
            else
            {
                e.CanExecute = false;
            }
            e.Handled = true;
        }

        private void OpenLASFormatExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFormatDataFile();
        }

        private void OpenLASCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {

            e.CanExecute = true;
            e.Handled = true;
        }

        private void OpenLASExecuted(object sender, ExecutedRoutedEventArgs e)
        {


            string fileName = UIFileUtils.ShowOpenFileChoose(LASFileExtension, LASFileDescription, SelectedFile);
            if (fileName == null)
            {
                //  SelectedInputFile.Text = "";
                LabelLoadedFile.Content = "no file loaded";
            }
            else
            {
                ResetUI();
                SelectedImportType = GeneralParameters.LAS;
                SelectedFile = fileName;
                GeophysicsFileSelected(fileName);
                LabelLoadedFile.Content = fileName;
                SetRibbonEnabledStatus(GeneralParameters.LAS);
            }




            e.Handled = true;

        }


        private void CollarImportCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.SelectedImportType == GeneralParameters.COLLAR)
            {
                e.CanExecute = true;
            }
            else
            {
                e.CanExecute = false;
            }
            e.Handled = true; ;
        }

        private void CollarImportExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (ComboBoxProjectList.SelectedValue == null)
            {
                ComboBoxProjectList.BorderBrush = Brushes.Red;
                MessageBox.Show("You must select a project before importing");
                return;
            }
            Guid NKDProjectID = (Guid)ComboBoxProjectList.SelectedValue;
            bool overwrite = (bool)checkBoxOverwrite.IsChecked;


            ImportDataMap importMap = MapConfigTable.GetImportDataMap(SelectedFile, MapConfigTable.collarPrimaryTableName, (SelectedFile.ToLower().IndexOf(".csv") > -1) ? ',' : '\t', ImportDataPreview.MaxColumns);
            // add into map details of which columns are foreign keys
            if (collarDBFields != null)
            {
                importMap.UpdateWithFKInof(collarDBFields);
            }

            // get the selected project ID

            var rawFileReader = new RawFileReader((SelectedFile.ToLower().IndexOf(".csv") > -1) ? ',' : '\t');
            ModelImportStatus status = commandDirector.DoCollarImport(SelectedFile, SelectedFormatFile, importMap, rawFileReader, NKDProjectID, overwrite);
            latestImportUpdateStatus = status;
            //if(status.finalErrorCode != ModelImportStatus.OK){
            //    string ss = status.GenerateStringMessage(true);
            //    lastestImportUpdateStatus.SaveReportData();
            //    if (status.finalErrorCode == ModelImportStatus.DATA_CONSISTENCY_ERROR)
            //    {
            //        string headline = "Import complete.  " + status.linesReadFromSource + " data lines read, " + status.recordsAdded + " new records added \n" + status.recordsUpdated + " existing records updated.";
            //        MessageBox.Show("Warnings issued during import.\n\n" + headline + ".\n\n" + ss);
            //    }
            //    else
            //    {
            //        MessageBox.Show("Import failed.  " + ss);
            //    }

            //}else{

            //    MessageBox.Show("Import complete.  "+status.linesReadFromSource+" data lines read, "+status.recordsAdded+" new records added \n"+status.recordsUpdated+" existing records updated.");

            //}

            if (latestImportUpdateStatus != null)
            {
                ImportStatusWindow ii = new ImportStatusWindow();
                ii.SetData(latestImportUpdateStatus);
                ii.ShowDialog();
            }

            //workerBMDataImport = new BackgroundWorker();
            //workerBMDataImport.WorkerReportsProgress = true;
            //workerBMDataImport.WorkerSupportsCancellation = false;
            //workerBMDataImport.DoWork += bw_DoCollarImportWork;
            //// Method to call when Progress has changed
            //workerBMDataImport.ProgressChanged += bw_BMImportProgressChanged;
            //// Method to run after BackgroundWorker has completed?
            //workerBMDataImport.RunWorkerCompleted += bw_BMImportRunWorkerCompleted;
            //workerBMDataImport.RunWorkerAsync(importMap);
            e.Handled = true;

        }

        private void SaveCollarFormatCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.SelectedImportType == GeneralParameters.COLLAR)
            {
                e.CanExecute = true;
            }
            else
            {
                e.CanExecute = false;
            }
            e.Handled = true;
        }

        private void SaveCollarFormatExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            SaveFormatFile((SelectedFile.ToLower().IndexOf(".csv") > -1) ? ',' : '\t', ImportDataPreview.MaxColumns);

            e.Handled = true;
        }

        private void OpenCollarFormatCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.SelectedImportType == GeneralParameters.COLLAR)
            {
                e.CanExecute = true;
            }
            else
            {
                e.CanExecute = false;
            }
            e.Handled = true;
        }

        private void OpenCollarFormatExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            SelectedImportType = GeneralParameters.COLLAR;
            OpenFormatDataFile();

            e.Handled = true;
        }

        private void OpenCollarCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void OpenCollarExecuted(object sender, ExecutedRoutedEventArgs e)
        {


            string fileName = UIFileUtils.ShowOpenFileChoose(FileExtension, FileDescription, SelectedFile);
            if (fileName == null)
            {
                //  SelectedInputFile.Text = "";
                LabelLoadedFile.Content = "no file loaded";
            }
            else
            {
                ResetUI();
                SelectedImportType = GeneralParameters.COLLAR;
                SelectedFile = fileName;
                FileSelected(fileName);
                LabelLoadedFile.Content = fileName;
                SetRibbonEnabledStatus(GeneralParameters.COLLAR);


            }

            e.Handled = true;
        }


        private void ReSetRibbonEnabledStatus(bool to)
        {
            // set the other tabs as empty
            RibbonTabSurveyFile.IsEnabled = to;
            RibbonTabBlockModel.IsEnabled = to;
            RibbonTabLithoFile.IsEnabled = to;
            RibbonTabAssayFile.IsEnabled = to;
            RibbonTabCollarFile.IsEnabled = to;
            RibbonTabLASFile.IsEnabled = to;
            RibbonTabMultiLAS.IsEnabled = to;
        }

        private void SetRibbonEnabledStatus(int selectedType)
        {

            ReSetRibbonEnabledStatus(false);

            if (selectedType == GeneralParameters.COLLAR)
            {
                RibbonTabCollarFile.IsEnabled = true;
            }
            else if (selectedType == GeneralParameters.SURVEY)
            {
                RibbonTabSurveyFile.IsEnabled = true;
            }
            else if (selectedType == GeneralParameters.BLOCKMODEL)
            {
                RibbonTabBlockModel.IsEnabled = true;
            }
            else if (selectedType == GeneralParameters.LITHO)
            {
                RibbonTabLithoFile.IsEnabled = true;
            }
            else if (selectedType == GeneralParameters.LAS)
            {
                RibbonTabLASFile.IsEnabled = true;
            }
            else if (selectedType == GeneralParameters.ASSAY)
            {
                RibbonTabAssayFile.IsEnabled = true;
            }


        }

        public string SelectedFile { get; set; }




        private void LoadLASTextDataForPreview(string inputFilename)
        {
            IOResults ares = new IOResults();

            List<ColumnMetaInfo> mandatoryColumns = new List<ColumnMetaInfo>();
            List<ColumnMetaInfo> optionalColumns = new List<ColumnMetaInfo>();

            List<ColumnMetaInfo> dbFields = GetGeophysicsFieldsFromNKD();
            foreach (ColumnMetaInfo s in dbFields)
            {
                mandatoryColumns.Add(s);
            }


            // talk to the database to get the column names 

            ImportDataPreview.SetMandatoryMappingColumns(mandatoryColumns);
            ImportDataPreview.SetOptionalMappingColumns(optionalColumns);
            ImportDataPreview.SetPreviewType("MODEL");

            bool firstLineIsHeader = true;
            var rawFileReader = new RawFileReader((inputFilename.ToLower().IndexOf(".csv") > -1) ? ',' : '\t');
            List<RawDataRow> dt = rawFileReader.LoadRawDataForPreview(inputFilename, ares);
            ImportDataPreview.ResetTable(dt, firstLineIsHeader);

        }

        private List<ColumnMetaInfo> GetGeophysicsFieldsFromNKD()
        {
            BaseImportTools bit = new BaseImportTools();
            List<ColumnMetaInfo> ls = bit.GetGeophysicsColumns();

            return ls;
        }

        private List<ColumnMetaInfo> GetSurveyFieldsFromNKD()
        {
            BaseImportTools bit = new BaseImportTools();
            List<ColumnMetaInfo> ls = bit.GetSurveyColumns(CommandDirector.ConnectionString);

            return ls;
        }

        private List<ColumnMetaInfo> GetLithoFieldsFromNKD()
        {
            BaseImportTools bit = new BaseImportTools();
            List<ColumnMetaInfo> ls = bit.GetLithoColumns(CommandDirector.ConnectionString);

            return ls;
        }

        private void Ribbon_ContextMenuOpening_1(object sender, ContextMenuEventArgs e)
        {

        }

        private void RibbonTab_ContextMenuOpening_1(object sender, ContextMenuEventArgs e)
        {
            string ss = e.ToString();
            return;
        }

        private void RibbonTabBlockModel_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            string ss = e.ToString();
            return;
        }


        private void AppRibbon_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            int x = 0;

        }

        private void AppRibbon_MouseUp(object sender, MouseButtonEventArgs e)
        {
            int x = 0;
            RibbonSelectionChanged(sender, null);
        }

        private void RibbonSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            groupBoxInput.Visibility = Visibility.Visible;
            ImportDataPreview.Visibility = Visibility.Visible;
            MapConfigTable.Visibility = Visibility.Visible;
            groupBoxMapConfigTable.Visibility = Visibility.Visible;
            groupBoxBatchLas.Visibility = Visibility.Hidden;

            if (ButtonOpenBM.IsVisible)
            {
                ModelDefGroupBox.Visibility = Visibility.Visible;
                ImportDataPreview.targetMainDataType = MapConfigTable.bmPrimaryTableName;
            }
            else
            {
                ModelDefGroupBox.Visibility = Visibility.Hidden;
            }

            if (ButtonOpenCollar.IsVisible)
            {
                ImportDataPreview.targetMainDataType = MapConfigTable.collarPrimaryTableName;
            }

            if (ButtonImportLAS.IsVisible)
            {
                ImportDataPreview.targetMainDataType = MapConfigTable.geophyiscsPrimaryTableName;
            }

            if (ButtonImportAssay.IsVisible)
            {
                ImportDataPreview.targetMainDataType = MapConfigTable.assayPrimaryTableName;
            }
            if (ButtonImportSurvey.IsVisible)
            {
                ImportDataPreview.targetMainDataType = MapConfigTable.surveyPrimaryTableName;
            }
            if (ButtonImportLitho.IsVisible)
            {
                ImportDataPreview.targetMainDataType = MapConfigTable.lithoPrimaryTableName;
            }

            if (ButtonImportBatchLAS.IsVisible)
            {
                ImportDataPreview.targetMainDataType = MapConfigTable.geophyiscsPrimaryTableName;

                //groupBoxInput.Visibility = Visibility.Hidden;
                ImportDataPreview.Visibility = Visibility.Hidden;
                MapConfigTable.Visibility = Visibility.Hidden;
                groupBoxMapConfigTable.Visibility = Visibility.Hidden;

                groupBoxBatchLas.Visibility = Visibility.Visible;
            }
            if (ButtonImportCoalQuality.IsVisible)
            {
                ImportDataPreview.targetMainDataType = MapConfigTable.coalqualPrimaryTableName; //.lithoPrimaryTableName;
            }

            return;
        }

        private void MenuItemAbout_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("NKD Importer version " + versionNumber + " alpha. " + releaseDate);
        }

        private void MenuItemHelp_Click(object sender, RoutedEventArgs e)
        {

            // System.Windows.Forms.Help.ShowHelp(null, @"help.chm");
            HelpWindow hw = new HelpWindow();
            hw.Show();
        }

        private void MenuItemResetMapping_Click(object sender, RoutedEventArgs e)
        {
            ResetFormatData();
        }

        private void MenuItemUpdateConStr_Click(object sender, RoutedEventArgs e)
        {
            var update = new UpdateConnectionString();
            update.ShowDialog();
        }

        private void MenuItemClean_Click(object sender, RoutedEventArgs e)
        {
            ResetUI();
        }

        private void ViewProjectHoles()
        {
            // list all the holes contained within this project
            if (ComboBoxProjectList.SelectedValue != null)
            {
                ComboBoxProjectList.BorderBrush = Brushes.Black;
                Guid currentSelectedProject = (Guid)ComboBoxProjectList.SelectedValue;
                List<CollarInfo> existingHoles = commandDirector.GetHolesForProject(currentSelectedProject);
                CollarPlanView cpv = new CollarPlanView();
                cpv.SetCollarData(existingHoles);
                cpv.ShowDialog();
            }
            else
            {
                ComboBoxProjectList.BorderBrush = Brushes.Red;
                MessageBox.Show("You must select a project to view first");
            }

        }

        public bool doDuplicateCheck { get; set; }

        public string blockModellName { get; set; }
    }



}
