using System;
using System.Threading.Tasks;

namespace Microsoft.Maui.MauiBlazorWebView.DeviceTests
{
	public static partial class WebViewHelpers
	{
		const int MaxWaitTimes = 30;
		const int WaitTimeInMS = 1000;

		private static async Task Retry(Func<Task<bool>> tryAction, Func<int, Task<Exception>> createExceptionWithTimeoutMS)
		{
			for (var i = 0; i < MaxWaitTimes; i++)
			{
				if (await tryAction())
				{
					return;
				}
				await Task.Delay(WaitTimeInMS);
			}

			throw await createExceptionWithTimeoutMS(MaxWaitTimes * WaitTimeInMS);
		}
	}
}
