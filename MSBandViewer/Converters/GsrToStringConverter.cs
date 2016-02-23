using System;
using Windows.UI.Xaml.Data;

namespace Niuware.MSBandViewer.Converters
{
    class GsrToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                object parameter, string language)
        {
            return String.Format("{0,-10:0.##}", System.Convert.ToDouble(value) / 1000.0);
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
