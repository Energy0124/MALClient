using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using MALClient.Models.Models.Favourites;

namespace MALClient.UWP.Shared.XamlConverters
{
    public class FavouriteBaseToStaffPersonToIsUnknownVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (value as AnimeStaffPerson).IsUnknown ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
