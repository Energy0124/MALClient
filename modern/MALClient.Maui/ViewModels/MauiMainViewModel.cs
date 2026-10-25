using System.Windows.Input;
using MALClient.Maui.Views;
using MALClient.Models.Enums;
using MALClient.XShared.Delegates;
using MALClient.XShared.Mvvm;
using MALClient.XShared.NavArgs;
using MALClient.XShared.Utils;
using MALClient.XShared.ViewModels;

namespace MALClient.Maui.ViewModels
{
    /// <summary>
    ///     Single pane navigation host logic, ported from the Xamarin app's MainViewModel. Pages without a MAUI view
    ///     yet navigate to <see cref="NotPortedView" />.
    /// </summary>
    public class MauiMainViewModel : MainViewModelBase
    {
        public override event MALClient.XShared.Delegates.NavigationRequest MainNavigationRequested;

        protected override void CurrentStatusStoryboardBegin()
        {
        }

        protected override void CurrentOffSubStatusStoryboardBegin()
        {
        }

        protected override void CurrentOffStatusStoryboardBegin()
        {
        }

        public override void Navigate(PageIndex index, object args = null)
        {
            var originalIndex = index;
            var wasOnSearchPage = SearchToggleLock;
            SearchToggleLock = false;
            CurrentStatusSub = "";
            IsCurrentStatusSelectable = false;
            if (!Credentials.Authenticated && PageUtils.PageRequiresAuth(index))
            {
                ResourceLocator.MessageDialogProvider.ShowMessageDialog("Log in first in order to access this page.",
                    "Log in required");
                return;
            }

            ResourceLocator.TelemetryProvider.TelemetryTrackNavigation(index);

            ScrollToTopButtonVisibility = false;
            RefreshButtonVisibility = false;
            ViewModelLocator.AnimeDetails.Id = -1;

            if (index == PageIndex.PageMangaList && args == null) // navigating from startup
                args = AnimeListPageNavigationArgs.Manga;

            if (index == PageIndex.PageSeasonal ||
                index == PageIndex.PageMangaList ||
                index == PageIndex.PageTopManga ||
                index == PageIndex.PageTopAnime)
                index = PageIndex.PageAnimeList;

            if (index == PageIndex.PageAnimeList && _searchStateBeforeNavigatingToSearch != null)
            {
                SearchToggleStatus = (bool) _searchStateBeforeNavigatingToSearch;
                if (SearchToggleStatus)
                    ShowSearchStuff();
                else
                    HideSearchStuff();
            }

            switch (index)
            {
                case PageIndex.PageAnimeList:
                    if (ViewModelLocator.AnimeList.Initializing)
                    {
                        if (!_subscribed)
                        {
                            ViewModelLocator.AnimeList.Initialized += AnimeListOnInitialized;
                            _subscribed = true;
                        }

                        _postponedNavigationArgs = new Tuple<PageIndex, object>(originalIndex, args);
                        return;
                    }

                    ViewModelLocator.GeneralHamburger.SetActiveButton(
                        ((args as AnimeListPageNavigationArgs)?.WorkMode ?? AnimeListWorkModes.Anime) switch
                        {
                            AnimeListWorkModes.SeasonalAnime => HamburgerButtons.Seasonal,
                            AnimeListWorkModes.Manga => HamburgerButtons.MangaList,
                            AnimeListWorkModes.TopAnime => HamburgerButtons.TopAnime,
                            AnimeListWorkModes.TopManga => HamburgerButtons.TopManga,
                            _ => HamburgerButtons.AnimeList
                        });
                    ShowSearchStuff();
                    if ((_searchStateBeforeNavigatingToSearch == null || !_searchStateBeforeNavigatingToSearch.Value) &&
                        (wasOnSearchPage || _wasOnDetailsFromSearch))
                    {
                        CurrentSearchQuery = "";
                        _wasOnDetailsFromSearch = false;
                        UnToggleSearchStuff();
                    }

                    var listArgs = args as AnimeListPageNavigationArgs;
                    if (CurrentMainPage == PageIndex.PageAnimeList)
                        ViewModelLocator.AnimeList.Init(listArgs);
                    else
                        MainNavigationRequested?.Invoke(typeof(AnimeListView), listArgs);

                    if (listArgs != null && listArgs.WorkMode == AnimeListWorkModes.Manga && listArgs.ResetBackNav)
                    {
                        ViewModelLocator.NavMgr.DeregisterBackNav();
                        ViewModelLocator.NavMgr.RegisterBackNav(PageIndex.PageAnimeList, null);
                    }

                    break;
                case PageIndex.PageAnimeDetails:
                    HideSearchStuff();
                    var detail = ViewModelLocator.AnimeDetails;
                    detail.DetailImage = null;
                    detail.LeftDetailsRow.Clear();
                    detail.RightDetailsRow.Clear();
                    RefreshButtonVisibility = true;
                    RefreshDataCommand = new RelayCommand(() => ViewModelLocator.AnimeDetails.RefreshData());
                    _wasOnDetailsFromSearch = (args as AnimeDetailsPageNavigationArgs)?.Source == PageIndex.PageSearch;
                    MainNavigationRequested?.Invoke(typeof(AnimeDetailsView), args);
                    break;
                case PageIndex.PageSettings:
                    HideSearchStuff();
                    CurrentStatus = "Settings";
                    MainNavigationRequested?.Invoke(typeof(SettingsView), args);
                    break;
                case PageIndex.PageSearch:
                case PageIndex.PageMangaSearch:
                    if (!IsSearchPage(CurrentMainPage))
                        _searchStateBeforeNavigatingToSearch = SearchToggleStatus;

                    if (args is SearchPageNavigationArgs searchArg && string.IsNullOrWhiteSpace(searchArg.Query))
                        searchArg.Query = CurrentSearchQuery;
                    HideSearchStuff();
                    CurrentStatus = index == PageIndex.PageMangaSearch ? "Manga search" : "Anime search";
                    MainNavigationRequested?.Invoke(typeof(SearchView), args);
                    break;
                case PageIndex.PageCharacterSearch:
                    if (!IsSearchPage(CurrentMainPage))
                        _searchStateBeforeNavigatingToSearch = SearchToggleStatus;
                    //CharacterSearchViewModel listens to OnSearchQuerySubmitted, which only fires while search is
                    //toggled on and locked to the search page
                    SearchToggleLock = true;
                    ToggleSearchStuff();
                    CurrentStatus = "Character search";
                    MainNavigationRequested?.Invoke(typeof(CharacterSearchView), args);
                    break;
                case PageIndex.PageCharacterDetails:
                    HideSearchStuff();
                    RefreshButtonVisibility = true;
                    RefreshDataCommand = new RelayCommand(() => ViewModelLocator.CharacterDetails.RefreshData());
                    if (CurrentMainPage == PageIndex.PageCharacterDetails)
                        ViewModelLocator.CharacterDetails.Init(args as CharacterDetailsNavigationArgs);
                    else
                        MainNavigationRequested?.Invoke(typeof(CharacterDetailsView), args);
                    break;
                case PageIndex.PageStaffDetails:
                    HideSearchStuff();
                    RefreshButtonVisibility = true;
                    RefreshDataCommand = new RelayCommand(() => ViewModelLocator.StaffDetails.RefreshData());
                    if (CurrentMainPage == PageIndex.PageStaffDetails)
                        ViewModelLocator.StaffDetails.Init(args as StaffDetailsNaviagtionArgs);
                    else
                        MainNavigationRequested?.Invoke(typeof(StaffDetailsView), args);
                    break;
                case PageIndex.PageRecomendations:
                    HideSearchStuff();
                    RefreshButtonVisibility = true;
                    RefreshDataCommand = new RelayCommand(() => ViewModelLocator.Recommendations.PopulateData(true));
                    ViewModelLocator.GeneralHamburger.SetActiveButton(HamburgerButtons.Recommendations);
                    MainNavigationRequested?.Invoke(typeof(RecommendationsView), args);
                    break;
                case PageIndex.PageHistory:
                    HideSearchStuff();
                    RefreshButtonVisibility = true;
                    RefreshDataCommand = new RelayCommand(() => ViewModelLocator.History.Init(null, true));
                    ViewModelLocator.GeneralHamburger.SetActiveButton(HamburgerButtons.History);
                    CurrentStatus = $"History - {(args as HistoryNavigationArgs)?.Source ?? Credentials.UserName}";
                    MainNavigationRequested?.Invoke(typeof(HistoryView), args);
                    break;
                case PageIndex.PageLogIn:
                    HideSearchStuff();
                    CurrentStatus = "Log In";
                    MainNavigationRequested?.Invoke(typeof(LogInView), args);
                    break;
                case PageIndex.PageProfile:
                    HideSearchStuff();
                    RefreshButtonVisibility = true;
                    RefreshDataCommand =
                        new RelayCommand(async () => await ViewModelLocator.ProfilePage.LoadProfileData(null, true));
                    if (CurrentMainPage == PageIndex.PageProfile)
                        _ = ViewModelLocator.ProfilePage.LoadProfileData(args as ProfilePageNavigationArgs);
                    else
                        MainNavigationRequested?.Invoke(typeof(ProfileView), args);
                    break;
                default:
                    HideSearchStuff();
                    CurrentStatus = index.ToString().Replace("Page", "");
                    MainNavigationRequested?.Invoke(typeof(NotPortedView), index);
                    break;
            }

            CurrentMainPage = index;
            CurrentMainPageKind = index;
            RaisePropertyChanged(() => SearchToggleLock);
        }

        private static bool IsSearchPage(PageIndex? page)
        {
            return page == PageIndex.PageSearch || page == PageIndex.PageMangaSearch ||
                   page == PageIndex.PageCharacterSearch;
        }

        public override string CurrentOffStatus
        {
            get => CurrentStatus;
            set => CurrentStatus = value;
        }

        public override ICommand RefreshOffDataCommand
        {
            get => RefreshDataCommand;
            set => RefreshDataCommand = value;
        }

        public void PerformFirstNavigation()
        {
            var hasArgumentsWithSync =
                InitDetailsFull?.Item1.GetAttribute<EnumUtilities.PageIndexEnumMember>().RequiresSyncBlock ?? true;
            if (Credentials.Authenticated)
            {
                if (hasArgumentsWithSync)
                    Navigate(Settings.DefaultMenuTab == "anime" ? PageIndex.PageAnimeList : PageIndex.PageMangaList);
                else if (InitDetailsFull != null)
                {
                    _ = ViewModelLocator.AnimeList.Init(null);
                    Navigate(InitDetailsFull.Item1, InitDetailsFull.Item2);
                }
            }
            else
            {
                Navigate(PageIndex.PageLogIn);
            }

            if (InitDetails != null || hasArgumentsWithSync)
                ViewModelLocator.AnimeList.Initialized += AnimeListOnInitializedLoadArgs;
        }
    }
}
