using Microsoft.UI;
using System;
using Windows.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using MALClient.XShared.Utils;

namespace MALClient.UWP.Shared.XamlConverters
{
    public class MailIconToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if ((Symbol) value == Symbol.Mail)
                return Application.Current.Resources["SystemControlBackgroundAccentBrush"];
            return Settings.SelectedTheme == (int)ApplicationTheme.Dark
                ? new SolidColorBrush(Colors.White)
                : new SolidColorBrush(Colors.Black);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}