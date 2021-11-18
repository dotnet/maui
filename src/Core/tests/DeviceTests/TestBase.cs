using System;
using System.Threading.Tasks;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.TestUtils.DeviceTests.Runners;

namespace Microsoft.Maui.DeviceTests
{
	public partial class TestBase
	{
		protected Task<T> InvokeOnMainThreadAsync<T>(Func<T> func) =>
			TestDispatcher.Current.InvokeOnMainThreadAsync(func);

		protected Task InvokeOnMainThreadAsync(Action action) =>
			TestDispatcher.Current.InvokeOnMainThreadAsync(action);

		protected Task InvokeOnMainThreadAsync(Func<Task> action) =>
			TestDispatcher.Current.InvokeOnMainThreadAsync(action);

		public Task<T> InvokeOnMainThreadAsync<T>(Func<Task<T>> func) =>
			TestDispatcher.Current.InvokeOnMainThreadAsync(func);
	}
}
