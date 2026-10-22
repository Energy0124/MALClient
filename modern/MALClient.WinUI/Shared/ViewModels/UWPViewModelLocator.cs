using MALClient.XShared.Ioc;
using MALClient.Adapters;
using MALClient.Adapters.Common;
using MALClient.XShared.ViewModels;

namespace MALClient.UWP.Shared.ViewModels
{
    public class UWPViewModelLocator
    {
        public static void RegisterDependencies()
        {
            SimpleIoc.Default.Register<SettingsViewModelBase,SettingsViewModel>();
            SimpleIoc.Default.Register<ISnackbarProvider, SnackbarProvider>();
            //live tiles aren't supported by WinUI 3 apps
            SimpleIoc.Default.Register<IPinTileService, NullPinTileService>();
        }

        class SnackbarProvider : ISnackbarProvider
        {
            public void ShowText(string text)
            {

            }
        }
    }
}
