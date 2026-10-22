using WinRT.Interop;

namespace MALClient.WinUI.Compat
{
    public static class WindowInterop
    {
        /// <summary>
        ///     UWP dialogs and pickers (MessageDialog, FileSavePicker...) found their CoreWindow on their own,
        ///     desktop apps have to hand them the owning window first.
        /// </summary>
        public static T WithMainWindow<T>(this T target)
        {
            InitializeWithWindow.Initialize(target, WindowNative.GetWindowHandle(App.MainWindow));
            return target;
        }
    }
}
