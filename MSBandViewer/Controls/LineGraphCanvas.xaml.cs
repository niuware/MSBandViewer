using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Navigation;
using System.ComponentModel;
using Niuware.MSBandViewer.DataModels;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Niuware.MSBandViewer.Controls
{

    public sealed partial class LineGraphCanvas : Grid, INotifyPropertyChanged
    {
        private class LineGraph
        {
            VectorData2D<double> linePoint;

            double yOffset = 0.0;
            double yOrigin = 0.0;
            double xScale = 10.0;
            double yScale = 1.0;

            public Canvas canvas;
            SolidColorBrush lineBrush;

            public string Label { get; set; }

            public LineGraph(ref Canvas targetCanvas, string label, SolidColorBrush brush, double yoffset = 0.0, double xscale = 10.0, double yscale = 1.0)
            {
                Label = label;
                canvas = targetCanvas;
                lineBrush = brush;
                yOffset = yoffset;
                xScale = xscale;
                yScale = yscale;
            }

            //public double XScale
            //{
            //    get { return xScale; }
            //    set { xScale = value; }
            //}

            //public double YScale
            //{
            //    get { return yScale; }
            //    set { yScale = value; }
            //}

            //public double YOffset
            //{
            //    get
            //    {
            //        return yOffset;
            //    }
            //    set
            //    {
            //        yOffset = value;
            //    }
            //}

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
                if (linePoint.X >= canvas.ActualWidth)
                {
                    linePoint.X = 0;
                    canvas.Children.Clear();
                }

                value *= yScale;

                li.X1 = linePoint.X;
                li.Y1 = linePoint.Y + yOffset + yOrigin;

                linePoint.X += xScale;

                li.X2 = linePoint.X;

                if (Math.Abs(value) > canvas.ActualHeight / 2.0)
                {
                    value = Math.Sign(value) * canvas.ActualHeight / 2.0;
                }

                linePoint.Y = value;

                li.Y2 = linePoint.Y + yOffset + yOrigin;

                canvas.Children.Add(li);
            }
        }

        List<LineGraph> lineGraphList;
        //public List<LineGraph> LineGraphList { get { return lineGraphList; } }
        double[] lineGraphValues;
        public double[] LineGraphValues { get { return lineGraphValues; } set { lineGraphValues = value; NotifyPropertyChanged("LineGraphValues"); } }

        Timer drawTimer;
        public int DrawTimerUpdate { get; set; }

        public int LineCount { get; set; }
        public double YOffset { get; set; }
        public double YOrigin { get; set; }
        public double XScale { get; set; }
        public double YScale { get; set; }

        public string Label { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public LineGraphCanvas()
        {
            this.InitializeComponent();

            XScale = 10.0;
            DrawTimerUpdate = 10;
            YScale = 1.0;

            lineGraphList = new List<LineGraph>();

            //drawTimer = new Timer(drawTimer_Callback, null, 0, DrawTimerUpdate);
        }

        public void AddLineGraph(SolidColorBrush brush, double yoffset = 0.0, double xscale = 10.0, double yscale = 1.0, string label = "")
        {
            lineGraphList.Add(new LineGraph(ref canvas, label, brush, yoffset - (this.Padding.Top), xscale, yscale));

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
                    Converter = new Converters.LineGraphLabelConverter(),
                    ConverterParameter = label,
                    Mode = BindingMode.OneWay
                };

                BindingOperations.SetBinding(tb, TextBlock.TextProperty, binding);

                labelStackPanel.Children.Add(tb);
            }

            LineCount++;
        }

        public void CreateLineGraphList(int count)
        {
            LineCount = count;

            lineGraphList = new List<LineGraph>();

            for (int i = 0; i < LineCount; i++)
            {
                lineGraphList.Add(new LineGraph(ref canvas, "", new SolidColorBrush(Windows.UI.Colors.White), YOffset, XScale, YScale));
                lineGraphList[i].YOrigin = ActualHeight / 2.0;
            }
        }

        public void UpdateValues(params double[] values)
        {
            if (values.Length != LineCount)
            {
                return;
            }

            LineGraphValues = values;

            for (int i = 0; i < LineCount; i++)
            {
                lineGraphList[i].YOrigin = YOrigin;
                lineGraphList[i].UpdateGraph(values[i]);
            }
        }

        public void Reset()
        {
            canvas.Children.Clear();
        }

        private void drawTimer_Callback(object state)
        {
            //System.Diagnostics.Debug.WriteLine(Hola);
            //foreach (LineGraph lg in lineGraphList)
            //{
            //    lg.UpdateGraph();
            //}
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            canvas.Children.Clear();

            YOrigin = ActualHeight / 2.0;
        }
    }
}
