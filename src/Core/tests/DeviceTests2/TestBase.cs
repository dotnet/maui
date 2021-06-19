using System;
using System.Threading.Tasks;
using Microsoft.Maui.Essentials;

namespace Microsoft.Maui.DeviceTests
{
	public partial class TestBase
	{
		public Task<T> InvokeOnMainThreadAsync<T>(Func<T> func) =>
#if WINDOWS
			MauiWinUIApplication.Current.MainWindow.DispatcherQueue.TryEnqueue();
#else
			MainThread.InvokeOnMainThreadAsync(func);
#endif;

		protected Task InvokeOnMainThreadAsync(Action action) =>
			MainThread.InvokeOnMainThreadAsync(action);

		protected Task InvokeOnMainThreadAsync(Func<Task> func) =>
			MainThread.InvokeOnMainThreadAsync(func);

		public Task<T> InvokeOnMainThreadAsync<T>(Func<Task<T>> func) =>
			MainThread.InvokeOnMainThreadAsync(func);
	}
}
