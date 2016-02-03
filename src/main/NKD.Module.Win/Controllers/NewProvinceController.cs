using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Xpo;
using DevExpress.ExpressApp.DC;
using System.ComponentModel;
using System.Diagnostics;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using DevExpress.Data.Filtering;
using NKD.Module.BusinessObjects;
using DevExpress.ExpressApp.SystemModule;

namespace NKD.Module.Win.Controllers
{

    public partial class NewProvinceController : NewObjectViewController {

        public NewProvinceController()        
        {
            var myAction = new SimpleAction(this, "Add Province", DevExpress.Persistent.Base.PredefinedCategory.View);
            myAction.TargetObjectType = typeof(object);
            myAction.Execute += myAction_Execute;
            myAction.ImageName = "BO_Localization";
            Actions.Add(myAction);
            this.FrameAssigned += NewProvinceController_FrameAssigned;
            ObjectCreated += NewProvinceController_ObjectCreated;
            TargetViewType = ViewType.ListView;
            TargetObjectType = typeof(object);
        }

        void NewProvinceController_ObjectCreated(object sender, ObjectCreatedEventArgs e)
        {
            if (e.CreatedObject is ProvinceData)
            {
                try
                {

                    var view = sender as DevExpress.ExpressApp.ViewController;
                    if (view == null || view.View == null || view.View.SelectedObjects == null || view.View.SelectedObjects.Count < 1)
                        return;
                    if (view.View.SelectedObjects[0] is System.Data.Objects.DataClasses.EntityObject)
                    {
                        var o = (System.Data.Objects.DataClasses.EntityObject)view.View.SelectedObjects[0];
                        var c = ((DevExpress.ExpressApp.EF.EFObjectSpace)view.View.ObjectSpace).ObjectContext;
                        //var t = c.MetadataWorkspace.GetEntityContainer(c.DefaultContainerName, System.Data.Metadata.Edm.DataSpace.CSpace);
                        ((ProvinceData)e.CreatedObject).TableType = NKD.Module.BusinessObjects.BusinessObjectHelper.GetTableName(c, view.View.SelectedObjects[0].GetType());
                        ((ProvinceData)e.CreatedObject).ReferenceID = (Guid)o.EntityKey.EntityKeyValues[0].Value;
                        ((ProvinceData)e.CreatedObject).ProvinceDataID = Guid.NewGuid();

                    }
                    else if (view.View.SelectedObjects[0] is DevExpress.Xpo.XPLiteObject)
                    {
                        XPLiteObject o = view.View.SelectedObjects[0] as XPLiteObject;
                        ((ProvinceData)e.CreatedObject).ReferenceID = (Guid)o.This.GetType().GetProperty(o.ClassInfo.KeyProperty.Name).GetValue(o.This);
                        ((ProvinceData)e.CreatedObject).TableType = o.ClassInfo.TableName;
                    }
                }
                catch { }
            }
        }

        void myAction_Execute(object sender, DevExpress.ExpressApp.Actions.SimpleActionExecuteEventArgs e)
        {
            var args = new SingleChoiceActionExecuteEventArgs(e.Action, e.Action.SelectionContext, new ChoiceActionItem("NewObject", typeof(ProvinceData)));
            New(args);
            e.ShowViewParameters.Assign(args.ShowViewParameters);
        }

        void NewProvinceController_FrameAssigned(object sender, System.EventArgs e)
        {
            NewObjectViewController standardController = Frame.GetController<NewObjectViewController>();
            standardController.ObjectCreated += NewProvinceController_ObjectCreated;
        }

       
    }
   
}