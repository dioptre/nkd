using System;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Web.Editors;
using DevExpress.Web.ASPxEditors;
using DevExpress.ExpressApp.Web.Editors.ASPx;
using DevExpress.Persistent.Base;

namespace NKD.Module.Win
{
    [PropertyEditor(typeof(Decimal), false)]
    public class GeoDecimalEditor : ASPxDecimalPropertyEditor
    {                
        public GeoDecimalEditor(Type objectType, DevExpress.ExpressApp.Model.IModelMemberViewItem model)
            : base(objectType, model)
        {
            this.DisplayFormat = "";
        }
        [VisibleInListView(true)]
        public GeoDecimalEditor Self
        {
            get { return this; }
        }
        
        protected override void ReadEditModeValueCore()
        {
            if (ASPxEditor is ASPxSpinEdit)
            {
                ASPxSpinEdit editor = (ASPxSpinEdit)ASPxEditor;
                editor.Value = PropertyValue;
                editor.Text = GetPropertyDisplayValue();
            }
        }

        protected override void ReadValueCore()
        {
            base.ReadValueCore();
        }


    }
}

