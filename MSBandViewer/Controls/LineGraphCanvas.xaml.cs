using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using System.ComponentModel;

namespace Niuware.MSBandViewer.Controls
{
    /// <summary>
    /// Custom control for drawing a Line Graph
    /// </summary>
    public sealed partial class LineGraphCanvas : Grid, INotifyPropertyChanged
    {
        /// <summary>
        /// A line that will be drawn in the LineGraph control
        /// </summary>
        class LineGraph
        {
            // Length of the line
            public double X { get; set; }

            // Value that the line will represent in the graph
            public double Y { get; set; }

            // Offset along the Y axis. By default the line will be drawn from the center (YOrigin) of the LineGraph canvas
            public double YOffset { get; set; }

            // Color of the line
            public SolidColorBrush Brush { get; set; }

            // Name of the value that the line represents
            public string Label { get; set; }
        }

        List<LineGraph> lineGraphList;

        double[] lineGraphValues;
        public double[] LineGraphValues
        {
            get { return lineGraphValues; }
            set { lineGraphValues = value; NotifyPropertyChanged("LineGraphValues"); }
        }

        // Counter for the total number of lines in the LineGraph control
        int LineCount { get; set; }

        public double YOrigin { get; set; }
        public double XScale { get; set; }
        public double YScale { get; set; }

        // Header of the LineGraph label stackpanel
        public string Label { get; set; }

        public double YScaleMax { get; set; }
        public double ScaleStep { get; set;}

        public LineGraphCanvas()
        {
            this.InitializeComponent();

            XScale = 10.0;
            YScale = 1.0;
            YScaleMax = 15.0;
            ScaleStep = 0.5;

            lineGraphList = new List<LineGraph>();
        }

        /// <summary>
        /// Adds a new line to the LineGraph control
        /// </summary>
        /// <param name="yoffset">Offset of the line along the Y axis.</param>
        /// <param name="label">Name of the value that the line represents</param>
        /// <param name="brush">Color of the line</param>
        public void AddLineGraph(double yoffset = 0.0, string label = "", SolidColorBrush brush = null)
        {
            lineGraphList.Add(new LineGraph()
            {
                Label = label,
                Brush = brush,
                YOffset = yoffset - (this.Padding.Top)
            });

            // If the line has label, we add the required controls and value binding
            if (label != "")
            {
                TextBlock tb = new TextBlock()
                {
                    Foreground = brush,
                    Text = label
                };

                Binding binding = new Binding()
                {
                    Path = new PropertyPath("LineGraphValues[" + LineCount + "]"),
                    Converter = new Converters.LineGraphLabelConverter(),   // Format the value as 'Label: Value'
                    ConverterParameter = label,
                    Mode = BindingMode.OneWay
                };

                BindingOperations.SetBinding(tb, TextBlock.TextProperty, binding);

                labelStackPanel.Children.Add(tb);
            }

            LineCount++;
        }

        /// <summary>
        /// Update all lines values
        /// </summary>
        /// <param name="values">Array of line ordered values</param>
        public void UpdateValues(params double[] values)
        {
            if (values.Length != LineCount)
            {
                return;
            }

            LineGraphValues = values;

            for (int i = 0; i < LineCount; i++)
            {
                UpdateLineGraphValue(i, values[i]);
            }
        }

        /// <summary>
        /// Clears all lines currently drawn in the canvas
        /// </summary>
        public void Reset()
        {
            canvas.Children.Clear();
        }

        /// <summary>
        /// Draws a new line into the canvas
        /// </summary>
        /// <param name="index">Index of the line to draw</param>
        /// <param name="value">Value that the line will represent</param>
        private void UpdateLineGraphValue(int index, double value)
        {
            Line li = new Line();
            li.Stroke = lineGraphList[index].Brush;
            li.StrokeThickness = 2.0;

            // If the X axis has reached the right limit, clear all lines and start from X = 0
            if (lineGraphList[index].X >= canvas.ActualWidth)
            {
                lineGraphList[index].X = 0.0;
                canvas.Children.Clear();
            }

            // Apply the scale for the value
            value *= YScale;

            // Set the starting X,Y for the line. Both X and Y are the previous line's values
            li.X1 = lineGraphList[index].X;
            li.Y1 = lineGraphList[index].Y + lineGraphList[index].YOffset + YOrigin;

            // Set the X value for the line
            lineGraphList[index].X += XScale;

            // If the value is greater than the canvas height, clip the value
            if (Math.Abs(value) > canvas.ActualHeight / 2.0)
            {
                value = Math.Sign(value) * canvas.ActualHeight / 2.0;
            }

            // Set the Y value for the line
            lineGraphList[index].Y = value;

            // Set the ending X,Y for the line. New calculated values for X and Y
            li.X2 = lineGraphList[index].X;
            li.Y2 = lineGraphList[index].Y + lineGraphList[index].YOffset + YOrigin;

            // Add the line to the canvas
            canvas.Children.Add(li);
        }

        /// <summary>
        /// Calculate the Y origin if the LineGraph control has been resized
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            canvas.Children.Clear();

            YOrigin = ActualHeight / 2.0;
        }

        /// <summary>
        /// Zoom in the LineGraph control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void zoomInButton_Click(object sender, RoutedEventArgs e)
        {
            if ((YScale + ScaleStep) <= YScaleMax)
            {
                YScale += ScaleStep;
                XScale += ScaleStep;
            }
        }

        /// <summary>
        /// Zoom out the LineGraph control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void zoomOutButton_Click(object sender, RoutedEventArgs e)
        {
            if ((YScale - ScaleStep) > 0)
            {
                YScale -= ScaleStep;
                XScale -= ScaleStep;
            }
        }

        #region INotifyPropertyChanged 

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
