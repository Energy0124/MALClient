using MALClient.Models.Enums;
using MALClient.XShared.NavArgs;
using MALClient.XShared.ViewModels;
using MALClient.XShared.ViewModels.Main;

namespace MALClient.Maui.Views
{
    /// <summary>
    ///     The view model listens to the main view model's search events, so queries go through
    ///     <see cref="MainViewModelBase.CurrentSearchQuery" /> like the Xamarin app's toolbar search.
    /// </summary>
    public partial class CharacterSearchView : ContentView, INavigatedView
    {
        public CharacterSearchView()
        {
            InitializeComponent();
        }

        private CharacterSearchViewModel ViewModel => ViewModelLocator.CharacterSearch;

        public void OnNavigatedTo(object args)
        {
            BindingContext = ViewModel;
            var query = (args as SearchPageNavigationArgs)?.Query;
            if (!string.IsNullOrWhiteSpace(query))
                ViewModelLocator.GeneralMain.CurrentSearchQuery = query;
            QueryInput.Text = ViewModelLocator.GeneralMain.CurrentSearchQuery;
            ViewModel.Init(args as SearchPageNavArgsBase);
        }

        public void OnNavigatedFrom()
        {
            ViewModel.OnNavigatedFrom();
        }

        private void QueryOnSubmitted(object sender, EventArgs e)
        {
            ViewModelLocator.GeneralMain.CurrentSearchQuery = QueryInput.Text ?? string.Empty;
            ViewModelLocator.GeneralMain.OnSearchInputSubmit();
        }

        private void AnimeOnCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (e.Value)
                SwitchTo(PageIndex.PageSearch, true);
        }

        private void MangaOnCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (e.Value)
                SwitchTo(PageIndex.PageMangaSearch, false);
        }

        private void SwitchTo(PageIndex page, bool anime)
        {
            ViewModelLocator.GeneralMain.Navigate(page,
                new SearchPageNavigationArgs {Anime = anime, Query = QueryInput.Text});
        }

        private void ResultsOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is FavouriteViewModel character)
            {
                Results.SelectedItem = null;
                ViewModel.NavigateCharacterDetailsCommand.Execute(character);
            }
        }
    }
}
