using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Win.Editors;

namespace NKD.Module.Win.Editors
{
    [PropertyEditor(typeof(DateTime))]
    public class DateAndTimeEditor : DatePropertyEditor
    {
        public DateAndTimeEditor(Type objectType, IModelMemberViewItem item) : base(objectType, item) { }
        protected override void SetupRepositoryItem(DevExpress.XtraEditors.Repository.RepositoryItem item)
        {
            base.SetupRepositoryItem(item);
            RepositoryItemDateTimeEdit defaultItem = (RepositoryItemDateTimeEdit)item;
            defaultItem.VistaDisplayMode = DevExpress.Utils.DefaultBoolean.True;
            defaultItem.VistaEditTime = DevExpress.Utils.DefaultBoolean.True;
        }
    }
}
