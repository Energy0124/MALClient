using MALClient.Adapters;
using MALClient.Maui.Pages;

namespace MALClient.Maui.Adapters
{
    public class MessageDialogProvider : IMessageDialogProvider
    {
        public void ShowMessageDialog(string content, string title)
        {
            MainThread.BeginInvokeOnMainThread(async () => await ShowMessageDialogAsync(content, title));
        }

        public Task ShowMessageDialogAsync(string content, string title)
        {
            return MainThread.InvokeOnMainThreadAsync(() => HostPage.Current.DisplayAlertAsync(title, content, "OK"));
        }

        public void ShowMessageDialogWithInput(string content, string title, string trueCommand, string falseCommand,
            Action callbackOnTrue, Action callBackOnFalse = null)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (await HostPage.Current.DisplayAlertAsync(title, content, trueCommand, falseCommand))
                    callbackOnTrue.Invoke();
                else
                    callBackOnFalse?.Invoke();
            });
        }

        public void ShowLoadingPopup(string title, string content)
        {
            MainThread.BeginInvokeOnMainThread(() => HostPage.Current.ShowLoading(title, content));
        }

        public void HideLoadingDialog()
        {
            MainThread.BeginInvokeOnMainThread(() => HostPage.Current.HideLoading());
        }

        public void UpdateLoadingPopup(string title, string content)
        {
            MainThread.BeginInvokeOnMainThread(() => HostPage.Current.ShowLoading(title, content));
        }
    }
}
