using System;
using System.Threading.Tasks;

namespace Microsoft.Maui.DeviceTests
{
	public partial class TestBase
	{
		readonly Random rnd = new Random();

		protected Task<T> InvokeOnMainThreadAsync<T>(Func<T> func) =>
			TestMainThread.InvokeOnMainThreadAsync(func);

		protected Task InvokeOnMainThreadAsync(Action action) =>
			TestMainThread.InvokeOnMainThreadAsync(action);

		protected Task InvokeOnMainThreadAsync(Func<Task> action) =>
			TestMainThread.InvokeOnMainThreadAsync(action);

		protected Task<T> InvokeOnMainThreadAsync<T>(Func<Task<T>> func) =>
			TestMainThread.InvokeOnMainThreadAsync(func);

		protected async Task<bool> Wait(Func<bool> exitCondition, int timeout = 1000)
		{
			while ((timeout -= 100) > 0)
			{
				if (!exitCondition.Invoke())
					await Task.Delay(rnd.Next(100, 200));
				else
					break;
			}

			return exitCondition.Invoke();
		}
	}
}
