using MALClient.Maui.Pages;
using MALClient.Maui.ViewModels;
using MALClient.XShared.BL;
using MALClient.XShared.Utils;
using MALClient.XShared.ViewModels;

namespace MALClient.Maui;

public partial class App : Application
{
	public App()
	{
		MauiViewModelLocator.RegisterDependencies();
		ResourceLocator.ConnectionInfoProvider.Init();
		InitializeComponent();
		UserAppTheme = Settings.SelectedTheme == 1 ? AppTheme.Dark : AppTheme.Light;
	}

	protected override Window CreateWindow(IActivationState activationState)
	{
		var window = new Window(new RootPage());
		window.Created += async (sender, args) =>
		{
			await InitializationRoutines.InitApp();
			MauiViewModelLocator.Main.PerformFirstNavigation();
		};
		return window;
	}
}
