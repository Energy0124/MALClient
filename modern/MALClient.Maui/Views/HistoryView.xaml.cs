using MALClient.Models.Enums;
using MALClient.Models.Models.MalSpecific;
using MALClient.XShared.NavArgs;
using MALClient.XShared.Utils;
using MALClient.XShared.ViewModels;
using MALClient.XShared.ViewModels.Main;

namespace MALClient.Maui.Views
{
    public partial class HistoryView : ContentView, INavigatedView
    {
        private HistoryNavigationArgs _args;

        public HistoryView()
        {
            InitializeComponent();
        }

        private HistoryViewModel ViewModel => ViewModelLocator.History;

        public void OnNavigatedTo(object args)
        {
            BindingContext = ViewModel;
            _args = args as HistoryNavigationArgs ?? new HistoryNavigationArgs {Source = Credentials.UserName};
            ViewModel.Init(_args);
        }

        public void OnNavigatedFrom()
        {
        }

        private void EntryOnTapped(object sender, TappedEventArgs e)
        {
            //shows that are no longer on the user's list come back without a view model
            if ((sender as BindableObject)?.BindingContext is Tuple<AnimeItemViewModel, List<MalProfileHistoryEntry>>
                {
                    Item1: { } item
                })
                item.NavigateDetails(PageIndex.PageHistory, _args);
        }
    }
}
