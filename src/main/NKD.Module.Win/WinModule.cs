using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

using DevExpress.ExpressApp;

namespace NKD.Module.Win
{
    [ToolboxItemFilter("Xaf.Platform.Win")]
    public sealed partial class NKDWindowsFormsModule : ModuleBase
    {
        public NKDWindowsFormsModule()
        {
            InitializeComponent();
        }
    }
}
