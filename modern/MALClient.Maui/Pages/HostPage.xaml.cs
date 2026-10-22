using MALClient.Maui.Managers;
using MALClient.Maui.ViewModels;
using MALClient.Maui.Views;
using MALClient.XShared.ViewModels;

namespace MALClient.Maui.Pages
{
    /// <summary>
    ///     Hosts one content view at a time, swapped by <see cref="MauiMainViewModel" /> navigation requests.
    /// </summary>
    public partial class HostPage : ContentPage
    {
        private readonly Action _toggleMenu;

        public HostPage(Action toggleMenu)
        {
            _toggleMenu = toggleMenu;
            Current = this;
            InitializeComponent();
            BindingContext = MauiViewModelLocator.Main;
            MauiViewModelLocator.Main.MainNavigationRequested += OnNavigationRequested;
        }

        public static HostPage Current { get; private set; }

        private void OnNavigationRequested(Type page, object args)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                (Host.Content as INavigatedView)?.OnNavigatedFrom();
                var view = (View) Activator.CreateInstance(page);
                Host.Content = view;
                (view as INavigatedView)?.OnNavigatedTo(args);
            });
        }

        public void ShowLoading(string title, string content)
        {
            LoadingTitle.Text = title;
            LoadingContent.Text = content;
            LoadingOverlay.IsVisible = true;
        }

        public void HideLoading()
        {
            LoadingOverlay.IsVisible = false;
        }

        private void MenuButtonOnClicked(object sender, EventArgs e)
        {
            _toggleMenu();
        }

        private void BackButtonOnClicked(object sender, EventArgs e)
        {
            ViewModelLocator.NavMgr.CurrentMainViewOnBackRequested();
        }

        protected override bool OnBackButtonPressed()
        {
            //nothing to go back to lets android close the app
            return ((NavMgr) ViewModelLocator.NavMgr).TryGoBack() || base.OnBackButtonPressed();
        }
    }
}
