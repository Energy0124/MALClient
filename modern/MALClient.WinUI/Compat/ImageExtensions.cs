using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

namespace MALClient.WinUI.Compat
{
    /// <summary>
    ///     Replacement for WinRTXamlToolkit's ImageExtensions.FadeInOnLoaded: fades images in once their source opens.
    /// </summary>
    public static class ImageExtensions
    {
        public static readonly DependencyProperty FadeInOnLoadedProperty =
            DependencyProperty.RegisterAttached("FadeInOnLoaded", typeof(bool), typeof(ImageExtensions),
                new PropertyMetadata(false, OnFadeInOnLoadedChanged));

        public static bool GetFadeInOnLoaded(DependencyObject obj)
        {
            return (bool) obj.GetValue(FadeInOnLoadedProperty);
        }

        public static void SetFadeInOnLoaded(DependencyObject obj, bool value)
        {
            obj.SetValue(FadeInOnLoadedProperty, value);
        }

        private static void OnFadeInOnLoadedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not Image image)
                return;

            image.ImageOpened -= ImageOnImageOpened;
            if ((bool) e.NewValue)
                image.ImageOpened += ImageOnImageOpened;
        }

        private static void ImageOnImageOpened(object sender, RoutedEventArgs e)
        {
            var image = (Image) sender;
            var animation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(300)
            };
            Storyboard.SetTarget(animation, image);
            Storyboard.SetTargetProperty(animation, nameof(UIElement.Opacity));
            var storyboard = new Storyboard();
            storyboard.Children.Add(animation);
            storyboard.Begin();
        }
    }
}
