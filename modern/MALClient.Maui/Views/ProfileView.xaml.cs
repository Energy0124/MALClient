using MALClient.Models.Models.Favourites;
using MALClient.XShared.NavArgs;
using MALClient.XShared.ViewModels;
using MALClient.XShared.ViewModels.Main;

namespace MALClient.Maui.Views
{
    public partial class ProfileView : ContentView, INavigatedView
    {
        public ProfileView()
        {
            InitializeComponent();
        }

        private ProfilePageViewModel ViewModel => ViewModelLocator.ProfilePage;

        public async void OnNavigatedTo(object args)
        {
            BindingContext = ViewModel;
            await ViewModel.LoadProfileData(args as ProfilePageNavigationArgs);
        }

        public void OnNavigatedFrom()
        {
        }

        private void CharactersOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is AnimeCharacter character)
            {
                ((CollectionView) sender).SelectedItem = null;
                ViewModel.NavigateCharacterDetailsCommand.Execute(character);
            }
        }

        private void PeopleOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is AnimeStaffPerson person)
            {
                ((CollectionView) sender).SelectedItem = null;
                ViewModel.NavigateStaffDetailsCommand.Execute(person);
            }
        }

        private void AnimeOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is AnimeItemViewModel item)
            {
                ((CollectionView) sender).SelectedItem = null;
                ViewModel.TemporarilySelectedAnimeItem = item;
            }
        }
    }
}
