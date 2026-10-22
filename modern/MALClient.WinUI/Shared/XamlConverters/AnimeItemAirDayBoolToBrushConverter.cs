using Microsoft.UI;
using System;
using Windows.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using MALClient.XShared.Utils;
using MALClient.XShared.ViewModels;

namespace MALClient.UWP.Shared.XamlConverters
{
    public class AnimeItemAirDayBoolToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if(value != null)
            {
                var model = (AnimeItemViewModel)value;
                if (model.AirDayBrush == true && model.AnimeItemDisplayContext != AnimeItemDisplayContext.Index)
                    return new SolidColorBrush(Settings.SelectedTheme == (int)ApplicationTheme.Dark ? Colors.Gray : Colors.LightGray);
            }

			return new SolidColorBrush(Settings.SelectedTheme == (int)ApplicationTheme.Dark ? Colors.White : Colors.Black);
		}

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
