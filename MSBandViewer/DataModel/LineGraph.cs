using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Media;
using Windows.UI;

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

        double xOffset = 10.0;
        double yOffset = 0.0;

        Canvas targetCanvas;
        Color lineColor;

        public LineGraph(ref Canvas canvas, Color color)
        {
            targetCanvas = canvas;
            lineColor = color;
        }

        public void SetXOffset(double value)
        {
            xOffset = value;
        }

        public void SetYOffset(double value)
        {
            yOffset = value;
        }

        public void UpdateGraph(double value)
        {
            Line li = new Line();
            li.Stroke = new SolidColorBrush(lineColor);
            li.StrokeThickness = 2.0;

            if (linePoint.X >= targetCanvas.ActualWidth)
            {
                linePoint.X = 0;
                targetCanvas.Children.Clear();
            }

            li.X1 = linePoint.X;
            li.Y1 = linePoint.Y;

            linePoint.X += xOffset;

            li.X2 = linePoint.X;

            linePoint.Y = value;

            li.Y2 = linePoint.Y;

            targetCanvas.Children.Add(li);
        }
    }
}
