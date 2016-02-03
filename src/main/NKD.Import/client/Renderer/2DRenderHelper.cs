using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace NKD.Import.Client.Renderer
{
    public class _2DRenderHelper
    {


    }

    public class LargeText : FrameworkElement
    {
        // Create a collection of child visual objects.
        private VisualCollection _children;

        public LargeText(String theText, double x, double y, bool isBold, bool doRotate, TextAlignment align, int fontSize)
        {
            _children = new VisualCollection(this);
            _children.Add(CreateDrawingVisualText(x, y, theText, isBold, doRotate, align, fontSize));
        }



        // Create a DrawingVisual that contains text.
        private DrawingVisual CreateDrawingVisualText(double x, double y, string vals, bool isBold, bool rotate90, TextAlignment align, int fontSize)
        {
            // Create an instance of a DrawingVisual.
            DrawingVisual drawingVisual = new DrawingVisual();
            // Retrieve the DrawingContext from the DrawingVisual.
            DrawingContext drawingContext = drawingVisual.RenderOpen();

            FormattedText ft = new FormattedText(vals,
                  CultureInfo.GetCultureInfo("en-us"),
                  FlowDirection.LeftToRight,
                  new Typeface("Verdana"),
                  fontSize, Brushes.LightGray);
            ft.SetFontWeight(FontWeights.Bold);
            ft.TextAlignment = align;
            // Draw a formatted text string into the DrawingContext.
            drawingContext.DrawText(ft, new Point(x, y));

            // Close the DrawingContext to persist changes to the DrawingVisual.
            drawingContext.Close();

            return drawingVisual;
        }




        // Provide a required override for the VisualChildrenCount property.
        protected override int VisualChildrenCount
        {
            get { return _children.Count; }
        }

        // Provide a required override for the GetVisualChild method.
        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= _children.Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            return _children[index];
        }
    }

    public class GeneralText : FrameworkElement
    {
        // Create a collection of child visual objects.
        private VisualCollection _children;

        public GeneralText(String theText, double x, double y, bool isBold, bool doRotate, TextAlignment align)
        {
            _children = new VisualCollection(this);
            _children.Add(CreateDrawingVisualText(x, y, theText, isBold, doRotate, align));
        }



        // Create a DrawingVisual that contains text.
        private DrawingVisual CreateDrawingVisualText(double x, double y, string vals, bool isBold, bool rotate90, TextAlignment align)
        {
            // Create an instance of a DrawingVisual.
            DrawingVisual drawingVisual = new DrawingVisual();
            // Retrieve the DrawingContext from the DrawingVisual.
            DrawingContext drawingContext = drawingVisual.RenderOpen();

            FormattedText ft = new FormattedText(vals,
                  CultureInfo.GetCultureInfo("en-us"),
                  FlowDirection.LeftToRight,
                  new Typeface("Verdana"),
                  10, Brushes.DarkGray);

            ft.SetFontWeight(FontWeights.Light);
            ft.TextAlignment = align;
            // Draw a formatted text string into the DrawingContext.
            drawingContext.DrawText(ft, new Point(x, y));
            // Close the DrawingContext to persist changes to the DrawingVisual.
            drawingContext.Close();
            return drawingVisual;
        }


        // Provide a required override for the VisualChildrenCount property.
        protected override int VisualChildrenCount
        {
            get { return _children.Count; }
        }

        // Provide a required override for the GetVisualChild method.
        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= _children.Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            return _children[index];
        }
    }

}
