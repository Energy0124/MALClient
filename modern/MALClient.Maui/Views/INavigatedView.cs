namespace MALClient.Maui.Views
{
    /// <summary>
    ///     Content views hosted by <see cref="Pages.HostPage" />, the MAUI counterpart of the Xamarin fragments.
    /// </summary>
    public interface INavigatedView
    {
        void OnNavigatedTo(object args);

        void OnNavigatedFrom();
    }
}
