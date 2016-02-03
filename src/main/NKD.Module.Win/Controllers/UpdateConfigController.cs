using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Xpo;
using DevExpress.ExpressApp.DC;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using DevExpress.Data.Filtering;
using NKD.Module.BusinessObjects;
using DevExpress.ExpressApp.SystemModule;
//using DevExpress.Persistent.BaseImpl;

namespace NKD.Module.Win.Controllers
{

    public class UpdateConfigController : NewObjectViewController
    {

        public UpdateConfigController()
        {
            var myAction = new SimpleAction(this, "Update Configuration", DevExpress.Persistent.Base.PredefinedCategory.Tools);
            myAction.Execute += myAction_Execute;
            myAction.ImageName = "ModelEditor_Action_Modules";
            Actions.Add(myAction);

        }

        void myAction_Execute(object sender, DevExpress.ExpressApp.Actions.SimpleActionExecuteEventArgs e)
        {
            var f = new NKD.Module.Win.Controllers.UpdateConfig();
            if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                //System.Windows.Forms.Application.Restart();
                System.Diagnostics.Process.Start(System.Windows.Forms.Application.ExecutablePath);
                Process.GetCurrentProcess().Kill();
            }
        }
    }
}
