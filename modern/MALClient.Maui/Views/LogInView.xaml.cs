using System.Text.RegularExpressions;
using MALClient.Models.Enums;
using MALClient.XShared.Utils;
using MALClient.XShared.ViewModels;
using MALClient.XShared.ViewModels.Main;
using CookieManager = Android.Webkit.CookieManager;

namespace MALClient.Maui.Views
{
    /// <summary>
    ///     MAL web login followed by the OAuth (PKCE) authorization, ported from the Xamarin fragment: session cookies
    ///     are taken once the login lands on the MAL home page, then the authorize redirect carries the code.
    /// </summary>
    public partial class LogInView : ContentView, INavigatedView
    {
        private const string MobileUserAgent =
            "Mozilla/5.0 (Linux; Android 7.0; SM-G930V Build/NRD90M) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/59.0.3071.125 Mobile Safari/537.36";

        private string _cookies;
        private bool _tokenStage;
        private bool _googleFlow;

        public LogInView()
        {
            InitializeComponent();
            AuthWebView.HandlerChanged += (sender, args) =>
            {
                if (AuthWebView.Handler?.PlatformView is Android.Webkit.WebView webView)
                {
                    webView.Settings.JavaScriptEnabled = true;
                    webView.Settings.UserAgentString = MobileUserAgent;
                }
            };
        }

        private LogInViewModel ViewModel => ViewModelLocator.LogIn;

        public void OnNavigatedTo(object args)
        {
            BindingContext = ViewModel;
            ViewModel.Init();
            if (Credentials.Authenticated)
            {
                ViewModelLocator.NavMgr.ResetMainBackNav();
                ViewModelLocator.NavMgr.RegisterBackNav(PageIndex.PageAnimeList, null);
            }

            ViewModel.Authenticating = false;
        }

        public void OnNavigatedFrom()
        {
        }

        private void SignInOnClicked(object sender, EventArgs e)
        {
            if (ViewModel.Authenticating)
                return;
            ViewModel.Authenticating = true;
            _tokenStage = false;
            _googleFlow = false;
            AuthWebView.IsVisible = true;
            AuthWebView.Source = "https://myanimelist.net/login.php";
        }

        private void LogOutOnClicked(object sender, EventArgs e)
        {
            CookieManager.Instance?.RemoveAllCookies(null);
            CookieManager.Instance?.Flush();
            ViewModel.LogOutCommand.Execute(null);
        }

        private void AuthWebViewOnNavigating(object sender, WebNavigatingEventArgs e)
        {
            var target = _tokenStage ? InterceptToken(e.Url) : InterceptLogin(e.Url);
            if (target == e.Url)
                return;

            e.Cancel = true;
            if (target != null)
                AuthWebView.Source = target;
        }

        /// <summary>
        ///     Returns where the web view should go instead: the same url allows the navigation.
        /// </summary>
        private string InterceptLogin(string targetUrl)
        {
            if (targetUrl == "https://myanimelist.net/" ||
                targetUrl == "https://myanimelist.net/#" || //from google sign in
                targetUrl.StartsWith("https://myanimelist.net/#")) //from fb sign in
            {
                _cookies = CookieManager.Instance?.GetCookie("https://myanimelist.net");
                _tokenStage = true;
                return "https://myanimelist.net/v1/oauth2/authorize?response_type=code&" +
                       "client_id=183063f74126e7551b00c3b4de66986c&" +
                       "state=signin&" +
                       $"code_challenge={ViewModel.PkceChallenge}&" +
                       "code_challenge_method=plain";
            }

            if (targetUrl == "https://myanimelist.net/register.php")
            {
                ViewModel.NavigateRegister.Execute(null);
                return null;
            }

            if (!_googleFlow && targetUrl.Contains("google"))
                _googleFlow = true;
            if (_googleFlow)
                return targetUrl;

            if (targetUrl.StartsWith("https://api.twitter") ||
                targetUrl.StartsWith("https://myanimelist.net/sns/login") ||
                targetUrl.StartsWith("https://myanimelist.net/sns/callback") ||
                targetUrl.Contains("apple") ||
                targetUrl.StartsWith("https://www.facebook") ||
                targetUrl.StartsWith("https://m.facebook") ||
                targetUrl.StartsWith("https://accounts.google") ||
                targetUrl.StartsWith("https://accounts.youtube") ||
                targetUrl.StartsWith("https://myanimelist.net/login.php"))
                return targetUrl;

            return "https://myanimelist.net/login.php";
        }

        private string InterceptToken(string targetUrl)
        {
            //http://localhost/maloauth?state=signin&error=access_denied&...
            if (targetUrl.Contains("maloauth?state=signin&error="))
            {
                AuthWebView.IsVisible = false;
                _tokenStage = false;
                ViewModel.FailedSignIn();
                return null;
            }

            if (targetUrl.Contains("maloauth"))
            {
                AuthWebView.IsVisible = false;
                _tokenStage = false;
                var match = Regex.Match(targetUrl, ".*maloauth\\?code=(.*?)(\\&.*|$)");
                if (match.Success)
                    ViewModel.SignIn(_cookies, match.Groups[1].Value);
                else
                    ViewModel.FailedSignIn();
                return null;
            }

            return targetUrl;
        }
    }
}
