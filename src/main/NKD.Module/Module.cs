using System;
using System.Collections.Generic;
using DevExpress.ExpressApp;
using System.Reflection;
using DevExpress.ExpressApp.EF;
using DevExpress.ExpressApp.Model;
using NKD.Module.Extenders;

namespace NKD.Module
{
    public sealed partial class NKDModule : ModuleBase
    {
        public NKDModule()
        {
            InitializeComponent();
            ExportedTypeHelpers.AddExportedTypeHelper(new EFExportedTypeHelper());
        }

        public override void ExtendModelInterfaces(ModelInterfaceExtenders extenders)
        {
            base.ExtendModelInterfaces(extenders);
            extenders.Add<IModelListView, IModelListViewExtender>();
        }

    }
}
