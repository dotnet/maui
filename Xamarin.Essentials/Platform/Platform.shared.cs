using System;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class Platform
    {
        public static void BeginInvokeOnMainThread(Action action)
            => PlatformBeginInvokeOnMainThread(action);

        internal static Task InvokeOnMainThread(Action action)
        {
            var tcs = new TaskCompletionSource<bool>();

            BeginInvokeOnMainThread(() =>
            {
                try
                {
                    action();
                    tcs.TrySetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });

            return tcs.Task;
        }

        internal static Task<T> InvokeOnMainThread<T>(Func<T> action)
        {
            var tcs = new TaskCompletionSource<T>();

            BeginInvokeOnMainThread(() =>
            {
                try
                {
                    var result = action();
                    tcs.TrySetResult(result);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });

            return tcs.Task;
        }
    }
}
