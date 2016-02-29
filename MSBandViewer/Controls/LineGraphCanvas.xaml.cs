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

    public sealed partial class LineGraphCanvas : Grid, INotifyPropertyChanged
    {
        class LineGraph
        {
            public double X { get; set; }
            public double Y { get; set; } 
            public double YOffset { get; set; }
            public SolidColorBrush Brush { get; set; }
            public string Label { get; set; }
        }

        List<LineGraph> lineGraphList;

        double[] lineGraphValues;
        public double[] LineGraphValues { get { return lineGraphValues; } set { lineGraphValues = value; NotifyPropertyChanged("LineGraphValues"); } }

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
            YScale = 1.0;

            lineGraphList = new List<LineGraph>();
        }

        public void AddLineGraph(double yoffset = 0.0, string label = "", SolidColorBrush brush = null)
        {
            lineGraphList.Add(new LineGraph()
            {
                Label = label,
                Brush = brush,
                YOffset = yoffset - (this.Padding.Top)
            });

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

        public void Reset()
        {
            canvas.Children.Clear();
        }

        private void UpdateLineGraphValue(int index, double value)
        {
            Line li = new Line();
            li.Stroke = lineGraphList[index].Brush;
            li.StrokeThickness = 2.0;

            // If the X axis has reached the limit, clear all lines and start from X = 0
            if (lineGraphList[index].X >= canvas.ActualWidth)
            {
                lineGraphList[index].X = 0.0;
                canvas.Children.Clear();
            }

            value *= YScale;

            li.X1 = lineGraphList[index].X;
            li.Y1 = lineGraphList[index].Y + lineGraphList[index].YOffset + YOrigin;

            lineGraphList[index].X += XScale;

            li.X2 = lineGraphList[index].X;

            if (Math.Abs(value) > canvas.ActualHeight / 2.0)
            {
                value = Math.Sign(value) * canvas.ActualHeight / 2.0;
            }

            lineGraphList[index].Y = value;

            li.Y2 = lineGraphList[index].Y + lineGraphList[index].YOffset + YOrigin;

            canvas.Children.Add(li);
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            canvas.Children.Clear();

            YOrigin = ActualHeight / 2.0;
        }
    }
}
