using System;
using System.Threading.Tasks;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	// Run all these tests on a new thread/task in order to control the dispatcher creation.

	public class DeviceUnitTests : BaseTestFixture
	{
		// just a check to make sure the test dispatcher is working
		[Fact]
		public Task TestImplementationHasDispatcher() => DispatcherTest.Run(() =>
		{
			Assert.False(DispatcherProviderStubOptions.SkipDispatcherCreation);
			Assert.False(Device.IsInvokeRequired);

			// can create things
			var button = new Button();
		});

		[Fact]
		public Task BackgroundThreadDoesNotHaveDispatcher() => DispatcherTest.Run(() =>
		{
			// act like the real world
			DispatcherProviderStubOptions.SkipDispatcherCreation = true;

			// can create things
			var button = new Button();
		});

		[Fact]
		public Task TestBeginInvokeOnMainThread() => DispatcherTest.Run(() =>
		{
			bool calledFromMainThread = false;
			MockPlatformServices(() => calledFromMainThread = true);

			bool invoked = false;
			Device.BeginInvokeOnMainThread(() => invoked = true);

			Assert.True(invoked, "Action not invoked.");
			Assert.True(calledFromMainThread, "Action not invoked from main thread.");
		});

		[Fact]
		public Task TestInvokeOnMainThreadWithSyncFunc() => DispatcherTest.Run(async () =>
		{
			bool calledFromMainThread = false;
			MockPlatformServices(() => calledFromMainThread = true);

			bool invoked = false;
			var result = await Device.InvokeOnMainThreadAsync(() => { invoked = true; return true; });

			Assert.True(invoked, "Action not invoked.");
			Assert.True(calledFromMainThread, "Action not invoked from main thread.");
			Assert.True(result, "Unexpected result.");
		});

		[Fact]
		public Task TestInvokeOnMainThreadWithSyncAction() => DispatcherTest.Run(async () =>
		{
			bool calledFromMainThread = false;
			MockPlatformServices(() => calledFromMainThread = true);

			bool invoked = false;
			await Device.InvokeOnMainThreadAsync(() => { invoked = true; });

			Assert.True(invoked, "Action not invoked.");
			Assert.True(calledFromMainThread, "Action not invoked from main thread.");
		});

		[Fact]
		public Task TestInvokeOnMainThreadWithAsyncFunc() => DispatcherTest.Run(async () =>
		{
			bool calledFromMainThread = false;
			MockPlatformServices(
				() => calledFromMainThread = true,
				invokeOnMainThread: action => Task.Delay(50).ContinueWith(_ => action()));

			bool invoked = false;
			var task = Device.InvokeOnMainThreadAsync(async () => { invoked = true; return true; });
			Assert.True(calledFromMainThread, "Action not invoked from main thread.");
			Assert.False(invoked, "Action invoked early.");

			var result = await task;
			Assert.True(invoked, "Action not invoked.");
			Assert.True(result, "Unexpected result.");
		});

		[Fact]
		public Task TestInvokeOnMainThreadWithAsyncFuncError() => DispatcherTest.Run(async () =>
		{
			bool calledFromMainThread = false;
			MockPlatformServices(
				() => calledFromMainThread = true,
				invokeOnMainThread: action => Task.Delay(50).ContinueWith(_ => action()));

			bool invoked = false;
			async Task<bool> boom()
			{
				invoked = true;
				throw new ApplicationException();
			}
			var task = Device.InvokeOnMainThreadAsync(boom);
			Assert.True(calledFromMainThread, "Action not invoked from main thread.");
			Assert.False(invoked, "Action invoked early.");

			async Task MethodThatThrows() => await task;
			await Assert.ThrowsAsync<ApplicationException>(MethodThatThrows);
		});

		[Fact]
		public Task TestInvokeOnMainThreadWithAsyncAction() => DispatcherTest.Run(async () =>
		{
			bool calledFromMainThread = false;
			MockPlatformServices(
				() => calledFromMainThread = true,
				invokeOnMainThread: action => Task.Delay(50).ContinueWith(_ => action()));

			bool invoked = false;
			var task = Device.InvokeOnMainThreadAsync(async () => { invoked = true; });
			Assert.True(calledFromMainThread, "Action not invoked from main thread.");
			Assert.False(invoked, "Action invoked early.");

			await task;
			Assert.True(invoked, "Action not invoked.");
		});

		[Fact]
		public Task TestInvokeOnMainThreadWithAsyncActionError() => DispatcherTest.Run(async () =>
		{
			bool calledFromMainThread = false;
			MockPlatformServices(
				() => calledFromMainThread = true,
				invokeOnMainThread: action => Task.Delay(50).ContinueWith(_ => action()));

			bool invoked = false;
			async Task boom()
			{ invoked = true; throw new ApplicationException(); }
			var task = Device.InvokeOnMainThreadAsync(boom);
			Assert.True(calledFromMainThread, "Action not invoked from main thread.");
			Assert.False(invoked, "Action invoked early.");

			async Task MethodThatThrows() => await task;
			await Assert.ThrowsAsync<ApplicationException>(MethodThatThrows);
		});

		private static void MockPlatformServices(Action onInvokeOnMainThread, Action<Action> invokeOnMainThread = null)
		{
			DispatcherProviderStubOptions.InvokeOnMainThread =
				action =>
				{
					onInvokeOnMainThread();

					if (invokeOnMainThread == null)
						action();
					else
						invokeOnMainThread(action);
				};
		}
	}
}
