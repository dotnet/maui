using System;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace Xamarin.Essentials
{
    public static partial class Platform
    {
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
