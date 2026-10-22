using System.ComponentModel;
using MALClient.Models.Enums;
using MALClient.XShared.NavArgs;
using MALClient.XShared.ViewModels;
using MALClient.XShared.ViewModels.Interfaces;
using MALClient.XShared.ViewModels.Main;

namespace MALClient.Maui.Views
{
    public partial class AnimeListView : ContentView, INavigatedView, IDimensionsProvider
    {
        public AnimeListView()
        {
            InitializeComponent();
        }

        private AnimeListViewModel ViewModel => ViewModelLocator.AnimeList;

        //the view model sizes its loading batches from these
        double IDimensionsProvider.ActualWidth => Width > 0 ? Width : 400;
        double IDimensionsProvider.ActualHeight => Height > 0 ? Height : 800;

        public async void OnNavigatedTo(object args)
        {
            ViewModel.DimensionsProvider = this;
            BindingContext = ViewModel;
            ViewModel.PropertyChanged += ViewModelOnPropertyChanged;
            UpdateFilters();
            await ViewModel.Init(args as AnimeListPageNavigationArgs);
            UpdateFilters();
        }

        public void OnNavigatedFrom()
        {
            ViewModel.PropertyChanged -= ViewModelOnPropertyChanged;
            ViewModel.OnNavigatedFrom();
        }

        private void ViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AnimeListViewModel.WorkMode))
                MainThread.BeginInvokeOnMainThread(UpdateFilters);
        }

        private void UpdateFilters()
        {
            var workMode = ViewModel.WorkMode;
            SeasonPicker.IsVisible = workMode == AnimeListWorkModes.SeasonalAnime;
            StatusPicker.IsVisible = workMode == AnimeListWorkModes.Anime || workMode == AnimeListWorkModes.Manga;
            //same order as AnimeListViewModel.GetDesiredStatus
            var statuses = new List<string>
            {
                ViewModel.IsMangaWorkMode ? "Reading" : "Watching", "Completed", "On hold", "Dropped",
                ViewModel.IsMangaWorkMode ? "Plan to read" : "Plan to watch", ViewModel.StatusAllLabel
            };
            if (!(StatusPicker.ItemsSource is List<string> current) || !current.SequenceEqual(statuses))
            {
                StatusPicker.ItemsSource = statuses;
                StatusPicker.SelectedIndex = ViewModel.StatusSelectorSelectedIndex;
            }
        }

        private void ItemsOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is AnimeItemViewModel item)
            {
                Items.SelectedItem = null;
                ViewModel.TemporarilySelectedAnimeItem = item;
            }
        }

        private void ItemsOnScrolled(object sender, ItemsViewScrolledEventArgs e)
        {
            ViewModel.IndefiniteScrollViewerOnViewChanging(e.VerticalOffset);
        }
    }
}
