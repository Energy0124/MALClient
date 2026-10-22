using System;
using Microsoft.UI.Xaml.Data;

namespace MALClient.UWP.Shared.XamlConverters
{
    public class DummyFontAwesomeToRealFontAwesomeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            //the compat FontAwesome control takes the shared enum directly
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
