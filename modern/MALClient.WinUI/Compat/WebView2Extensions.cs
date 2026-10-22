using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;

namespace MALClient.WinUI.Compat
{
    public static class WebView2Extensions
    {
        /// <summary>
        ///     The link a user navigated to, or null for the app's own content. UWP reported NavigateToString content
        ///     with a null uri, WebView2 reports it as a data: (or about:blank) uri, which the old handlers would cancel.
        ///     Relative links inside that content resolve against about:, map them back onto MAL.
        /// </summary>
        public static Uri GetExternalUri(this CoreWebView2NavigationStartingEventArgs args)
        {
            if (string.IsNullOrEmpty(args.Uri) || !Uri.TryCreate(args.Uri, UriKind.Absolute, out var uri))
                return null;

            if (uri.Scheme == "data")
                return null;

            if (uri.Scheme == "about")
            {
                var path = uri.AbsolutePath;
                if (path == "blank" || string.IsNullOrEmpty(path))
                    return null;
                return new Uri("https://myanimelist.net" + (path.StartsWith("/") ? path : "/" + path));
            }

            return uri;
        }

        /// <summary>
        ///     UWP's WebView could navigate right away, WebView2 needs its browser core to be created first.
        /// </summary>
        public static async Task NavigateToStringAsync(this WebView2 view, string html)
        {
            await view.EnsureCoreWebView2Async();
            view.NavigateToString(html ?? string.Empty);
        }
    }
}
