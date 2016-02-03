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

namespace NKD.Import.Client.Renderer
{
    /// <summary>
    /// Interaction logic for CollarRenderPanel.xaml
    /// </summary>
    public partial class CollarRenderPanel : UserControl
    {
        public CollarRenderPanel()
        {
            InitializeComponent();
        }


        private void DrawStartupMessage()
        {
            string stringToDraw = "Welcome to NKD.";
            LargeText gt = new LargeText(stringToDraw, 50, 10, false, false, TextAlignment.Left, 20);
            drawingCanvas.Children.Add(gt);
           
        }


        /// <summary>
        /// Draw the gridlines for a particualr trace.  At this stage the gridlines and scales are based on min/masx valeus.
        /// In the future, we could allow the user to specifiy number of ticks, min/max etc.
        /// </summary>
        /// <param name="seriesName"></param>
        /// <param name="minY"></param>
        /// <param name="maxY"></param>
        /// <param name="yScale"></param>
        /// <param name="yIncrements"></param>
        /// <param name="yDistance"></param>
        /// <param name="xIncrements"></param>
        /// <param name="xOffset"></param>
        /// <param name="xMargin"></param>
        /// <param name="yMargin"></param>
        /// <param name="minX"></param>
        /// <param name="maxX"></param>
        /// <param name="holeID"></param>
        private void DrawGridlines(string seriesName, double minY, double maxY, double yScale, int yIncrements, 
            double yDistance, double xIncrements, int xOffset, int xMargin, int yMargin, double minX, double maxX, string title)
        {
            double incY = yDistance / (double)yIncrements;
            double currentY = minY;

            GeneralText gtName = new GeneralText(title, xMargin + xOffset, yMargin - 45, true, false, TextAlignment.Left);
            drawingCanvas.Children.Add(gtName);

            GeneralText gt = new GeneralText(seriesName, xMargin + xOffset, yMargin - 30, true, false, TextAlignment.Left);
            drawingCanvas.Children.Add(gt);

            for (int i = 0; i < yIncrements + 1; i++)
            {

                double cv = currentY + (incY * i);
                double screenPos = (cv - minY) / yScale;

                Line ly = new Line();
                ly.X1 = xMargin + xOffset + 0;
                ly.X2 = xMargin + xOffset + 100;
                ly.Y1 = yMargin + screenPos;
                ly.Y2 = yMargin + screenPos;
                ly.Stroke = Brushes.LightGray;
                ly.StrokeThickness = 1;
                ly.SnapsToDevicePixels = true;
                ly.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
                drawingCanvas.Children.Add(ly);


            }

            string minXText = string.Format("{0:0.###}", minX);
            string maxXText = string.Format("{0:0.###}", maxX);
            if (minX == double.MaxValue)
            {
                minXText = "0";
            }
            if (maxX == double.MinValue)
            {
                maxXText = "0";
            }
            GeneralText gt1 = new GeneralText(minXText, xMargin + xOffset, yMargin - 15, false, true, TextAlignment.Left);
            drawingCanvas.Children.Add(gt1);

            GeneralText gt2 = new GeneralText(maxXText, xMargin + xOffset + 100, yMargin - 15, false, true, TextAlignment.Right);
            drawingCanvas.Children.Add(gt2);


            for (int i = 0; i < xIncrements + 1; i++)
            {
                Line lx1 = new Line();
                lx1.X1 = xMargin + xOffset + (i * 10);
                lx1.X2 = xMargin + xOffset + (i * 10);
                lx1.Y1 = yMargin + 0;
                lx1.Y2 = yMargin + ((maxY - minY) / yScale);

                lx1.Stroke = Brushes.LightGray;
                lx1.StrokeThickness = 1;
                lx1.SnapsToDevicePixels = true;
                lx1.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
                drawingCanvas.Children.Add(lx1);
            }

            Rectangle outline = new Rectangle();
            outline.Width = 100;
            outline.Height = ((maxY - minY) / yScale);
            outline.Stroke = Brushes.Black;
            Canvas.SetLeft(outline, xMargin + xOffset);
            Canvas.SetTop(outline, yMargin);

            drawingCanvas.Children.Add(outline);

        }


    }
}
