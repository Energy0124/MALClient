﻿using System;
using System.Xml.Linq;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using MALClient.Items;
using MALClient.ViewModels;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace MALClient.Pages
{
    public class AnimeDetailsPageNavigationArgs
    {
        public readonly XElement AnimeElement;
        public readonly IAnimeData AnimeItem;
        public readonly int Id;
        public readonly object PrevPageSetup;
        public readonly string Title;
        public PageIndex Source;
        public bool RegisterBackNav = true;

        public AnimeDetailsPageNavigationArgs(int id, string title, XElement element, IAnimeData animeReference,
            object args = null)
        {
            Id = id;
            Title = title;
            AnimeElement = element;
            PrevPageSetup = args;
            AnimeItem = animeReference;
        }
    }

    public sealed partial class AnimeDetailsPage : Page , IDetailsViewInteraction
    {
        private AnimeDetailsPageViewModel ViewModel => DataContext as AnimeDetailsPageViewModel;

        public AnimeDetailsPage()
        {
            this.InitializeComponent();
            ViewModel.View = this;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var param = e.Parameter as AnimeDetailsPageNavigationArgs;
            if (param == null)
                throw new Exception("No paramaters for this page");
            ViewModel.Init(param);           
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            DataContext = null;
            base.OnNavigatedFrom(e);
            NavMgr.DeregisterBackNav();
        }      

        private void SubmitWatchedEps(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
                ViewModel.ChangeWatchedEps();
        }

        public Flyout GetWatchedEpsFlyout()
        {
            return WatchedEpsFlyout;
        }

        private void Pivot_OnPivotItemLoading(Pivot sender, PivotItemEventArgs args)
        {
            switch (args.Item.Tag as string)
            {
                case "Details":
                    ViewModel.LoadDetails();
                    break;
                case "Reviews":
                    ViewModel.LoadReviews();
                    break;
                case "Recomm":
                    ViewModel.LoadRecommendations();
                    break;
                case "Related":
                    ViewModel.LoadRelatedAnime();
                    break;
            }            
        }

        private void ButtonNavDetails_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.NavigateDetails();
        }
    }
}