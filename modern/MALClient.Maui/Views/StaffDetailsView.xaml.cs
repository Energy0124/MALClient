using MALClient.Models.Models.Anime;
using MALClient.Models.Models.ScrappedDetails;
using MALClient.XShared.NavArgs;
using MALClient.XShared.ViewModels;
using MALClient.XShared.ViewModels.Details;

namespace MALClient.Maui.Views
{
    public partial class StaffDetailsView : ContentView, INavigatedView
    {
        public StaffDetailsView()
        {
            InitializeComponent();
        }

        private StaffDetailsViewModel ViewModel => ViewModelLocator.StaffDetails;

        public void OnNavigatedTo(object args)
        {
            BindingContext = ViewModel;
            ViewModel.Init(args as StaffDetailsNaviagtionArgs);
        }

        public void OnNavigatedFrom()
        {
        }

        private void ShowOnTapped(object sender, TappedEventArgs e)
        {
            if ((sender as BindableObject)?.BindingContext is ShowCharacterPair pair)
                ViewModel.NavigateAnimeDetailsCommand.Execute(pair.AnimeLightEntry);
        }

        private void CharacterOnTapped(object sender, TappedEventArgs e)
        {
            if ((sender as BindableObject)?.BindingContext is ShowCharacterPair pair)
                ViewModel.NavigateCharacterDetailsCommand.Execute(pair.AnimeCharacter);
        }

        private void PositionOnTapped(object sender, TappedEventArgs e)
        {
            if ((sender as BindableObject)?.BindingContext is AnimeLightEntry entry)
                ViewModel.NavigateAnimeDetailsCommand.Execute(entry);
        }
    }
}
