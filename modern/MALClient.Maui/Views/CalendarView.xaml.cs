using System.ComponentModel;
using MALClient.Models.Enums;
using MALClient.XShared.ViewModels;
using MALClient.XShared.ViewModels.Main;

namespace MALClient.Maui.Views
{
    public class CalendarDayGroup : List<AnimeItemViewModel>
    {
        public CalendarDayGroup(string day, IEnumerable<AnimeItemViewModel> items) : base(items)
        {
            Day = day;
        }

        public string Day { get; }
    }

    public partial class CalendarView : ContentView, INavigatedView
    {
        private static readonly TimeZoneInfo JstTimeZone =
            TimeZoneInfo.CreateCustomTimeZone("JST", TimeSpan.FromHours(9), "JST", "JST");

        public CalendarView()
        {
            InitializeComponent();
        }

        private CalendarPageViewModel ViewModel => ViewModelLocator.CalendarPage;

        private bool _active;

        public async void OnNavigatedTo(object args)
        {
            _active = true;
            //rebuilt every time, it only filters the local list and picks up list and settings changes
            await ViewModel.Init(true);
            if (!_active)
                return;
            //from here on only the refresh button rebuilds
            ViewModel.PropertyChanged += ViewModelOnPropertyChanged;
            ViewModel.PivotSelectedIndexChange += ViewModelOnPivotSelectedIndexChange;
            //coming back from details restores the day we left from
            if (args is int index && index < ViewModel.CalendarData.Count)
                ViewModel.CalendarPivotIndex = index;
            BuildTabs();
        }

        public void OnNavigatedFrom()
        {
            _active = false;
            ViewModel.PropertyChanged -= ViewModelOnPropertyChanged;
            ViewModel.PivotSelectedIndexChange -= ViewModelOnPivotSelectedIndexChange;
        }

        private void ViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //a forced refresh rebuilds every page
            if (e.PropertyName == nameof(CalendarPageViewModel.CalendarData))
                MainThread.BeginInvokeOnMainThread(BuildTabs);
        }

        private void ViewModelOnPivotSelectedIndexChange()
        {
            MainThread.BeginInvokeOnMainThread(ShowPage);
        }

        private static string Title(CalendarPivotPage page)
        {
            //the summary page hides Header and Sub with its own
            if (page is CalendarSummaryPivotPage summary)
                return summary.Header;
            return string.IsNullOrEmpty(page.Sub) ? page.Header : $"{page.Header} ({page.Sub})";
        }

        private void BuildTabs()
        {
            Tabs.Children.Clear();
            var pages = ViewModel.CalendarData;
            for (var i = 0; i < pages.Count; i++)
            {
                var index = i;
                var tab = new Button
                {
                    Text = Title(pages[i]),
                    Padding = new Thickness(12, 0),
                    CornerRadius = 16,
                    HeightRequest = 36,
                    AutomationId = $"CalendarTab{i}"
                };
                tab.Clicked += (sender, e) => ViewModel.CalendarPivotIndex = index;
                Tabs.Children.Add(tab);
            }

            foreach (var item in pages.SelectMany(page => page.Items))
                item.TimeTillNextAirCache = item.GetTimeTillNextAir(JstTimeZone);
            ShowPage();
        }

        private void ShowPage()
        {
            var pages = ViewModel.CalendarData;
            var index = ViewModel.CalendarPivotIndex;
            if (index < 0 || index >= pages.Count)
            {
                Items.ItemsSource = null;
                return;
            }

            for (var i = 0; i < Tabs.Children.Count; i++)
                if (Tabs.Children[i] is Button tab)
                {
                    var selected = i == index;
                    tab.BackgroundColor = selected
                        ? (Color) Application.Current.Resources["Primary"]
                        : Colors.Transparent;
                    tab.TextColor = selected
                        ? Colors.White
                        : (Color) Application.Current.Resources[
                            Application.Current.RequestedTheme == AppTheme.Dark ? "White" : "Black"];
                }

            var page = pages[index];
            Items.ItemsSource = page is CalendarSummaryPivotPage summary
                ? summary.Data.Select(day => new CalendarDayGroup(day.Item1, day.Item2)).ToList()
                : page.Items.Count == 0
                    ? new List<CalendarDayGroup>()
                    : new List<CalendarDayGroup> {new CalendarDayGroup(page.FullHeader, page.Items)};
            if (Tabs.Children.ElementAtOrDefault(index) is Element selectedTab)
                _ = TabsScroll.ScrollToAsync(selectedTab, ScrollToPosition.Center, true);
        }

        private void NextDayOnSwiped(object sender, SwipedEventArgs e)
        {
            if (ViewModel.CalendarPivotIndex < ViewModel.CalendarData.Count - 1)
                ViewModel.CalendarPivotIndex++;
        }

        private void PreviousDayOnSwiped(object sender, SwipedEventArgs e)
        {
            if (ViewModel.CalendarPivotIndex > 0)
                ViewModel.CalendarPivotIndex--;
        }

        private void ItemsOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is AnimeItemViewModel item)
            {
                Items.SelectedItem = null;
                item.NavigateDetails(PageIndex.PageCalendar, ViewModel.CalendarPivotIndex);
            }
        }
    }
}
