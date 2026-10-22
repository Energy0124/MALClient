using System;
using System.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Windows.Media.Core;

namespace MALClient.WinUI.Compat
{
    //replacements for the WinRTXamlToolkit / UWP Community Toolkit converters used by the ported pages

    public class BooleanToVisibilityConverter : IValueConverter
    {
        public bool IsInversed { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var visible = value is bool b && b;
            return visible ^ IsInversed ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value is Visibility visibility && (visibility == Visibility.Visible) ^ IsInversed;
        }
    }

    /// <summary>
    ///     MediaPlayerElement (WinUI's replacement for MediaElement) takes a media source instead of a url.
    /// </summary>
    public class UriToMediaSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value is string url && Uri.TryCreate(url, UriKind.Absolute, out var uri)
                ? MediaSource.CreateFromUri(uri)
                : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    ///     Visible when the bound collection has any items.
    /// </summary>
    public class CollectionVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var hasItems = value is IEnumerable enumerable && enumerable.GetEnumerator().MoveNext();
            return hasItems ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }
}
