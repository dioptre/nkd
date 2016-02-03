using DevExpress.Accessibility;
using DevExpress.ExpressApp.DC;
using DevExpress.LookAndFeel;
using DevExpress.Persistent.Base;
using DevExpress.Skins;
using DevExpress.Utils;
using DevExpress.XtraEditors.Drawing;
using DevExpress.XtraEditors.Registrator;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraEditors.ViewInfo;
using System;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.CompilerServices;
using DevExpress.ExpressApp.Utils;
using DevExpress.ExpressApp;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Runtime.Serialization;
using DevExpress.Data.Filtering;
using DevExpress.Data.Filtering.Helpers;
using DevExpress.ExpressApp.Localization;

namespace NKD.Module.Win.Editors
{
    public class RepositoryFileEditor : RepositoryItemButtonEdit
    {
        private string fileTypesFilter = string.Empty;
        private IObjectSpace objectSpace;
        private IMemberInfo memberInfo;
        private bool fileDataReadOnly;

        private EventHandler<CreateCustomFileDataObjectEventArgs> createCustomFileDataObject;
        public event EventHandler<CreateCustomFileDataObjectEventArgs> CreateCustomFileDataObject
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            add
            {
                this.createCustomFileDataObject += value;
            }
            [MethodImpl(MethodImplOptions.Synchronized)]
            remove
            {
                this.createCustomFileDataObject -= value;
            }
        }
        internal static string EditorName
        {
            get
            {
                return typeof(FileDataEdit).FullName;
            }
        }
        public override string EditorTypeName
        {
            get
            {
                return RepositoryFileEditor.EditorName;
            }
        }
        public bool FileDataReadOnly
        {
            get
            {
                return this.fileDataReadOnly;
            }
            set
            {
                this.fileDataReadOnly = value;
            }
        }
        public IObjectSpace ObjectSpace
        {
            get
            {
                return this.objectSpace;
            }
            set
            {
                this.objectSpace = value;
            }
        }
        public IMemberInfo MemberInfo
        {
            get
            {
                return this.memberInfo;
            }
            set
            {
                this.memberInfo = value;
            }
        }
        public string FileTypesFilter
        {
            get
            {
                return this.fileTypesFilter;
            }
            set
            {
                this.fileTypesFilter = value;
            }
        }

        internal static void Register()
        {
            if (!EditorRegistrationInfo.Default.Editors.Contains(RepositoryFileEditor.EditorName))
            {
                EditorRegistrationInfo.Default.Editors.Add(new EditorClassInfo(RepositoryFileEditor.EditorName, typeof(FileDataEdit), typeof(RepositoryFileEditor), typeof(ButtonEditViewInfo), new ButtonEditPainter(), true, EditImageIndexes.ButtonEdit, typeof(ButtonEditAccessible)));
            }
        }
        static RepositoryFileEditor()
        {
            RepositoryFileEditor.Register();
        }
        public IFileData CreateFileDataObject()
        {
            IFileData fileData = CreateFileData(this.objectSpace, this.memberInfo);
            CreateCustomFileDataObjectEventArgs createCustomFileDataObjectEventArgs = new CreateCustomFileDataObjectEventArgs(fileData);
            if (createCustomFileDataObject != null)
            {
                createCustomFileDataObject(this, createCustomFileDataObjectEventArgs);
            }
            return createCustomFileDataObjectEventArgs.FileData;
        }
        public RepositoryFileEditor()
        {
            this.InitializeAppearance();
        }
        internal void InitializeAppearance()
        {
            Color foreColor = Color.Empty;
            if (this.LookAndFeel.ActiveStyle == ActiveLookAndFeelStyle.Skin)
            {
                Skin skin = EditorsSkins.GetSkin(this.LookAndFeel);
                if (skin != null)
                {
                    foreColor = skin.Colors.GetColor(EditorsSkins.SkinHyperlinkTextColor);
                }
            }
            if (foreColor.IsEmpty)
            {
                foreColor = Color.Blue;
            }
            this.Appearance.ForeColor = foreColor;
            this.Appearance.Font = new Font(this.Appearance.Font, FontStyle.Underline);
        }
        public override void Assign(RepositoryItem item)
        {
            base.Assign(item);
            if (item is RepositoryFileEditor)
            {
                RepositoryFileEditor repositoryItemFileDataEdit = (RepositoryFileEditor)item;
                this.fileDataReadOnly = repositoryItemFileDataEdit.fileDataReadOnly;
                this.createCustomFileDataObject = repositoryItemFileDataEdit.createCustomFileDataObject;
                this.fileTypesFilter = repositoryItemFileDataEdit.fileTypesFilter;                
                this.ObjectSpace = repositoryItemFileDataEdit.ObjectSpace;
                this.MemberInfo = repositoryItemFileDataEdit.MemberInfo;
            }
        }
        public override string GetDisplayText(FormatInfo format, object editValue)
        {
            return ReflectionHelper.GetObjectDisplayText(editValue);
        }

        public static IFileData CreateFileData(IObjectSpace objectSpace, IMemberInfo memberDescriptor)
        {
            if (!memberDescriptor.IsReadOnly)
            {
                return objectSpace.CreateObject(memberDescriptor.MemberType) as IFileData;
            }
            throw new InvalidOperationException("Unable to create file.");
        }

        private Boolean HasConstructorWithSession(Type type)
        {
            return type.GetConstructor(new Type[] { typeof(DevExpress.Xpo.Session) }) != null;
        }

        private EventHandler<CustomFileOperationEventArgs> customOpenFileWithDefaultProgram;
        public event EventHandler<CustomFileOperationEventArgs> CustomOpenFileWithDefaultProgram
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            add
            {
                this.customOpenFileWithDefaultProgram += value;
            }
            [MethodImpl(MethodImplOptions.Synchronized)]
            remove
            {
                this.customOpenFileWithDefaultProgram -= value;
            }
        }

        private void OnCustomOpenFileWithDefaultProgram(CustomFileOperationEventArgs args)
        {
            if (this.customOpenFileWithDefaultProgram != null)
            {
                this.customOpenFileWithDefaultProgram(this, args);
            }
        }
        public void Open(IFileData fileData)
        {
            DevExpress.ExpressApp.Utils.Guard.ArgumentNotNull(fileData, "fileData");
            if (!FileDataHelper.IsFileDataEmpty(fileData))
            {
                CustomFileOperationEventArgs customFileOperationEventArgs = new CustomFileOperationEventArgs(fileData);
                this.OnCustomOpenFileWithDefaultProgram(customFileOperationEventArgs);
                if (!customFileOperationEventArgs.Handled)
                {
                    string text = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("B"));
                    try
                    {
                        Directory.CreateDirectory(text);
                    }
                    catch
                    {
                        Tracing.Tracer.LogValue("tempDirectory", text);
                        throw;
                    }
                    string text2 = Path.Combine(text, fileData.FileName);
                    try
                    {
                        using (FileStream fileStream = new FileStream(text2, FileMode.CreateNew))
                        {
                            fileData.SaveToStream(fileStream);
                        }
                        Process.Start(text2);
                    }
                    catch
                    {
                        Tracing.Tracer.LogValue("tempFileName", text2);
                        throw;
                    }
                }
            }
        }
        public void Save(IFileData fileData)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.CreatePrompt = false;
                saveFileDialog.OverwritePrompt = true;
                string text = Path.GetExtension(fileData.FileName).TrimStart(new char[]
				{
					'.'
				});
                string localizedText = CaptionHelper.GetLocalizedText("FileAttachments", "WordAll");
                string localizedText2 = CaptionHelper.GetLocalizedText("FileAttachments", "WordFiles");
                saveFileDialog.Filter = string.Concat(new string[]
				{
					text.ToUpper(),
					" ",
					localizedText2,
					" (*.",
					text,
					")|*.",
					text,
					"|",
					localizedText,
					" ",
					localizedText2,
					" (*.*)|*.*"
				});
                //IModelOptionsFileAttachments modelOptionsFileAttachments = base.Application.Model.Options as IModelOptionsFileAttachments;
                //saveFileDialog.InitialDirectory = modelOptionsFileAttachments.Attachments.DefaultDirectory;
                saveFileDialog.FileName = fileData.FileName;
                saveFileDialog.Title = CaptionHelper.GetLocalizedText("FileAttachments", "OverwritePromptCaption");
                if (saveFileDialog.ShowDialog(Form.ActiveForm) == DialogResult.OK)
                {
                   //modelOptionsFileAttachments.Attachments.DefaultDirectory = Path.GetDirectoryName(saveFileDialog.FileName);
                    using (FileStream fileStream = new FileStream(saveFileDialog.FileName, FileMode.Create))
                    {
                        fileData.SaveToStream(fileStream);
                    }
                }
            }
        }
        //void IFileDataManager.SaveFiles(List<IFileData> fileDataList)
        //{
        //    DevExpress.ExpressApp.Utils.Guard.ArgumentNotNull(fileDataList, "fileData");
        //    CustomFileListOperationEventArgs customFileListOperationEventArgs = new CustomFileListOperationEventArgs(fileDataList);
        //    this.OnCustomSaveFiles(customFileListOperationEventArgs);
        //    if (!customFileListOperationEventArgs.Handled)
        //    {
        //        if (fileDataList.Count == 1)
        //        {
        //            ((IFileDataManager)this).Save(fileDataList[0]);
        //            return;
        //        }
        //        using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
        //        {
        //            IModelOptionsFileAttachments modelOptionsFileAttachments = base.Application.Model.Options as IModelOptionsFileAttachments;
        //            folderBrowserDialog.SelectedPath = modelOptionsFileAttachments.Attachments.DefaultDirectory;
        //            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
        //            {
        //                string selectedPath = folderBrowserDialog.SelectedPath;
        //                foreach (IFileData current in fileDataList)
        //                {
        //                    string text = Path.Combine(selectedPath, current.FileName);
        //                    if (!this.CancelFile(text))
        //                    {
        //                        using (FileStream fileStream = new FileStream(text, FileMode.Create))
        //                        {
        //                            current.SaveToStream(fileStream);
        //                        }
        //                    }
        //                }
        //                modelOptionsFileAttachments.Attachments.DefaultDirectory = selectedPath;
        //            }
        //        }
        //    }
        //}

    }
}
