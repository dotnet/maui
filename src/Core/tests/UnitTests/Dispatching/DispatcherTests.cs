using System;
using System.Threading.Tasks;
using Microsoft.Maui.Dispatching;
using Xunit;

namespace Microsoft.Maui.UnitTests.Dispatching
{
	// All these tests should run in a separate thread/task to avoid polluting the other tests.
	// This is only because the dispatcher and dispatcher provider are both "static" classes.
	[Category(TestCategory.Core, TestCategory.Dispatching)]
	public class DispatcherTests : IDisposable
	{
		DispatcherProviderStub _dispatcherProvider;

		public DispatcherTests()
		{
			_dispatcherProvider = new DispatcherProviderStub();
			DispatcherProvider.SetCurrent(_dispatcherProvider);
		}

		public void Dispose()
		{
			DispatcherProvider.SetCurrent(null);
			_dispatcherProvider.Dispose();
		}

		// just a check to make sure the test dispatcher is working
		[Fact]
		public Task TestImplementationHasDispatcher() =>
			DispatcherTest.Run(() =>
			{
				Assert.False(DispatcherProviderStubOptions.SkipDispatcherCreation);

				var dispatcher = Dispatcher.GetForCurrentThread();

				Assert.NotNull(dispatcher);
				Assert.False(dispatcher.IsDispatchRequired);
			});

		[Fact]
		public Task BackgroundThreadDoesNotHaveDispatcher() =>
			DispatcherTest.Run(() =>
			{
				// act like the real world
				DispatcherProviderStubOptions.SkipDispatcherCreation = true;

				var dispatcher = Dispatcher.GetForCurrentThread();

				Assert.Null(dispatcher);
			});

		[Fact]
		public Task BackgroundThreadDoesNotGetDispatcherFromMainThread() =>
			DispatcherTest.Run(async () =>
			{
				var dispatcher = Dispatcher.GetForCurrentThread();
				Assert.NotNull(dispatcher);

				await Task.Run(() =>
				{
					DispatcherProviderStubOptions.SkipDispatcherCreation = true;

					var dispatcher = Dispatcher.GetForCurrentThread();
					Assert.Null(dispatcher);
				});
			});

		[Fact]
		public Task SameDispatcherForSameThread() =>
			DispatcherTest.Run(() =>
			{
				var dispatcher1 = Dispatcher.GetForCurrentThread();
				var dispatcher2 = Dispatcher.GetForCurrentThread();

				Assert.Same(dispatcher1, dispatcher2);
			});

		[Fact]
		public Task DifferentDispatcherForDifferentThread() =>
			DispatcherTest.Run(async () =>
			{
				var outerId = Environment.CurrentManagedThreadId;
				var dispatcher1 = Dispatcher.GetForCurrentThread();

				await Task.Run(() =>
				{
					var innerId = Environment.CurrentManagedThreadId;
					var dispatcher2 = Dispatcher.GetForCurrentThread();

					Assert.NotEqual(outerId, innerId);
					Assert.NotSame(dispatcher1, dispatcher2);
				});
			});
	}
}