using MALClient.Adapters;
using MALClient.Adapters.Common;
using MALClient.Adapters.Credentials;
using MALClient.Maui.Adapters;
using MALClient.Maui.Managers;
using MALClient.XShared.Interfaces;
using MALClient.XShared.Ioc;
using MALClient.XShared.ViewModels;
using MalHttpContextProvider = MALClient.Adapters.Common.MalHttpContextProvider;
using PasswordVault = MALClient.Maui.Adapters.PasswordVault;

namespace MALClient.Maui.ViewModels
{
    public static class MauiViewModelLocator
    {
        public static void RegisterDependencies()
        {
            ViewModelLocator.Mobile = true;
            ViewModelLocator.RegisterBase();

            SimpleIoc.Default.Register<MauiMainViewModel>();
            SimpleIoc.Default.Register<MauiHamburgerViewModel>();
            SimpleIoc.Default.Register<SettingsViewModel>();
            SimpleIoc.Default.Register<MainViewModelBase>(() => SimpleIoc.Default.GetInstance<MauiMainViewModel>());
            SimpleIoc.Default.Register<IHamburgerViewModel>(() => SimpleIoc.Default.GetInstance<MauiHamburgerViewModel>());
            SimpleIoc.Default.Register<SettingsViewModelBase>(() => SimpleIoc.Default.GetInstance<SettingsViewModel>());
            SimpleIoc.Default.Register<INavMgr, NavMgr>();

            SimpleIoc.Default.Register<IDataCache>(() => new FileDataCache(FileSystem.AppDataDirectory,
                FileSystem.AppDataDirectory, SimpleIoc.Default.GetInstance<IConnectionInfoProvider>()));
            SimpleIoc.Default.Register<IPasswordVault, PasswordVault>();
            SimpleIoc.Default.Register<IApplicationDataService, ApplicationDataService>();
            SimpleIoc.Default.Register<IClipboardProvider, ClipboardProvider>();
            SimpleIoc.Default.Register<ISystemControlsLauncherService, SystemControlsLauncherService>();
            SimpleIoc.Default.Register<IMessageDialogProvider, MessageDialogProvider>();
            SimpleIoc.Default.Register<IImageDownloaderService, ImageDownloaderService>();
            SimpleIoc.Default.Register<ITelemetryProvider, NullTelemetryProvider>();
            SimpleIoc.Default.Register<INotificationsTaskManager, NullNotificationsTaskManager>();
            SimpleIoc.Default.Register<ISchdeuledJobsManger, ScheduledJobsManager>();
            SimpleIoc.Default.Register<ICssManager, CssManager>();
            SimpleIoc.Default.Register<IChangeLogProvider, ChangeLogProvider>();
            SimpleIoc.Default.Register<IMalHttpContextProvider, MalHttpContextProvider>();
            SimpleIoc.Default.Register<ISnackbarProvider, SnackbarProvider>();
            SimpleIoc.Default.Register<IConnectionInfoProvider, ConnectionInfoProvider>();
            SimpleIoc.Default.Register<IDispatcherAdapter, DispatcherAdapter>();
            SimpleIoc.Default.Register<IAiringNotificationsAdapter, NullAiringNotificationsAdapter>();
            SimpleIoc.Default.Register<IDialogsProvider, DialogsProvider>();
            SimpleIoc.Default.Register<IShareProvider, ShareProvider>();
            SimpleIoc.Default.Register<ILiveTilesManager, NullLiveTilesManager>();
            SimpleIoc.Default.Register<ICalendarExportProvider, NullCalendarExportProvider>();
            SimpleIoc.Default.Register<IPinTileService, NullPinTileService>();
        }

        public static MauiMainViewModel Main => SimpleIoc.Default.GetInstance<MauiMainViewModel>();

        public static MauiHamburgerViewModel Hamburger => SimpleIoc.Default.GetInstance<MauiHamburgerViewModel>();
    }
}
