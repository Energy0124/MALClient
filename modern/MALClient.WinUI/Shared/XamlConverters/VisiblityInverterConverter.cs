using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace MALClient.UWP.Shared.XamlConverters
{
    public sealed class VisiblityInverterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var param = (Visibility) value;
            return param == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}