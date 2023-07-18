using System;
using System.Threading.Tasks;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.TestUtils.DeviceTests.Runners;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	//// Uncomment these sections if you hit issues with parallel executions
	//[CollectionDefinition("Non-Parallel Collection", DisableParallelization = true)]
	//public class NonParallelCollectionDefinitionClass
	//{
	//}

	[Collection("Non-Parallel Collection")]
	public partial class TestBase
	{
		public const int EmCoefficientPrecision = 4;

		protected static Task<T> InvokeOnMainThreadAsync<T>(Func<T> func) =>
			TestDispatcher.Current.DispatchAsync(func);

		protected static Task InvokeOnMainThreadAsync(Action action) =>
			TestDispatcher.Current.DispatchAsync(action);

		protected static Task InvokeOnMainThreadAsync(Func<Task> action) =>
			TestDispatcher.Current.DispatchAsync(action);

		protected static Task<T> InvokeOnMainThreadAsync<T>(Func<Task<T>> func) =>
			TestDispatcher.Current.DispatchAsync(func);

		protected static async Task WaitForGC()
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();
			await Task.Delay(10);
			GC.Collect();
			GC.WaitForPendingFinalizers();
		}
	}
}
