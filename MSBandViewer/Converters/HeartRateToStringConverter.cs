using System;
using Windows.UI.Xaml.Data;

namespace Niuware.MSBandViewer.Converters
{
    class HeartRateToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                object parameter, string language)
        {
            if (parameter.ToString() == "min")
            {
                return ((int)value >= 250) ? "--" : value.ToString();
            }
            else
            {
                return ((int)value <= 0) ? "--" : value.ToString();
            }
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
