using System.ComponentModel;
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

        //DetailsPivotSelectedIndex is 1 + the open section's index here, 0 when none is open
        private View[] Sections => new View[]
            {DetailsSection, ReviewsSection, RelatedSection, RecommendationsSection, CharactersSection};

        public void OnNavigatedTo(object args)
        {
            BindingContext = ViewModel;
            ViewModel.PropertyChanged += ViewModelOnPropertyChanged;
            ViewModel.Init(args as AnimeDetailsPageNavigationArgs);
        }

        public void OnNavigatedFrom()
        {
            ViewModel.PropertyChanged -= ViewModelOnPropertyChanged;
        }

        //Init restores the index the page was left with, e.g. when coming back from a character
        private void ViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(AnimeDetailsPageViewModel.DetailsPivotSelectedIndex))
                return;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var index = ViewModel.DetailsPivotSelectedIndex - 1;
                if (index < 0 || index >= Sections.Length || Sections[index].IsVisible)
                    return;
                EventHandler open = index switch
                {
                    0 => DetailsOnClicked,
                    1 => ReviewsOnClicked,
                    2 => RelatedOnClicked,
                    3 => RecommendationsOnClicked,
                    _ => CharactersOnClicked
                };
                open(this, EventArgs.Empty);
            });
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
            var sections = Sections;
            foreach (var view in sections)
                view.IsVisible = view == section;
            ViewModel.DetailsPivotSelectedIndex = Array.IndexOf(sections, section) + 1;
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

        private void AirNotificationsOnToggled(object sender, ToggledEventArgs e)
        {
            //the binding also lands here when the view model changes
            if (e.Value != ViewModel.AreAirNotificationsEnabled)
                ViewModel.ToggleAirNotificationsCommand.Execute(null);
        }

        private void CharactersOnClicked(object sender, EventArgs e)
        {
            ShowSection(CharactersSection);
            if (ViewModel.AnimeStaffData == null)
                ViewModel.LoadCharacters();
        }

        private void CharacterOnTapped(object sender, TappedEventArgs e)
        {
            if ((sender as BindableObject)?.BindingContext is
                AnimeDetailsPageViewModel.AnimeStaffDataViewModels.AnimeCharacterStaffModelViewModel pair)
                ViewModel.NavigateCharacterDetailsCommand.Execute(pair.AnimeCharacter.Data);
        }

        private void VoiceActorOnTapped(object sender, TappedEventArgs e)
        {
            if ((sender as BindableObject)?.BindingContext is
                AnimeDetailsPageViewModel.AnimeStaffDataViewModels.AnimeCharacterStaffModelViewModel pair)
                ViewModel.NavigateStaffDetailsCommand.Execute(pair.AnimeStaffPerson.Data);
        }

        private void StaffOnTapped(object sender, TappedEventArgs e)
        {
            if ((sender as BindableObject)?.BindingContext is FavouriteViewModel person)
                ViewModel.NavigateStaffDetailsCommand.Execute(person.Data);
        }

        //related and recommended entries both open their own details page
        private void DetailsArgsOnTapped(object sender, TappedEventArgs e)
        {
            if ((sender as BindableObject)?.BindingContext is IDetailsPageArgs args)
                ViewModel.NavigateDetailsCommand.Execute(args);
        }
    }
}
