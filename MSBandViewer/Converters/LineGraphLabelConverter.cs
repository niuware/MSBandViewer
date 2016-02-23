using System;
using Windows.UI.Xaml.Data;

namespace Niuware.MSBandViewer.Converters
{
    class LineGraphLabelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                object parameter, string language)
        {
            return String.Format("{0}: {1:0.###}", parameter, value);
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}