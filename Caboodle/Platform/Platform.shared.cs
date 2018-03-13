using System;
using System.Threading.Tasks;

namespace Microsoft.Caboodle
{
    public static partial class Platform
    {
        internal static Task InvokeOnMainThread(Action action)
        {
            var tcs = new TaskCompletionSource<object>();

            BeginInvokeOnMainThread(() =>
            {
                try
                {
                    action();
                    tcs.TrySetResult(null);
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
