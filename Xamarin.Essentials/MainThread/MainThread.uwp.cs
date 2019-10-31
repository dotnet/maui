using System;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace Xamarin.Essentials
{
    public static partial class MainThread
    {
        static bool PlatformIsMainThread
        {
            get
            {
                // if there is no main window, then this is either a service
                // or the UI is not yet constructed, so the main thread is the
                // current thread
                if (CoreApplication.MainView?.CoreWindow == null)
                    return true;

                return CoreApplication.MainView.CoreWindow.Dispatcher?.HasThreadAccess ?? false;
            }
        }

        static void PlatformBeginInvokeOnMainThread(Action action)
        {
            var dispatcher = CoreApplication.MainView?.CoreWindow?.Dispatcher;

            if (dispatcher == null)
                throw new InvalidOperationException("Unable to find main thread.");
            dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => action()).WatchForError();
        }
    }
}
