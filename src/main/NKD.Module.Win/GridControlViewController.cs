using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Columns;

namespace WinSolution.Module.Win {
    public partial class GridControlViewController : ViewController {
        public GridControlViewController() {
            InitializeComponent();
            RegisterActions(components);
        }
        protected override void OnActivated() {
            base.OnActivated();
            if (View is ListView)  // && View.ObjectTypeInfo.Type == typeof(Header)) 
            {
                View.ControlsCreated += new EventHandler(View_ControlsCreated);
            }
        }
        protected override void OnDeactivated() {
            base.OnDeactivated();
            if (View is ListView)  //&& View.ObjectTypeInfo.Type == typeof(Header))
            {
                View.ControlsCreated -= new EventHandler(View_ControlsCreated);
            }
        }
        void View_ControlsCreated(object sender, EventArgs e) {
            GridControl grid = (GridControl)View.Control;
            grid.HandleCreated += new EventHandler(grid_HandleCreated);
        }
        void grid_HandleCreated(object sender, EventArgs e) {
            GridControl grid = (GridControl)View.Control;
            GridView view = (GridView)grid.FocusedView;
            SetBestFitForColumns(view);                
        }
        private void SetBestFitForColumns(GridView gridView) {
            if (gridView != null) {
                //Calculate the estimated best width
                int estimatedBestWidth = 0;

                //To avoid a full grid scan, only use the 20 first rows to set the best fit
                gridView.BestFitMaxRowCount = 20;

                foreach (GridColumn column in gridView.Columns) {
                    estimatedBestWidth += column.GetBestWidth();
                }

                //If the calculated best width is greater than the grid width
                if (estimatedBestWidth > gridView.GridControl.Width || true) {
                    //Disable the column autowidth (which is true by default) to allow the scrollbar to appear
                    gridView.OptionsView.ColumnAutoWidth = false;

                    //And force the columns to resize to its best width
                    gridView.BestFitColumns();
                }
            }
        }
    }
}
