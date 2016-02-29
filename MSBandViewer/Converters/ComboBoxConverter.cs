using System;
using Windows.UI.Xaml.Data;

namespace Niuware.MSBandViewer.Converters
{
    class ComboBoxConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                object parameter, string language)
        {
            if (targetType == typeof(int))
            {
                return (int)value;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, string language)
        {
            if (targetType == typeof(int))
            {
                return (int)value;
            }

            return value;
        }
    }
}
