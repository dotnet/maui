using System;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace Xamarin.Essentials
{
    public static partial class Platform
    {
        public static void BeginInvokeOnMainThread(Action action)
        {
            var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;

            if (dispatcher != null)
                dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => action()).WatchForError();
            else
                action();
        }
    }
}
