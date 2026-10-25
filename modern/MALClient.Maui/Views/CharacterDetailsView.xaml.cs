using MALClient.Models.Models.Anime;
using MALClient.XShared.NavArgs;
using MALClient.XShared.ViewModels;
using MALClient.XShared.ViewModels.Details;

namespace MALClient.Maui.Views
{
    public partial class CharacterDetailsView : ContentView, INavigatedView
    {
        public CharacterDetailsView()
        {
            InitializeComponent();
        }

        private CharacterDetailsViewModel ViewModel => ViewModelLocator.CharacterDetails;

        public void OnNavigatedTo(object args)
        {
            BindingContext = ViewModel;
            ViewModel.Init(args as CharacterDetailsNavigationArgs);
        }

        public void OnNavigatedFrom()
        {
        }

        private void SpoilerOnClicked(object sender, EventArgs e)
        {
            SpoilerLabel.IsVisible = !SpoilerLabel.IsVisible;
            SpoilerButton.Text = SpoilerLabel.IsVisible ? "Hide spoiler" : "Show spoiler";
        }

        private void VoiceActorOnTapped(object sender, TappedEventArgs e)
        {
            if ((sender as BindableObject)?.BindingContext is FavouriteViewModel actor)
                ViewModel.NavigateStaffDetailsCommand.Execute(actor.Data);
        }

        private void AnimeOnTapped(object sender, TappedEventArgs e)
        {
            //the details page registers the way back here from its navigation args
            if ((sender as BindableObject)?.BindingContext is AnimeLightEntry entry)
                ViewModel.NavigateAnimeDetailsCommand.Execute(entry);
        }

        private void MangaOnTapped(object sender, TappedEventArgs e)
        {
            if ((sender as BindableObject)?.BindingContext is AnimeLightEntry entry)
                ViewModel.NavigateMangaDetailsCommand.Execute(entry);
        }
    }
}
