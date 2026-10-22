using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using MALClient.Models.Enums;
using MALClient.UWP.Adapters;
using MALClient.UWP.Shared.UserControls;
using MALClient.UWP.Shared.ViewModels;
using MALClient.UWP.Shared.ViewModels.Interfaces;
using MALClient.UWP.ViewModels;
using MALClient.XShared.Utils;
using MALClient.XShared.ViewModels;
using Windows.UI;
using Microsoft.UI.Xaml.Media;
using MALClient.XShared.Ioc;
using MALClient.Adapters;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MALClient.UWP
{
	/// <summary>
	///     An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainPage : Page, IMainViewInteractions
	{
		private double _prevOffContntWidth;
		private Timer _timer;
		private bool _paneState;// _currentPaneState;

		public MainPage()
		{
			InitializeComponent();

			Loaded += (sender, args) =>
			{
				var vm = DesktopViewModelLocator.Main;
				vm.MainNavigationRequested += Navigate;
				vm.OffNavigationRequested += NavigateOff;
				vm.PropertyChanged += VmOnPropertyChanged;
				vm.MediaElementCollapsed += VmOnMediaElementCollapsed;
				DesktopViewModelLocator.Main.View = this;
				ViewModelLocator.GeneralMain.ChangelogVisibility = ResourceLocator.ChangelogProvider.NewChangelog;
            };
		}

	    private void VmOnMediaElementCollapsed()
		{
			MediaElement.MediaPlayer?.Pause();
		}


		private void VmOnPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			if (args.PropertyName == "MenuPaneState")
			{
				var paneState = (sender as MainViewModel).MenuPaneState;
				_paneState = paneState;

				HamburgerControl.Width = _paneState ? 250 : 48;
				HamburgerTopGrid.Width = _paneState ? 250 : 48;
			}
			else if (args.PropertyName == "OffContentVisibility")
			{
				SplitterColumn.Width = new GridLength(ViewModelLocator.GeneralMain.OffContentVisibility ? 16 : 0);
			}
			else if (args.PropertyName == nameof(ViewModelLocator.GeneralMain.AdsContainerVisibility))
			{
				//if (ViewModelLocator.GeneralMain.AdsContainerVisibility)
				//	AdControl.Resume();
				//else
				//	AdControl.Suspend();
			}

		}

		private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (!_paneState)
				return;

			if (e.NewSize.Width > 720)
			{
				HamburgerControl.Width = 250;
				HamburgerTopGrid.Width = 250;
				//_currentPaneState = true;
			}
			else
			{
				HamburgerControl.Width = 48;
				HamburgerTopGrid.Width = 48;
				//_currentPaneState = false;
			}
		}

		public Tuple<int, string> InitDetails { get; private set; }

		private void Navigate(Type page, object args = null)
		{
			MainContent.Navigate(page, args);
		}

		private void NavigateOff(Type page, object args = null)
		{
			OffContent.Navigate(page, args);
		}

		public SplitViewDisplayMode CurrentDisplayMode { get; }

		public void SearchInputFocus(FocusState state)
		{
			SearchInput.Focus(state);
		}

		public void InitSplitter()
		{
			try
			{
				RootContentGrid.ColumnDefinitions[2].Width =
					new GridLength(_prevOffContntWidth == 0
						? (_prevOffContntWidth = GetStartingSplitterWidth())
						: _prevOffContntWidth);
				OffContent.UpdateLayout();
			}
			catch (Exception)
			{
				//magic
			}

		}

		public Storyboard CurrentStatusStoryboard => FadeInCurrentStatus;
		public Storyboard CurrentOffStatusStoryboard => FadeInCurrentOffStatus;
		public Storyboard CurrentOffSubStatusStoryboard => FadeInCurrentSubStatus;

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			InitDetails = e.Parameter as Tuple<int, string>;
		}

		private double GetStartingSplitterWidth()
		{
			return MALClient.WinUI.App.MainWindow.Bounds.Width > 1400.0 ? 535 : 465;
		}

		#region Search

		private void SearchInput_OnQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
		{
			if (SearchInput.Text.Length >= 2)
			{
				SearchInput.IsEnabled = false; //reset input
				SearchInput.IsEnabled = true;
				ViewModelLocator.GeneralMain.OnSearchInputSubmit();
			}
		}

		#endregion

		private void CustomGridSplitter_OnDraggingCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
		{
			if (DesktopViewModelLocator.Main.CurrentMainPageKind == PageIndex.PageAnimeList && RootContentGrid.ColumnDefinitions[2].ActualWidth < _prevOffContntWidth &&
				RootContentGrid.ColumnDefinitions[2].ActualWidth - _prevOffContntWidth < -50)
			{
				var vm = ViewModelLocator.AnimeList;
				if (vm.AreThereItemsWaitingForLoad)
					vm.RefreshList();
			}
			if (RootContentGrid.ColumnDefinitions[2].ActualWidth == 0)
				DesktopViewModelLocator.Main.HideOffContentCommand.Execute(null);

			_prevOffContntWidth = RootContentGrid.ColumnDefinitions[2].ActualWidth;
		}

		private void OffContent_OnSizeChanged(object sender, SizeChangedEventArgs e)
		{
			(DataContext as MainViewModel).OffContentStatusBarWidth = e.NewSize.Width;
		}

		private void OffContent_OnPointerPressed(object sender, PointerRoutedEventArgs e)
		{
			if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
			{
				var properties = e.GetCurrentPoint(this).Properties;
				if (properties.IsXButton1Pressed)
					ViewModelLocator.NavMgr.CurrentOffViewOnBackRequested();
			}
		}

		private void MainContent_OnPointerPressed(object sender, PointerRoutedEventArgs e)
		{
			if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
			{
				var properties = e.GetCurrentPoint(this).Properties;
				if (properties.IsXButton1Pressed)
					ViewModelLocator.NavMgr.CurrentMainViewOnBackRequested();
			}
		}

		private void ToggleButton_OnClick(object sender, RoutedEventArgs e)
		{
			var btn = sender as LockableToggleButton;
			if (btn.IsChecked.GetValueOrDefault(false))
				SearchInput.Focus(FocusState.Keyboard);
		}

		private void ButtonCloseChangelogOnClick(object sender, RoutedEventArgs e)
		{
			ViewModelLocator.GeneralMain.ChangelogVisibility = false;
		}

	}
}