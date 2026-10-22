using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using MALClient.UWP.Shared.Managers;
using MALClient.WinUI.Compat;
using MALClient.XShared.ViewModels;

namespace MALClient.UWP.Shared.UserControls.AttachedProperties
{
    public class WebViewExtensions : DependencyObject
    {
        public static readonly DependencyProperty StyleImagesProperty = DependencyProperty.RegisterAttached(
            "StyleImages", typeof(bool), typeof(WebViewExtensions), new PropertyMetadata(default(bool)));

        public static void SetStyleImages(DependencyObject element, bool value)
        {
            element.SetValue(StyleImagesProperty, value);
        }

        public static bool GetStyleImages(DependencyObject element)
        {
            return (bool)element.GetValue(StyleImagesProperty);
        }

        public static readonly DependencyProperty ContentProperty = DependencyProperty.RegisterAttached(
            "Content", typeof(string), typeof(WebViewExtensions), new PropertyMetadata(default(string), PropertyChangedCallback));

        private static async void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var view = dependencyObject as WebView2;
            if (GetResizeToFit(view))
            {
                if (view.Tag == null)
                {
                    view.WebMessageReceived += ViewOnWebMessageReceived;
                    view.Tag = true;
                }
            }
            SetComputedHeight(view, 0);
            var minWidth = GetMinimumWidth(dependencyObject);
            await view.NavigateToStringAsync(ResourceLocator.CssManager.WrapWithCss(dependencyPropertyChangedEventArgs.NewValue as string, GetStyleImages(dependencyObject), minWidth > 0 ? (int?)minWidth : null));
        }

        //the wrapping css reports document height through chrome.webview.postMessage
        private static async void ViewOnWebMessageReceived(WebView2 view, CoreWebView2WebMessageReceivedEventArgs e)
        {
            double val;
            if (!double.TryParse(e.TryGetWebMessageAsString(), NumberStyles.Any, CultureInfo.InvariantCulture, out val))
                return;

            // val = val / 72 * _displayInformation.LogicalDpi;
            val *= 1.1;
            // val *= _scaleCoefficient;
            if (view.ActualHeight == 0) // let's recheck
            {
                await Task.Delay(1000);
                try
                {
                    await view.ExecuteScriptAsync(@"notifyDocumentHeightChanged('content');");
                }
                catch (Exception)
                {
                    //null content
                }
                return;
            }
            if (view.DataContext == null || Math.Abs(val - GetComputedHeight(view)) < .1)
                return;

            if (val > view.ActualHeight)
            {
                SetComputedHeight(view, val);
                //Debug.WriteLine($"{(view.DataContext as ForumTopicMessageEntryViewModel)?.Data.MessageNumber} - {val}");
            }
            else
            {
                SetComputedHeight(view, view.ActualHeight);
            }

        }

        public static void SetContent(DependencyObject element, string value)
        {
            element.SetValue(ContentProperty, value);
        }

        public static string GetContent(DependencyObject element)
        {
            return (string)element.GetValue(ContentProperty);
        }

        public static readonly DependencyProperty MinimumWidthProperty = DependencyProperty.RegisterAttached(
            "MinimumWidth", typeof(int), typeof(WebViewExtensions), new PropertyMetadata(-1));

        public static void SetMinimumWidth(DependencyObject element, int value)
        {
            element.SetValue(MinimumWidthProperty, value);
        }

        public static int GetMinimumWidth(DependencyObject element)
        {
            return (int)element.GetValue(MinimumWidthProperty);
        }

        public static readonly DependencyProperty ResizeToFitProperty = DependencyProperty.RegisterAttached(
            "ResizeToFit", typeof(bool), typeof(WebViewExtensions), new PropertyMetadata(default(bool)));

        public static void SetResizeToFit(DependencyObject element, bool value)
        {
            element.SetValue(ResizeToFitProperty, value);
        }

        public static bool GetResizeToFit(DependencyObject element)
        {
            return (bool)element.GetValue(ResizeToFitProperty);
        }

        public static readonly DependencyProperty ComputedHeightProperty = DependencyProperty.RegisterAttached(
            "ComputedHeight", typeof(double), typeof(WebViewExtensions), new PropertyMetadata(default(double)));


        public static void SetComputedHeight(DependencyObject element, double value)
        {
            element.SetValue(ComputedHeightProperty, value);
        }

        public static double GetComputedHeight(DependencyObject element)
        {
            return (double)element.GetValue(ComputedHeightProperty);
        }
    }
}