using System;
using System.Threading.Tasks;

namespace Microsoft.Maui.DeviceTests
{
	public partial class TestBase
	{
		protected Task<T> InvokeOnMainThreadAsync<T>(Func<T> func) =>
			TestMainThread.InvokeOnMainThreadAsync(func);

		protected Task InvokeOnMainThreadAsync(Action action) =>
			TestMainThread.InvokeOnMainThreadAsync(action);

		protected Task InvokeOnMainThreadAsync(Func<Task> action) =>
			TestMainThread.InvokeOnMainThreadAsync(action);

		public Task<T> InvokeOnMainThreadAsync<T>(Func<Task<T>> func) =>
			TestMainThread.InvokeOnMainThreadAsync(func);
	}
}
