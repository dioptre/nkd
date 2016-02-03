using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DevExpress.Data;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Win.Editors;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Columns;
using NKD.Module.Extenders;

namespace NKD.Module.Win
{
    public class WinGroupFooterViewController : ViewController<ListView>
    {
        //private void View_InfoSynchronized(object sender, EventArgs e)
        //{
        //    IModelListViewExtender modelListView = View.Model as IModelListViewExtender;
        //    if (modelListView != null)
        //    {
        //        GridListEditor gridListEditor = View.Editor as GridListEditor;
        //        if (gridListEditor != null)
        //        {
        //        }
        //    }
        //}
        //protected override void OnViewControlsCreated()
        //{
        //    base.OnViewControlsCreated();
        //    IModelListViewExtender modelListView = View.Model as IModelListViewExtender;
        //    if (modelListView != null)
        //    {
        //        GridListEditor gridListEditor = View.Editor as GridListEditor;
        //        if (gridListEditor != null)
        //        {
        //            GridView gridView = gridListEditor.GridView;
        //            gridView.GroupFooterShowMode = GroupFooterShowMode.VisibleAlways;
        //            foreach (IModelColumn modelColumn in View.Model.Columns)
        //            {
        //               //var x =  modelColumn.SetValue<ValueType>(
        //                    GridColumn gridColumn = gridView.Columns[modelColumn.ModelMember.MemberInfo.BindingName];
        //                    //gridView.GroupSummary.Add(modelColumnExtender.GroupFooterSummaryType, modelColumn.Id, gridColumn);
        //                    dynamic mn = modelColumn as DevExpress.ExpressApp.Model.Core.ModelNode;
                        
        //                    //mn.IsReadOnly = false;
                        
        //            }
        //        }
        //    }
        //}
        //protected override void OnActivated()
        //{
        //    base.OnActivated();
        //    View.ModelSaved += View_InfoSynchronized;
        //}
        //protected override void OnDeactivated()
        //{
        //    View.ModelSaved -= View_InfoSynchronized;
        //    base.OnDeactivated();
        //}
    }

}

