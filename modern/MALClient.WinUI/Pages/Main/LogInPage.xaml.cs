using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Web.WebView2.Core;
using MALClient.XShared.ViewModels;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace MALClient.UWP.Pages.Main
{
    public sealed partial class LogInPage : Page
    {
        private const string MobileUserAgent =
            @"Mozilla/5.0 (Linux; Android 7.0; SM-G930V Build/NRD90M) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.125 Mobile Safari/537.36";

        private bool _headerSent;
        private string _cookies;

        public LogInPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModelLocator.LogIn.Init();
            base.OnNavigatedTo(e);
        }

        private async void ButtonProblems_OnClick(object sender, RoutedEventArgs e)
        {
            var msg = new ContentDialog
            {
                Content =
                    "If you are experiencing constant error messages while trying to log in , resetting your password on MAL may solve this issue. Why you may ask... MAL api is just very very bad and it tends to do such things which are beyond my control.",
                CloseButtonText = "Close",
                XamlRoot = XamlRoot
            };
            await msg.ShowAsync();
        }

        private void Password_OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                e.Handled = true;
                var txt = sender as PasswordBox;
                if (txt.Password.Length == 0)
                    return;
                txt.IsEnabled = false; //reset input
                txt.IsEnabled = true;
                ViewModelLocator.LogIn.LogInCommand.Execute(null);
            }
        }

        private async void SignInButtonClick(object sender, RoutedEventArgs e)
        {
            SignInWebView.Visibility = Visibility.Visible;
            await SignInWebView.EnsureCoreWebView2Async();
            SignInWebView.CoreWebView2.Settings.IsScriptEnabled = true;
            //UWP sent this per request, WebView2 has it as a setting
            SignInWebView.CoreWebView2.Settings.UserAgent = MobileUserAgent;
            SignInWebView.NavigationStarting -= SignInWebViewOnNavigationStarting;
            SignInWebView.NavigationStarting += SignInWebViewOnNavigationStarting;
            Navigate(new Uri("https://myanimelist.net/login.php"));
        }

        private async void SignInWebViewOnNavigationStarting(WebView2 sender, CoreWebView2NavigationStartingEventArgs args)
        {
            var targeturl = args.Uri;


            if (targeturl.Contains("maloauth"))
            {
                try
                {
                    var regex = new Regex(".*maloauth\\?code=(.*)\\&.*");
                    var match = regex.Matches(targeturl);
                    ViewModelLocator.LogIn.SignIn(_cookies, match[0].Groups[1].Value);
                    SignInWebView.Visibility = Visibility.Collapsed;
                    return;
                }
                catch
                {
                    //error, display the error page?
                    args.Cancel = false;
                    return;
                }
            }


            if (targeturl == "https://myanimelist.net/" ||
                targeturl == "https://myanimelist.net/#" || //from google signi
                targeturl.StartsWith("https://myanimelist.net/#"))  //from fb signin
            {
                args.Cancel = true;
                _cookies = await GetMalCookies();
                var url = "https://myanimelist.net/v1/oauth2/authorize?response_type=code&" +
                          "client_id=183063f74126e7551b00c3b4de66986c&" +
                          "state=signin&" +
                          $"code_challenge={ViewModelLocator.LogIn.PkceChallenge}&" +
                          "code_challenge_method=plain";
                Navigate(new Uri(url));


                return;
            }

            if (_headerSent)
            {
                _headerSent = false;
                return;
            }

            if (targeturl == "https://myanimelist.net/register.php")
            {
                ViewModelLocator.LogIn.NavigateRegister.Execute(null);
            }

            if (targeturl.StartsWith("https://api.twitter") ||
                targeturl.StartsWith("https://myanimelist.net/sns/login") ||
                targeturl.StartsWith("https://myanimelist.net/sns/callback") ||
                targeturl.Contains("apple") ||
                targeturl.StartsWith("https://www.facebook") ||
                targeturl.StartsWith("https://m.facebook") ||
                targeturl.StartsWith("https://accounts.google") ||
                targeturl.StartsWith("https://accounts.youtube") ||
                targeturl.StartsWith("https://myanimelist.net/login.php") ||
                targeturl.StartsWith("https://myanimelist.net/submission/authorization") ||
                targeturl.StartsWith("https://myanimelist.net/dialog/authorization"))
            {
                args.Cancel = false;
                return;
            }

            args.Cancel = true;
            Navigate(new Uri("https://myanimelist.net/login.php"));
        }

        private async Task<string> GetMalCookies()
        {
            var cookies = await SignInWebView.CoreWebView2.CookieManager.GetCookiesAsync("https://myanimelist.net");
            return cookies.Aggregate("", (s, cookie) => s + $"{cookie.Name}={cookie.Value};");
        }

        private void Navigate(Uri uri)
        {
            _headerSent = true;
            SignInWebView.CoreWebView2.Navigate(uri.ToString());
        }

        private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            await SignInWebView.EnsureCoreWebView2Async();
            var cookieManager = SignInWebView.CoreWebView2.CookieManager;
            foreach (var cookie in await cookieManager.GetCookiesAsync("https://myanimelist.net"))
            {
                cookieManager.DeleteCookie(cookie);
            }

            ViewModelLocator.LogIn.LogOutCommand.Execute(null);
        }
    }
}
