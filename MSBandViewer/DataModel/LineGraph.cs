using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Media;
using Windows.UI;
using System;

namespace Niuware.MSBandViewer.DataModel
{
    class LineGraphPoint
    {
        public double X { get; set; }
        public double Y { get; set; }

        public LineGraphPoint()
        {
            X = Y = 0.0;
        }

        public void ResetX()
        {
            X = 0.0;
        }

        public void ResetY()
        {
            Y = 0.0;
        }
    }

    public class LineGraph
    {
        LineGraphPoint linePoint = new LineGraphPoint();

        double xOffset = 0.0;
        double yOffset = 0.0;
        double yOrigin = 0.0;

        Canvas targetCanvas;
        SolidColorBrush lineBrush;

        public LineGraph(ref Canvas canvas, SolidColorBrush brush, double xoffset = 0.0, double yoffset = 0.0)
        {
            targetCanvas = canvas;
            lineBrush = brush;
            xOffset = xoffset;
            yOffset = yoffset;
        }

        public void SetXOffset(double value)
        {
            xOffset = value;
        }

        public void SetYOffset(double value)
        {
            yOffset+= value;
        }

        public void UpdateGraph(double value)
        {
            Line li = new Line();
            li.Stroke = lineBrush;
            li.StrokeThickness = 2.0;

            if (linePoint.X >= targetCanvas.ActualWidth)
            {
                linePoint.X = 0;
                targetCanvas.Children.Clear();
            }

            li.X1 = linePoint.X;
            li.Y1 = linePoint.Y + yOffset + yOrigin;

            linePoint.X += xOffset;

            li.X2 = linePoint.X;

            if(Math.Abs(value) > targetCanvas.ActualHeight / 2.0)
            {
                value = Math.Sign(value) * targetCanvas.ActualHeight / 2.0;
            }

            linePoint.Y = value;

            li.Y2 = linePoint.Y + yOffset + yOrigin;

            targetCanvas.Children.Add(li);
        }

        public void SizeChanged()
        {
            yOrigin = targetCanvas.ActualHeight / 2.0;
        }
    }
}
