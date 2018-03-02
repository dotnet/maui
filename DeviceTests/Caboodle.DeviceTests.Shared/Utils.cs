using System;
using System.Threading.Tasks;

namespace Caboodle.DeviceTests
{
    public class Utils
    {
#if WINDOWS_UWP
        public static async Task OnMainThread(Windows.UI.Core.DispatchedHandler action)
        {
            var mainView = Windows.ApplicationModel.Core.CoreApplication.MainView;
            var normal = Windows.UI.Core.CoreDispatcherPriority.Normal;
            await mainView.CoreWindow.Dispatcher.RunAsync(normal, action);
        }
#elif __ANDROID__
        public static Task OnMainThread(Action action)
        {
            var tcs = new TaskCompletionSource<bool>();
            var looper = Android.OS.Looper.MainLooper;
            var handler = new Android.OS.Handler(looper);
            handler.Post(() =>
            {
                action();

                tcs.SetResult(true);
            });
            return tcs.Task;
        }
#elif __IOS__
       public static Task OnMainThread(Action action)
        {
            var obj = new Foundation.NSObject();
            obj.InvokeOnMainThread(action);
            return Task.FromResult(true);
        }
#endif
    }
}
