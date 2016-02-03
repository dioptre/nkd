using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using NKD.Import.DataWrappers;

namespace NKD.Import.Client.UI
{
    /// <summary>
    /// Interaction logic for CollarPlanView.xaml
    /// </summary>
    public partial class CollarPlanView : Window
    {
        public CollarPlanView()
        {
            InitializeComponent();
            Chart c1 = this.FindName("MyWinformChart") as Chart;
            c1.Titles.Add("");

            Font fb = c1.Titles[0].Font;
            c1.Titles[0].Font = new Font(fb.FontFamily, 10, System.Drawing.FontStyle.Bold);
            // this is a small 'Watermark' in the chart area to tag the image
            c1.Titles.Add("NakedEnterprise.Org");
            c1.Titles[1].Alignment = System.Drawing.ContentAlignment.BottomRight;
            // Set Title Docking 
            c1.Titles[1].IsDockedInsideChartArea = true;
            c1.Titles[1].Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Bottom;
            c1.Titles[1].DockedToChartArea = c1.ChartAreas[0].Name;
            c1.Titles[1].Font = new Font(fb.FontFamily, 10, System.Drawing.FontStyle.Italic);
            // Set chart title color
            c1.Titles[1].ForeColor = System.Drawing.Color.FromArgb(100, 255, 0, 0);
            c1.TextAntiAliasingQuality = TextAntiAliasingQuality.High;    
            
        }

      

        internal void SetCollarData(List<CollarInfo> _existingHoles)
        {
            existingHoles = _existingHoles;
            Chart c1 = this.FindName("MyWinformChart") as Chart;
            List<string> collarNames = new List<string>();
            c1.Titles[0].Text = "Plan view of collars";
            c1.ChartAreas[0].AxisY.Title = "Northing";
            c1.ChartAreas[0].AxisX.Title = "Easting";
            c1.Series["Collars"].Points.Clear();
            int i = 0;
            double minX = double.MaxValue;
            double maxX = double.MinValue;
            double minY = double.MaxValue;
            double maxY = double.MinValue;

            foreach (CollarInfo ci in existingHoles) {
                double east = 0;
                double north = 0;
                try
                {
                    east = ci.Easting;
                    north = ci.Northing;
                }
                catch (Exception ex) {
                    continue;
                }
                c1.Series["Collars"].Points.AddXY(east, north);
                c1.Series["Collars"].Points[i].ToolTip = ci.Name;
                collarNames.Add(ci.Name);
                minX = Math.Min(minX, east);
                minY = Math.Min(minY, north);
                maxX = Math.Max(maxX, east);
                maxY = Math.Max(maxY, north);

                i++;
            }

            double minorXInc = (maxX - minX) / 20.0;
            double minorYInc = (maxY - minY) / 20.0;

            c1.ChartAreas[0].AxisX.Minimum = minX - minorXInc;
            c1.ChartAreas[0].AxisY.Minimum = minY - minorYInc;

            c1.ChartAreas[0].AxisX.Maximum = maxX + minorXInc;
            c1.ChartAreas[0].AxisY.Maximum = maxY + minorYInc;
           
            listCollars.ItemsSource = collarNames;
            //DoScaling(c1, minX, minY, maxX, maxY);
        }

        private void DoScaling(Chart c1, double minX, double minY, double maxX, double maxY)
        {

            double ww = this.Width;
            double hh = this.Height;
          
            int chartHeight = c1.Height;
            int chartWidth = c1.Width;

            double axesWidthX = maxX = minX;
            double axesWidthY = maxY = minY;

            // which is larger in the screen?
            bool screenHeightIsLongAxis = false;
            if (chartHeight > chartWidth)
            {
                screenHeightIsLongAxis = true;
            }
            bool plotHeightIsLongAxis = false;
            if (axesWidthY > axesWidthX) {
                plotHeightIsLongAxis = true;
            }



            if (screenHeightIsLongAxis)
            {
                c1.Width = (int)ww-50;
                c1.Height = (int)ww-50;
            }
            else {
                c1.Width = (int)hh-50;
                c1.Height = (int)hh-50;            
            }


        }

        private void buttonCopyImage_Click(object sender, RoutedEventArgs e)
        {
            CopyChartToClipboard();
        }

        private void CopyChartToClipboard()
        {

            // create a memory stream to save the chart image    
            System.IO.MemoryStream stream = new System.IO.MemoryStream();
            // save the chart image to the stream    
            Chart c1 = this.FindName("MyWinformChart") as Chart;
            c1.SaveImage(stream, System.Drawing.Imaging.ImageFormat.Bmp);
            // create a bitmap using the stream    
            Bitmap bmp = new Bitmap(stream);

            // save the bitmap to the clipboard    

            // copying data into the clipboad has to work around a know issue with Windows/Terminal services 
            // where an exception is generally generated if somehting else is using the clipboard.  THe solution is to 
            // retry a number of times.
            //  http://blogs.microsoft.co.il/blogs/tamir/archive/2007/10/24/clipboard-setdata-getdata-troubles-with-vpc-and-ts.aspx

            for (int i = 0; i < 10; i++)
            {
                try
                {
                    Clipboard.SetDataObject(bmp, true);
                    return;
                }
                catch { }
                System.Threading.Thread.Sleep(100);
            }
        }

        private void buttonDismiss_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void listCollars_MouseDown(object sender, MouseButtonEventArgs e)
        {
            


            
            
        }

        private void FindMatchingHoleData(string s, out double east, out double north)
        {
            east = -1;
            north  = -1;
            foreach (CollarInfo ci in existingHoles) { 
                if(ci.Name.Trim().Equals(s.Trim())){
                    east = ci.Easting;
                    north = ci.Northing;
                    break;
                }
            
            }
        }

        public List<CollarInfo> existingHoles { get; set; }

        private void listCollars_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            List<string> selColls = new List<string>();
            Chart c1 = this.FindName("MyWinformChart") as Chart;
            c1.Series["Selected"].Points.Clear();
            int i = 0;
            foreach (var item in listCollars.SelectedItems)
            {
                string s = item.ToString();
                selColls.Add(s);
                double east = 0;
                double north = 0;
                FindMatchingHoleData(s, out east, out north);
                c1.Series["Selected"].Points.AddXY(east, north);
                c1.Series["Selected"].Points[i].ToolTip = s;
                i++;
            }
        }
    }
}
