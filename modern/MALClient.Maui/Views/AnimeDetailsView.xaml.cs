using MALClient.Maui.Pages;
using MALClient.Models.Interfaces;
using MALClient.XShared.NavArgs;
using MALClient.XShared.ViewModels;
using MALClient.XShared.ViewModels.Details;

namespace MALClient.Maui.Views
{
    public partial class AnimeDetailsView : ContentView, INavigatedView
    {
        public AnimeDetailsView()
        {
            InitializeComponent();
        }

        private AnimeDetailsPageViewModel ViewModel => ViewModelLocator.AnimeDetails;

        public void OnNavigatedTo(object args)
        {
            BindingContext = ViewModel;
            ViewModel.Init(args as AnimeDetailsPageNavigationArgs);
        }

        public void OnNavigatedFrom()
        {
        }

        private async void StatusOnClicked(object sender, EventArgs e)
        {
            var choice = await HostPage.Current.DisplayActionSheetAsync("Status", "Cancel", null,
                ViewModel.Status1Label, "Completed", "On hold", "Dropped", ViewModel.Status5Label);
            if (choice != null && choice != "Cancel")
                ViewModel.ChangeStatusCommand.Execute(choice);
        }

        private async void ScoreOnClicked(object sender, EventArgs e)
        {
            var scores = Enumerable.Range(1, 10).Reverse().Select(score => score.ToString()).ToArray();
            var choice = await HostPage.Current.DisplayActionSheetAsync("Score", "Cancel", "Remove score", scores);
            if (choice != null && choice != "Cancel")
                ViewModel.ChangeScoreCommand.Execute(choice == "Remove score" ? "0" : choice);
        }

        private void ShowSection(View section)
        {
            foreach (var view in new View[] {DetailsSection, ReviewsSection, RelatedSection, RecommendationsSection})
                view.IsVisible = view == section;
        }

        private void DetailsOnClicked(object sender, EventArgs e)
        {
            ShowSection(DetailsSection);
            ViewModel.LoadDetails();
        }

        private void ReviewsOnClicked(object sender, EventArgs e)
        {
            ShowSection(ReviewsSection);
            ViewModel.LoadReviews();
        }

        private void RelatedOnClicked(object sender, EventArgs e)
        {
            ShowSection(RelatedSection);
            ViewModel.LoadRelatedAnime();
        }

        private void RecommendationsOnClicked(object sender, EventArgs e)
        {
            ShowSection(RecommendationsSection);
            ViewModel.LoadRecommendations();
        }

        //related and recommended entries both open their own details page
        private void DetailsArgsOnTapped(object sender, TappedEventArgs e)
        {
            if ((sender as BindableObject)?.BindingContext is IDetailsPageArgs args)
                ViewModel.NavigateDetailsCommand.Execute(args);
        }
    }
}
