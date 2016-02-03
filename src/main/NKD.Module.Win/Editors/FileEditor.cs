using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Win.Editors;
using DevExpress.Persistent.Base;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Win;
using DevExpress.XtraEditors.Repository;
using DevExpress.Data;
using DevExpress.ExpressApp;

namespace NKD.Module.Win.Editors
{
    public class CreateCustomFileDataObjectEventArgs : EventArgs
    {
        private IFileData fileData;
        public IFileData FileData
        {
            get
            {
                return this.fileData;
            }
            set
            {
                this.fileData = value;
            }
        }
        public CreateCustomFileDataObjectEventArgs(IFileData fileData)
        {
            this.fileData = fileData;
        }
    }

    [PropertyEditor(typeof(NKD.Module.BusinessObjects.FileData), true)]
    public class FileEditor : DXPropertyEditor, IComplexViewItem
    {
        public FileEditor(Type objectType, DevExpress.ExpressApp.Model.IModelMemberViewItem model)
            : base(objectType, model)
        {

        }
        [VisibleInListView(true)]
        public FileEditor Self
        {
            get { return this; }
        }


        private IObjectSpace objectSpace;
        private XafApplication application;
        //protected FileAttachmentsWindowsFormsModule FileAttachmentsWindowsFormsModule
        //{
        //    get
        //    {
        //        if (this.application != null)
        //        {
        //            return (FileAttachmentsWindowsFormsModule)this.application.Modules.FindModule(typeof(FileAttachmentsWindowsFormsModule));
        //        }
        //        return null;
        //    }
        //}
        public new FileDataEdit Control
        {
            get
            {
                return (FileDataEdit)base.Control;
            }
        }
        protected override object CreateControlCore()
        {
            return new FileDataEdit();
        }
        protected override void OnCurrentObjectChanged()
        {
            base.OnCurrentObjectChanged();
            this.RefreshReadOnly();
        }
        protected override bool IsMemberSetterRequired()
        {
            bool flag = base.IsMemberSetterRequired();
            if (flag)
            {
                return !(base.PropertyValue is IFileData);
            }
            return flag;
        }
        protected virtual void CreateFileDataObject(object sender, CreateCustomFileDataObjectEventArgs e)
        {
        }
        protected override void SetRepositoryItemReadOnly(RepositoryItem item, bool readOnly)
        {
            base.SetRepositoryItemReadOnly(item, readOnly);
            ((RepositoryFileEditor)item).FileDataReadOnly = readOnly;
        }
        protected override RepositoryItem CreateRepositoryItem()
        {
            return new RepositoryFileEditor();
        }
        private void RepositoryItemFileDataEdit_Disposed(object sender, EventArgs e)
        {
            ((RepositoryFileEditor)sender).Disposed -= new EventHandler(this.RepositoryItemFileDataEdit_Disposed);
            ((RepositoryFileEditor)sender).CreateCustomFileDataObject -= new EventHandler<CreateCustomFileDataObjectEventArgs>(this.CreateFileDataObject);
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                this.objectSpace = null;
                this.application = null;
            }
        }
        protected override void SetupRepositoryItem(RepositoryItem item)
        {
            base.SetupRepositoryItem(item);
            ((RepositoryFileEditor)item).CreateCustomFileDataObject += new EventHandler<CreateCustomFileDataObjectEventArgs>(this.CreateFileDataObject);
            ((RepositoryFileEditor)item).Disposed += new EventHandler(this.RepositoryItemFileDataEdit_Disposed);
            ((RepositoryFileEditor)item).ObjectSpace = this.objectSpace;
            ((RepositoryFileEditor)item).MemberInfo = base.MemberInfo;
            //IModelCommonFileTypeFilters modelCommonFileTypeFilters = base.Model.ModelMember as IModelCommonFileTypeFilters;
            //if (modelCommonFileTypeFilters != null)
            //{
            //    ((RepositoryFileEditor)item).FileTypesFilter = modelCommonFileTypeFilters.FileTypeFilters.FileTypesFilter;
            //}
            //((RepositoryFileEditor)item).FileDataManager = FileAttachmentsWindowsFormsModule.GetFileDataManager(this.application);
        }
        protected override void InitializeAppearance(RepositoryItem item)
        {
            base.InitializeAppearance(item);
            RepositoryFileEditor repositoryItemFileDataEdit = item as RepositoryFileEditor;
            if (repositoryItemFileDataEdit != null)
            {
                repositoryItemFileDataEdit.InitializeAppearance();
            }
        }

        public void Setup(IObjectSpace objectSpace, XafApplication application)
        {
            this.objectSpace = objectSpace;
            this.application = application;
        }
    }
}
