using System;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace DeviceTests
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

        public static Task OnMainThread(Func<Task> action)
        {
            var tcs = new TaskCompletionSource<bool>();
            var mainView = Windows.ApplicationModel.Core.CoreApplication.MainView;
            var normal = Windows.UI.Core.CoreDispatcherPriority.Normal;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            mainView.CoreWindow.Dispatcher.RunAsync(normal, async () =>
            {
                try
                {
                    await action();
                    tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            return tcs.Task;
        }
#elif __ANDROID__
        public static Task OnMainThread(Action action)
        {
            var tcs = new TaskCompletionSource<bool>();
            var looper = Android.OS.Looper.MainLooper;
            var handler = new Android.OS.Handler(looper);
            handler.Post(() =>
            {
                try
                {
                    action();
                    tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });
            return tcs.Task;
        }

        public static Task OnMainThread(Func<Task> action)
        {
            var tcs = new TaskCompletionSource<bool>();
            var looper = Android.OS.Looper.MainLooper;
            var handler = new Android.OS.Handler(looper);
            handler.Post(async () =>
            {
                try
                {
                    await action();
                    tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
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

        public static Task OnMainThread(Func<Task> action)
        {
            var tcs = new TaskCompletionSource<bool>();
            var obj = new Foundation.NSObject();
            obj.InvokeOnMainThread(async () =>
            {
                try
                {
                    await action();
                    tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });
            return tcs.Task;
        }
#endif
    }
}
