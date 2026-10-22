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
