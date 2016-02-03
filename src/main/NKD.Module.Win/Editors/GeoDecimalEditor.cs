using System;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Win.Editors;
using DevExpress.Persistent.Base;

namespace NKD.Module.Win
{
    [PropertyEditor(typeof(Decimal), false)]
    public class GeoDecimalEditor : DecimalPropertyEditor
    {                
        public GeoDecimalEditor(Type objectType, DevExpress.ExpressApp.Model.IModelMemberViewItem model)
            : base(objectType, model)
        {
        }
        [VisibleInListView(true)]
        public GeoDecimalEditor Self
        {
            get { return this; }
        }
        protected override void SetupRepositoryItem(DevExpress.XtraEditors.Repository.RepositoryItem item)
        {
            base.SetupRepositoryItem(item);
            RepositoryItemDecimalEdit properties = (RepositoryItemDecimalEdit)item;
            properties.Mask.EditMask = "n";
            properties.Mask.UseMaskAsDisplayFormat = true;
        }
    }
}

