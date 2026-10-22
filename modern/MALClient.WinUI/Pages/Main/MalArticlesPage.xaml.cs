using MALClient.WinUI.Compat;
using Microsoft.Web.WebView2.Core;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MALClient.XShared.Mvvm;
using MALClient.Models.Enums;
using MALClient.Models.Models.MalSpecific;
using MALClient.UWP.Adapters;
using MALClient.UWP.Shared.Managers;
using MALClient.XShared.Comm.Anime;
using MALClient.XShared.NavArgs;
using MALClient.XShared.Utils;
using MALClient.XShared.ViewModels;
using MALClient.XShared.ViewModels.Main;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace MALClient.UWP.Pages.Main
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MalArticlesPage : Page
    {
       
        private MalArticlesPageNavigationArgs _lastArgs;

        public MalArticlesPage()
        {
            InitializeComponent();
            Loaded += (sedner, args) => ViewModel.Init(_lastArgs);
            ViewModel.OpenWebView += ViewModelOnOpenWebView;
        }

        public MalArticlesViewModel ViewModel => DataContext as MalArticlesViewModel;

        private async void ViewModelOnOpenWebView(string html,MalNewsUnitModel id)
        {
            //back nav
            ViewModelLocator.NavMgr.RegisterOneTimeMainOverride(new RelayCommand(() =>
            {
                ViewModel.WebViewVisibility = false;
                ViewModel.ArticleIndexVisibility = true;
                ViewModelLocator.GeneralMain.CurrentStatus = ViewModel.PrevWorkMode != null &&
                                                      ViewModel.PrevWorkMode.Value == ArticlePageWorkMode.Articles
                    ? "Articles"
                    : "News";
                ViewModel.CurrentNews = -1;
            }));
            //

            
            await ArticleWebView.NavigateToStringAsync(ResourceLocator.CssManager.WrapWithCss(html));
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModelLocator.NavMgr.ResetMainBackNav();
            _lastArgs = e.Parameter as MalArticlesPageNavigationArgs;
            base.OnNavigatedTo(e);
        }

        private void ListViewBase_OnItemClick(object sender, ItemClickEventArgs e)
        {
            ViewModel.LoadArticleCommand.Execute(e.ClickedItem);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            ViewModelLocator.NavMgr.ResetOneTimeMainOverride();
            if(!(e.Parameter is ProfilePageNavigationArgs))
                ViewModelLocator.NavMgr.ResetMainBackNav();
            base.OnNavigatedFrom(e);
        }

        private async void ArticleWebView_OnNavigationStarting(WebView2 sender, CoreWebView2NavigationStartingEventArgs args)
        {
            try
            {
                var targetUri = args.GetExternalUri();
                if (targetUri != null)
                {
                    var uri = targetUri.ToString();
                    var paramIndex = uri.IndexOf('?');
                    if (paramIndex != -1)
                        uri = uri.Substring(0, paramIndex);
                    args.Cancel = true;
                    if (Regex.IsMatch(uri, "anime\\/\\d") ||
                        (Settings.SelectedApiType != ApiType.Hummingbird && Regex.IsMatch(uri, "manga\\/\\d")))
                    {
                        var link = uri.Substring(8).Split('/');
                        //if (link[3] == "")
                        //{
                        //    if (Settings.ArticlesLaunchExternalLinks)
                        //    {
                        //        ResourceLocator.SystemControlsLauncherService.LaunchUri(targetUri);
                        //    }
                        //    return;
                        //}
                        var id = int.Parse(link[2]);
                        //if (Settings.SelectedApiType == ApiType.Hummingbird) //id switch
                        //    id = await new AnimeDetailsHummingbirdQuery(id).GetHummingbirdId();
                        ViewModelLocator.GeneralMain.Navigate(PageIndex.PageAnimeDetails,
                            new AnimeDetailsPageNavigationArgs(id, link.Length == 4 ? link[3] : null, null, null)
                            {
                                AnimeMode = link[1] == "anime"
                            });
                    }
                    else if (uri.Contains("/profile/"))
                    {
                        var vm = ViewModelLocator.MalArticles;
                        ViewModelLocator.NavMgr.RegisterBackNav(PageIndex.PageArticles,new MalArticlesPageNavigationArgs { NewsId = vm.CurrentNews, WorkMode = vm.PrevWorkMode.Value});
                        ViewModelLocator.GeneralMain.Navigate(PageIndex.PageProfile,new ProfilePageNavigationArgs {TargetUser = uri.Split('/').Last()});
                    }
                    else if (Settings.ArticlesLaunchExternalLinks)
                    {
                        ResourceLocator.SystemControlsLauncherService.LaunchUri(targetUri);
                    }
                }
            }
            catch (Exception)
            {
                args.Cancel = true;
            }
        }

        private void ArticleWebView_OnNavigationCompleted(WebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
        {
            ViewModel.LoadingVisibility = false;
            ViewModel.WebViewVisibility = true;
        }

      


    }
}