//using System;
//using System.Threading.Tasks;
//using Microsoft.Maui.Dispatching;
//using Microsoft.Maui.TestUtils.DeviceTests.Runners;

//namespace Microsoft.Maui.DeviceTests
//{
//	public partial class TestBase
//	{
//		readonly Random rnd = new Random();

//		protected Task<T> InvokeOnMainThreadAsync<T>(Func<T> func) =>
//			TestDispatcher.Current.DispatchAsync(func);

//		protected Task InvokeOnMainThreadAsync(Action action) =>
//			TestDispatcher.Current.DispatchAsync(action);

//		protected Task InvokeOnMainThreadAsync(Func<Task> action) =>
//			TestDispatcher.Current.DispatchAsync(action);

//		protected Task<T> InvokeOnMainThreadAsync<T>(Func<Task<T>> func) =>
//			TestDispatcher.Current.DispatchAsync(func);

//		protected Task<bool> Wait(Func<bool> exitCondition, int timeout = 1000) =>
//			AssertionExtensions.Wait(exitCondition, timeout);
//	}
//}
