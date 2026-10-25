using System.ComponentModel;
using MALClient.Models.Enums;
using MALClient.XShared.NavArgs;
using MALClient.XShared.Utils;
using MALClient.XShared.ViewModels;
using MALClient.XShared.ViewModels.Main;

namespace MALClient.Maui.Views
{
    public partial class RecommendationsView : ContentView, INavigatedView
    {
        private static readonly RecommendationsPageWorkMode[] WorkModes =
            Enum.GetValues<RecommendationsPageWorkMode>();

        public RecommendationsView()
        {
            InitializeComponent();
            ModePicker.ItemsSource = WorkModes.Select(mode => mode.GetDescription()).ToList();
        }

        private RecommendationsViewModel ViewModel => ViewModelLocator.Recommendations;

        private List<RecommendationsViewModel.XPivotItem> CurrentPairs =>
            ViewModel.CurrentWorkMode switch
            {
                RecommendationsPageWorkMode.Anime => ViewModel.RecommendationAnimeItems,
                RecommendationsPageWorkMode.Manga => ViewModel.RecommendationMangaItems,
                _ => null
            };

        public void OnNavigatedTo(object args)
        {
            BindingContext = ViewModel;
            ViewModel.PropertyChanged += ViewModelOnPropertyChanged;
            //coming back from details restores the pair we left from
            if (args is RecommendationPageNavigationArgs recommendationArgs)
                ViewModel.PivotItemIndex = recommendationArgs.Index;
            UpdateView();
            ViewModel.PopulateData();
        }

        public void OnNavigatedFrom()
        {
            ViewModel.PropertyChanged -= ViewModelOnPropertyChanged;
        }

        private void ViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(RecommendationsViewModel.CurrentWorkMode):
                case nameof(RecommendationsViewModel.PivotItemIndex):
                case nameof(RecommendationsViewModel.RecommendationAnimeItems):
                case nameof(RecommendationsViewModel.RecommendationMangaItems):
                case nameof(RecommendationsViewModel.PersonalizedAnimeItems):
                case nameof(RecommendationsViewModel.PersonalizedMangaItems):
                    MainThread.BeginInvokeOnMainThread(UpdateView);
                    break;
            }
        }

        private void UpdateView()
        {
            var mode = ViewModel.CurrentWorkMode;
            ModePicker.SelectedIndex = Array.IndexOf(WorkModes, mode);
            var personalized = mode == RecommendationsPageWorkMode.PersonalizedAnime ||
                               mode == RecommendationsPageWorkMode.PersonalizedManga;
            PairsPanel.IsVisible = !personalized;
            Personalized.IsVisible = personalized;
            if (personalized)
                Personalized.ItemsSource = mode == RecommendationsPageWorkMode.PersonalizedAnime
                    ? ViewModel.PersonalizedAnimeItems
                    : ViewModel.PersonalizedMangaItems;
            else
                ShowPair(ViewModel.PivotItemIndex);
        }

        private void ShowPair(int index)
        {
            var pairs = CurrentPairs;
            if (pairs == null || pairs.Count == 0)
            {
                PairContent.IsVisible = false;
                PositionLabel.Text = string.Empty;
                return;
            }

            index = Math.Clamp(index, 0, pairs.Count - 1);
            var item = (RecommendationItemViewModel) pairs[index].Content;
            PairContent.IsVisible = true;
            if (PairContent.BindingContext != item)
            {
                PairContent.BindingContext = item;
                _ = PairContent.ScrollToAsync(0, 0, false);
            }
            PositionLabel.Text = $"{index + 1} / {pairs.Count}";
            item.PopulateData();
            //the setter notifies, which lands back here with an unchanged index
            if (ViewModel.PivotItemIndex != index)
                ViewModel.PivotItemIndex = index;
        }

        private void PreviousOnClicked(object sender, EventArgs e)
        {
            ShowPair(ViewModel.PivotItemIndex - 1);
        }

        private void NextOnClicked(object sender, EventArgs e)
        {
            ShowPair(ViewModel.PivotItemIndex + 1);
        }

        private void ModeOnSelectedIndexChanged(object sender, EventArgs e)
        {
            if (ModePicker.SelectedIndex >= 0)
                ViewModel.CurrentWorkMode = WorkModes[ModePicker.SelectedIndex];
        }

        private void PersonalizedOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is AnimeItemViewModel item)
            {
                Personalized.SelectedItem = null;
                item.NavigateDetails(PageIndex.PageRecomendations,
                    new RecommendationPageNavigationArgs {Index = ViewModel.PivotItemIndex});
            }
        }
    }
}
