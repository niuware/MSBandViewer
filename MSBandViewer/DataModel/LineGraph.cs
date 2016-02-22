using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Media;

namespace Niuware.MSBandViewer.DataModel
{
    public class LineGraph
    {
        VectorData2D<double> linePoint;

        double yOffset = 0.0;
        double yOrigin = 0.0;
        double xScale = 10.0;
        double yScale = 1.0;

        Canvas targetCanvas;
        SolidColorBrush lineBrush;

        /// <summary>
        /// Creates a new line graph value
        /// </summary>
        /// <param name="canvas">Canvas where the line graph is drawn</param>
        /// <param name="brush">Brush of the line</param>
        /// <param name="yoffset">The graph will start at the middle height of the canvas plus this offset</param>
        /// <param name="xscale">The graph X scale</param>
        /// <param name="yscale">The graph Y scale</param>
        public LineGraph(ref Canvas canvas, SolidColorBrush brush, double yoffset = 0.0, double xscale = 10.0, double yscale = 1.0)
        {
            targetCanvas = canvas;
            lineBrush = brush;
            yOffset = yoffset;
            xScale = xscale;
            yScale = yscale;
        }

        public double XScale
        {
            get { return xScale; } 
            set { xScale = value; }
        }

        public double YScale
        {
            get { return yScale; }
            set { yScale = value; }
        }

        public double YOffset
        {
            get
            {
                return yOffset;
            }
            set
            {
                yOffset = value;
            }
        }

        public double YOrigin
        {
            get
            {
                return yOrigin;
            }
            set
            {
                yOrigin = value;
            }
        }

        /// <summary>
        /// Adds a new line to the graph, well to the canvas
        /// </summary>
        /// <param name="value">The y value for the new line</param>
        public void UpdateGraph(double value)
        {
            Line li = new Line();
            li.Stroke = lineBrush;
            li.StrokeThickness = 2.0;

            // If the X axis has reached the limit, clear all lines and start from X = 0
            if (linePoint.X >= targetCanvas.ActualWidth)
            {
                linePoint.X = 0;
                targetCanvas.Children.Clear();
            }

            value *= yScale;

            li.X1 = linePoint.X;
            li.Y1 = linePoint.Y + yOffset + yOrigin;

            linePoint.X += xScale;

            li.X2 = linePoint.X;

            if(Math.Abs(value) > targetCanvas.ActualHeight / 2.0)
            {
                value = Math.Sign(value) * targetCanvas.ActualHeight / 2.0;
            }

            linePoint.Y = value;

            li.Y2 = linePoint.Y + yOffset + yOrigin;

            targetCanvas.Children.Add(li);
        }

        void ClearTargetCanvas()
        {
            targetCanvas.Children.Clear();
        }

        /// <summary>
        /// If window is resized, change the origin of the graph
        /// </summary>
        public void SizeChanged()
        {
            ClearTargetCanvas();
            yOrigin = targetCanvas.ActualHeight / 2.0;
        }


    }
}
