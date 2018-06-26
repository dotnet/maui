using System;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace Xamarin.Essentials
{
    public static partial class MainThread
    {
        static bool PlatformIsMainThread =>
            CoreApplication.MainView.CoreWindow?.Dispatcher?.HasThreadAccess ?? false;

        static void PlatformBeginInvokeOnMainThread(Action action)
        {
            var dispatcher = CoreApplication.MainView.CoreWindow?.Dispatcher;

            if (dispatcher == null)
                throw new InvalidOperationException("Unable to find main thread.");

            if (!dispatcher.HasThreadAccess)
                dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => action()).WatchForError();
            else
                action();
        }
    }
}
