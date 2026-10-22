using System.Windows.Input;
using MALClient.Models.Enums;
using MALClient.XShared.Delegates;
using MALClient.XShared.Mvvm;
using MALClient.XShared.ViewModels;

namespace MALClient.Maui.ViewModels
{
    public class SettingsViewModel : SettingsViewModelBase
    {
        private ICommand _requestNavigationCommand;

        public override event SettingsNavigationRequest NavigationRequest;

        public override ICommand ReviewCommand => new RelayCommand(() =>
            ResourceLocator.SystemControlsLauncherService.LaunchUri(
                new Uri($"market://details?id={AppInfo.Current.PackageName}")));

        public override ICommand RequestNavigationCommand
            => _requestNavigationCommand ??= new RelayCommand<SettingsPageIndex>(page =>
            {
                if (page == SettingsPageIndex.LogIn)
                {
                    ViewModelLocator.GeneralMain.Navigate(PageIndex.PageLogIn);
                    return;
                }

                NavigationRequest?.Invoke(page);
            });

        public override void LoadCachedEntries()
        {
        }
    }
}
