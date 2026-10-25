using MALClient.Models.Enums;
using MALClient.XShared.NavArgs;
using MALClient.XShared.ViewModels;
using MALClient.XShared.ViewModels.Main;

namespace MALClient.Maui.Views
{
    public partial class SearchView : ContentView, INavigatedView
    {
        private SearchPageNavigationArgs _args;

        public SearchView()
        {
            InitializeComponent();
        }

        private SearchPageViewModel ViewModel => ViewModelLocator.SearchPage;

        public void OnNavigatedTo(object args)
        {
            BindingContext = ViewModel;
            _args = args as SearchPageNavigationArgs ?? new SearchPageNavigationArgs();
            //the search box lives on this page, not in a main page toolbar
            _args.DisplayMode = SearchPageDisplayModes.Off;
            (_args.Anime ? AnimeMode : MangaMode).IsChecked = true;
            QueryInput.Text = _args.Query;
            ViewModel.Init(_args);
            if (!string.IsNullOrWhiteSpace(_args.Query))
                ViewModel.SubmitQuery(_args.Query);
        }

        public void OnNavigatedFrom()
        {
            ViewModel.OnNavigatedFrom();
        }

        private void QueryOnSubmitted(object sender, EventArgs e)
        {
            ViewModel.SubmitQuery(QueryInput.Text ?? string.Empty);
        }

        private void CharactersOnCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (e.Value)
                ViewModelLocator.GeneralMain.Navigate(PageIndex.PageCharacterSearch,
                    new SearchPageNavigationArgs {Query = QueryInput.Text});
        }

        private void ModeOnCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            //the characters option navigates to its own page instead
            if (_args == null || CharactersMode.IsChecked || _args.Anime == AnimeMode.IsChecked)
                return;

            _args.Anime = AnimeMode.IsChecked;
            ViewModelLocator.GeneralMain.CurrentStatus = _args.Anime ? "Anime search" : "Manga search";
            ViewModel.Init(_args);
            if (!string.IsNullOrWhiteSpace(QueryInput.Text))
                ViewModel.SubmitQuery(QueryInput.Text);
        }

        private void ResultsOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is AnimeSearchItemViewModel item)
            {
                Results.SelectedItem = null;
                ViewModel.CurrentlySelectedItem = item;
            }
        }
    }
}
