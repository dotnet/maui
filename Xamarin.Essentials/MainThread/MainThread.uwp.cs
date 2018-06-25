using System;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace Xamarin.Essentials
{
    public static partial class MainThread
    {
        static bool PlatformIsMainThread =>
            CoreApplication.MainView.CoreWindow.Dispatcher == null;

        static void PlatformBeginInvokeOnMainThread(Action action)
        {
            var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;

            if (dispatcher != null)
                dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => action()).WatchForError();
            else
                action();
        }
    }
}
