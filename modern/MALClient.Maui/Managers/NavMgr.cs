using System.Windows.Input;
using MALClient.Models.Enums;
using MALClient.XShared.Interfaces;
using MALClient.XShared.NavArgs;
using MALClient.XShared.ViewModels;

namespace MALClient.Maui.Managers
{
    /// <summary>
    ///     Back navigation manager, ported from the Xamarin app: a single stack of main pages plus one-time overrides
    ///     pages use for their own inner navigation.
    /// </summary>
    public class NavMgr : INavMgr
    {
        private ICommand _currentOverride;
        private ICommand _enqueuedCommand;

        private readonly Stack<Tuple<PageIndex, object>> _randomNavigationStackMain =
            new Stack<Tuple<PageIndex, object>>(30);

        /// <summary>
        ///     Returns false when there's nothing to go back to, so the system can close the app.
        /// </summary>
        public bool TryGoBack()
        {
            if (_currentOverride == null && _randomNavigationStackMain.Count == 0)
                return false;

            CurrentViewOnBackRequested();
            return true;
        }

        private void CurrentViewOnBackRequested()
        {
            if (_currentOverride != null)
            {
                var over = _currentOverride;
                _currentOverride.Execute(null);
                if (_currentOverride == over)
                    _currentOverride = null;
                if (_randomNavigationStackMain.Count == 0)
                    ViewModelLocator.GeneralMain.NavigateMainBackButtonVisibility = false;
                return;
            }

            if (_randomNavigationStackMain.Count == 0)
                return;

            var data = _randomNavigationStackMain.Pop();
            ViewModelLocator.GeneralMain.Navigate(data.Item1, data.Item2);
            if (_randomNavigationStackMain.Count == 0)
                ViewModelLocator.GeneralMain.NavigateMainBackButtonVisibility = false;
        }

        public void RegisterBackNav(AnimeDetailsPageNavigationArgs args)
        {
            RegisterBackNav(PageIndex.PageAnimeDetails, args);
        }

        public bool HasSomethingOnStack()
        {
            return _randomNavigationStackMain.Any();
        }

        public void RegisterBackNav(ProfilePageNavigationArgs args)
        {
            RegisterBackNav(PageIndex.PageProfile, args);
        }

        public void DeregisterBackNav()
        {
            _randomNavigationStackMain.Clear();
        }

        public void ResetOffBackNav()
        {
            _randomNavigationStackMain.Clear();
        }

        public void CurrentOffViewOnBackRequested()
        {
            //single pane, there's no off view
        }

        public void ResetMainBackNav()
        {
            _randomNavigationStackMain.Clear();
            _currentOverride = null;
            ViewModelLocator.GeneralMain.NavigateMainBackButtonVisibility = false;
        }

        public void RegisterBackNav(PageIndex page, object args, PageIndex source = PageIndex.PageAbout)
        {
            _randomNavigationStackMain.Push(new Tuple<PageIndex, object>(page, args));
            ViewModelLocator.GeneralMain.NavigateMainBackButtonVisibility = true;
        }

        public void RegisterOneTimeOverride(ICommand command)
        {
            _currentOverride = command;
            ViewModelLocator.GeneralMain.NavigateOffBackButtonVisibility = true;
        }

        public void ResetOneTimeOverride()
        {
            _currentOverride = _enqueuedCommand;
            _enqueuedCommand = null;
            if (_randomNavigationStackMain.Count == 0)
                ViewModelLocator.GeneralMain.NavigateMainBackButtonVisibility = true;
        }

        public void RegisterOneTimeMainOverride(ICommand command)
        {
            _currentOverride = command;
            ViewModelLocator.GeneralMain.NavigateMainBackButtonVisibility = true;
        }

        public void ResetOneTimeMainOverride()
        {
            _currentOverride = _enqueuedCommand;
            _enqueuedCommand = null;
            if (_randomNavigationStackMain.Count == 0)
                ViewModelLocator.GeneralMain.NavigateMainBackButtonVisibility = true;
        }

        public void CurrentMainViewOnBackRequested()
        {
            CurrentViewOnBackRequested();
        }

        public void RegisterUnmonitoredMainBackNav(PageIndex page, object args)
        {
            RegisterBackNav(page, args);
        }
    }
}
