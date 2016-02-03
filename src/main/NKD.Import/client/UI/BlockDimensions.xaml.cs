using NKD.Import.Client.Definitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NKD.Import.Client.UI
{
    /// <summary>
    /// Interaction logic for BlockDimensions.xaml
    /// </summary>
    public partial class BlockDimensions : UserControl
    {
        private bool textBoxXOrigin_isDirty = false;
        private bool textBoxYOrigin_isDirty = false;
        private bool textBoxZOrigin_isDirty = false;
        private bool textBoxXWidth_isDirty = false;
        private bool textBoxYWidth_isDirty = false;
        private bool textBoxZWidth_isDirty = false;
        
        public BlockDimensions()
        {
            InitializeComponent();
        }
        public void Reset() { 
            textBoxXOrigin_isDirty = false;
            textBoxXOrigin.Text = "";
            textBoxYOrigin_isDirty = false;
            textBoxYOrigin.Text = "";
            textBoxZOrigin_isDirty = false;
            textBoxZOrigin.Text = "";
            textBoxXWidth_isDirty = false;
            textBoxXWidth.Text = "";
            textBoxYWidth_isDirty = false;
            textBoxYWidth.Text = "";
            textBoxZWidth_isDirty = false;
            textBoxZWidth.Text = "";
        }

        public PhysicalDimensions GetBlockDimensions(){
            PhysicalDimensions pd = new PhysicalDimensions();
            pd.originX = GetValAsDouble(textBoxXOrigin);
            pd.originY = GetValAsDouble(textBoxYOrigin);
            pd.originZ = GetValAsDouble(textBoxZOrigin);
            pd.blockXWidth = GetValAsDouble(textBoxXWidth);
            pd.blockYWidth = GetValAsDouble(textBoxYWidth);
            pd.blockZWidth = GetValAsDouble(textBoxZWidth);

            return pd;
        }


        public void SetBlockDimensions(PhysicalDimensions pd)
        {

            if (!textBoxXOrigin_isDirty)
            {
                textBoxXOrigin.Text = "" + pd.originX;
            }
            if (!textBoxYOrigin_isDirty)
            {
                textBoxYOrigin.Text = "" + pd.originY;
            }
            if (!textBoxZOrigin_isDirty)
            {
                textBoxZOrigin.Text = "" + pd.originZ;
            }
            if (!textBoxXWidth_isDirty)
            {
                textBoxXWidth.Text = "" + pd.blockXWidth;
            }
            if (!textBoxYWidth_isDirty)
            {
                textBoxYWidth.Text = "" + pd.blockYWidth;
            }
            if (!textBoxZWidth_isDirty)
            {
                textBoxZWidth.Text = "" + pd.blockZWidth;
            }
        }

        private double GetValAsDouble(TextBox textBoxData)
        {
 	        string ss = textBoxData.Text;
            double res = double.MinValue;
            bool parsedOK =  double.TryParse(ss, out res);
            return res;
        }

        private void textBoxXOrigin_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            textBoxXOrigin_isDirty = true;
            if (textBoxXOrigin.Text.Trim().Length == 0) {
                textBoxXOrigin_isDirty = false;
            }

        }

        private void textBoxYOrigin_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            textBoxYOrigin_isDirty = true;
            if (textBoxYOrigin.Text.Trim().Length == 0)
            {
                textBoxYOrigin_isDirty = false;
            }
        }

        private void textBoxZOrigin_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            textBoxZOrigin_isDirty = true;
            if (textBoxZOrigin.Text.Trim().Length == 0)
            {
                textBoxZOrigin_isDirty = false;
            }
        }

        private void textBoxXWidth_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            textBoxXWidth_isDirty = true;
            if (textBoxXWidth.Text.Trim().Length == 0)
            {
                textBoxXWidth_isDirty = false;
            }
        }

        private void textBoxYWidth_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            textBoxYWidth_isDirty = true;
            if (textBoxYWidth.Text.Trim().Length == 0)
            {
                textBoxYWidth_isDirty = false;
            }
        }

        private void textBoxZWidth_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            textBoxZWidth_isDirty = true;
            if (textBoxZWidth.Text.Trim().Length == 0)
            {
                textBoxZWidth_isDirty = false;
            }
        }



    }
}
