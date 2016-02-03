using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.ExpressApp.Win.Editors;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Editors;
using DevExpress.Persistent.Base;

namespace NKD.Module.Win.Editors
{
    [ListEditor(typeof(object),true)]
    public class DocGridListEditor : GridListEditor
    {
        public DocGridListEditor(IModelListView model)
            : base(model)
        {          
        
    
        }     
       
    }
}
