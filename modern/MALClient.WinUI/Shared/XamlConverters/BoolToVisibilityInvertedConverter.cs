using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace MALClient.UWP.Shared.XamlConverters
{
    public class BoolToVisibilityInvertedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (bool) value ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
