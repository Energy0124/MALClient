using System;
using MALClient.Adapters;

namespace MALClient.UWP.Adapters
{
    public class DispatcherAdapter : IDispatcherAdapter
    {
        //WinUI 3 has no CoreDispatcher, the window's DispatcherQueue is the UI thread's queue
        public void Run(Action action)
        {
            MALClient.WinUI.App.MainWindow.DispatcherQueue.TryEnqueue(() => action());
        }
    }
}
