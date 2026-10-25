using System.Collections.ObjectModel;
using System.Windows.Input;
using MALClient.Models.Enums;
using MALClient.XShared.Comm.Anime;
using MALClient.XShared.Interfaces;
using MALClient.XShared.Mvvm;
using MALClient.XShared.NavArgs;
using MALClient.XShared.Utils;
using MALClient.XShared.ViewModels;

namespace MALClient.Maui.ViewModels
{
    public class HamburgerMenuItem : ViewModelBase
    {
        private bool _isActive;

        public HamburgerMenuItem(HamburgerButtons button, string title, Action navigate)
        {
            Button = button;
            Title = title;
            NavigateCommand = new RelayCommand(navigate);
        }

        public HamburgerButtons Button { get; }
        public string Title { get; }
        public ICommand NavigateCommand { get; }

        public bool IsActive
        {
            get => _isActive;
            set => Set(ref _isActive, value);
        }
    }

    /// <summary>
    ///     Flyout menu state. Covers the subset of the desktop/xamarin hamburger the MVP pages need.
    /// </summary>
    public class MauiHamburgerViewModel : ViewModelBase, IHamburgerViewModel
    {
        private string _logInLabel;
        private string _profileImage;
        private bool _mangaSectionVisbility = true;

        public MauiHamburgerViewModel()
        {
            Items = new ObservableCollection<HamburgerMenuItem>
            {
                new HamburgerMenuItem(HamburgerButtons.AnimeList, "Anime list",
                    () => Navigate(PageIndex.PageAnimeList, null)),
                new HamburgerMenuItem(HamburgerButtons.Seasonal, "Seasonal anime",
                    () => Navigate(PageIndex.PageSeasonal,
                        AnimeListPageNavigationArgs.Seasonal)),
                new HamburgerMenuItem(HamburgerButtons.TopAnime, "Top anime",
                    () => Navigate(PageIndex.PageTopAnime,
                        AnimeListPageNavigationArgs.TopAnime(TopAnimeType.General))),
                new HamburgerMenuItem(HamburgerButtons.Recommendations, "Recommendations",
                    () => Navigate(PageIndex.PageRecomendations, null)),
                new HamburgerMenuItem(HamburgerButtons.Calendar, "Calendar",
                    () => Navigate(PageIndex.PageCalendar, null)),
                new HamburgerMenuItem(HamburgerButtons.AnimeSearch, "Search",
                    () => Navigate(PageIndex.PageSearch, new SearchPageNavigationArgs())),
                new HamburgerMenuItem(HamburgerButtons.MangaList, "Manga list",
                    () => Navigate(PageIndex.PageMangaList, AnimeListPageNavigationArgs.Manga)),
                new HamburgerMenuItem(HamburgerButtons.Profile, "Profile",
                    () => Navigate(PageIndex.PageProfile,
                        new ProfilePageNavigationArgs {TargetUser = Credentials.UserName})),
                new HamburgerMenuItem(HamburgerButtons.History, "History",
                    () => Navigate(PageIndex.PageHistory, null)),
                new HamburgerMenuItem(HamburgerButtons.Settings, "Settings",
                    () => Navigate(PageIndex.PageSettings, null)),
            };
            UpdateLogInLabel();
        }

        public ObservableCollection<HamburgerMenuItem> Items { get; }

        public event EventHandler NavigationRequested;

        public string LogInLabel
        {
            get => _logInLabel;
            private set => Set(ref _logInLabel, value);
        }

        public string ProfileImage
        {
            get => _profileImage;
            private set => Set(ref _profileImage, value);
        }

        public ICommand LogInCommand => new RelayCommand(() => Navigate(PageIndex.PageLogIn, null));

        private void Navigate(PageIndex page, object args)
        {
            ViewModelLocator.NavMgr.ResetMainBackNav();
            ViewModelLocator.GeneralMain.Navigate(page, args);
            NavigationRequested?.Invoke(this, EventArgs.Empty);
        }

        public Task UpdateProfileImg(bool dl = true)
        {
            ProfileImage = Credentials.Authenticated && Credentials.Id > 0
                ? $"https://cdn.myanimelist.net/images/userimages/{Credentials.Id}.jpg"
                : null;
            return Task.CompletedTask;
        }

        public void SetActiveButton(HamburgerButtons val)
        {
            foreach (var item in Items)
                item.IsActive = item.Button == val;
        }

        public void SetActiveButton(TopAnimeType topType)
        {
            SetActiveButton(HamburgerButtons.TopAnime);
        }

        public void UpdateApiDependentButtons()
        {
        }

        public void UpdateAnimeFiltersSelectedIndex()
        {
        }

        public void UpdateLogInLabel()
        {
            LogInLabel = Credentials.Authenticated ? $"Signed in as {Credentials.UserName}" : "Sign in";
            _ = UpdateProfileImg();
        }

        public bool MangaSectionVisbility
        {
            get => _mangaSectionVisbility;
            set => Set(ref _mangaSectionVisbility, value);
        }

        public void UpdatePinnedProfiles()
        {
        }

        public void UpdateBottomMargin()
        {
        }
    }
}
