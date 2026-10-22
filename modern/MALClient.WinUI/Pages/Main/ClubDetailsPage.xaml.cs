using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using MALClient.Models.Models;
using MALClient.UWP.Shared.Managers;
using MALClient.XShared.NavArgs;
using MALClient.XShared.ViewModels;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MALClient.UWP.Pages.Main
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ClubDetailsPage : Page
    {
        private BlurHelper _blur1;
        private BlurHelper _blur2;

        public ClubDetailsPage()
        {
            this.InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            _blur1 = new BlurHelper(LoadingOverlay,false);
            _blur2 = new BlurHelper(LoadingOverlayComments,false);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModelLocator.ClubDetails.NavigatedTo(e.Parameter as ClubDetailsPageNavArgs);
            base.OnNavigatedTo(e);
        }

        private void MemberOnClick(object sender, ItemClickEventArgs e)
        {
            ViewModelLocator.ClubDetails.NavigateUserCommand.Execute(e.ClickedItem);
        }

        private void AnimeItemOnClick(object sender, ItemClickEventArgs e)
        {
            ViewModelLocator.ClubDetails.NavigateAnimeDetailsCommand.Execute(e.ClickedItem);
        }

        private void MangaItemOnClick(object sender, ItemClickEventArgs e)
        {
            ViewModelLocator.ClubDetails.NavigateMangaDetailsCommand.Execute(e.ClickedItem);
        }
    }
}
