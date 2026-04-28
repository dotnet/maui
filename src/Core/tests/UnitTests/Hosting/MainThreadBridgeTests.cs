#nullable enable

using System;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.UnitTests.Hosting
{
	[Category(TestCategory.Core, TestCategory.Hosting)]
	public class MainThreadBridgeTests : IDisposable
	{
		public MainThreadBridgeTests()
		{
			// Ensure clean state before each test
			MainThread.ClearCustomImplementation();
		}

		public void Dispose()
		{
			MainThread.ClearCustomImplementation();
			DispatcherProvider.SetCurrent(null);
		}

		[Fact]
		public void WithoutCustomImpl_IsMainThread_Throws()
		{
			// On netstandard with no backing implementation, IsMainThread should throw
			Assert.Throws<NotImplementedInReferenceAssemblyException>(
				() => _ = MainThread.IsMainThread);
		}

		[Fact]
		public void WithoutCustomImpl_BeginInvoke_Throws()
		{
			// On netstandard with no backing implementation, BeginInvokeOnMainThread should throw
			Assert.Throws<NotImplementedInReferenceAssemblyException>(
				() => MainThread.BeginInvokeOnMainThread(() => { }));
		}

		[Fact]
		public void WithCustomImpl_IsMainThread_UsesBackingImpl()
		{
			MainThread.SetCustomImplementation(
				isMainThread: () => true,
				beginInvokeOnMainThread: _ => { });

			Assert.True(MainThread.IsMainThread);
		}

		[Fact]
		public void WithCustomImpl_IsMainThread_ReturnsFalse()
		{
			MainThread.SetCustomImplementation(
				isMainThread: () => false,
				beginInvokeOnMainThread: _ => { });

			Assert.False(MainThread.IsMainThread);
		}

		[Fact]
		public void WithCustomImpl_BeginInvoke_CallsBackingImpl()
		{
			var invoked = false;
			Action? capturedAction = null;

			MainThread.SetCustomImplementation(
				isMainThread: () => false,
				beginInvokeOnMainThread: action => capturedAction = action);

			MainThread.BeginInvokeOnMainThread(() => invoked = true);

			// The custom impl captured the action; execute it
			Assert.NotNull(capturedAction);
			capturedAction!();
			Assert.True(invoked);
		}

		[Fact]
		public void BeginInvoke_WhenOnMainThread_InvokesDirectly()
		{
			var invoked = false;

			MainThread.SetCustomImplementation(
				isMainThread: () => true,
				beginInvokeOnMainThread: _ => throw new InvalidOperationException("Should not be called"));

			// When IsMainThread returns true, the shared code invokes the action directly
			MainThread.BeginInvokeOnMainThread(() => invoked = true);

			Assert.True(invoked);
		}

		[Fact]
		public void ClearCustomImpl_RestoresThrowBehavior()
		{
			MainThread.SetCustomImplementation(
				isMainThread: () => true,
				beginInvokeOnMainThread: _ => { });

			Assert.True(MainThread.IsMainThread);

			MainThread.ClearCustomImplementation();

			Assert.Throws<NotImplementedInReferenceAssemblyException>(
				() => _ = MainThread.IsMainThread);
		}

		[Fact]
		public void SetCustomImpl_NullIsMainThread_Throws()
		{
			Assert.Throws<ArgumentNullException>(
				() => MainThread.SetCustomImplementation(null!, _ => { }));
		}

		[Fact]
		public void SetCustomImpl_NullBeginInvoke_Throws()
		{
			Assert.Throws<ArgumentNullException>(
				() => MainThread.SetCustomImplementation(() => true, null!));
		}

		[Fact]
		public void MauiAppBuild_BridgesDispatcherToMainThread()
		{
			// Set up a dispatcher provider that returns a real dispatcher stub
			var dispatcherStub = new DispatcherStub(
				isInvokeRequired: () => false,
				invokeOnMainThread: null);

			var dispatcherProvider = new TestDispatcherProvider(dispatcherStub);
			DispatcherProvider.SetCurrent(dispatcherProvider);

			try
			{
				var builder = MauiApp.CreateBuilder();
				using var app = builder.Build();

				// After MauiApp.Build(), the bridge should have connected MainThread
				// to the dispatcher. IsMainThread should return true (since IsDispatchRequired is false).
				Assert.True(MainThread.IsMainThread);
			}
			finally
			{
				DispatcherProvider.SetCurrent(null);
			}
		}

		[Fact]
		public void MauiAppBuild_BeginInvoke_DispatchesToDispatcher()
		{
			var dispatched = false;
			var dispatcherStub = new DispatcherStub(
				isInvokeRequired: () => true,
				invokeOnMainThread: action => { dispatched = true; action(); });

			var dispatcherProvider = new TestDispatcherProvider(dispatcherStub);
			DispatcherProvider.SetCurrent(dispatcherProvider);

			try
			{
				var builder = MauiApp.CreateBuilder();
				using var app = builder.Build();

				var actionExecuted = false;
				MainThread.BeginInvokeOnMainThread(() => actionExecuted = true);

				Assert.True(dispatched);
				Assert.True(actionExecuted);
			}
			finally
			{
				DispatcherProvider.SetCurrent(null);
			}
		}

		[Fact]
		public async Task InvokeOnMainThreadAsync_Action_WorksThroughBridge()
		{
			var dispatched = false;
			var dispatcherStub = new DispatcherStub(
				isInvokeRequired: () => true,
				invokeOnMainThread: action => { dispatched = true; action(); });

			var dispatcherProvider = new TestDispatcherProvider(dispatcherStub);
			DispatcherProvider.SetCurrent(dispatcherProvider);

			try
			{
				var builder = MauiApp.CreateBuilder();
				using var app = builder.Build();

				var actionExecuted = false;
				await MainThread.InvokeOnMainThreadAsync(() => actionExecuted = true);

				Assert.True(dispatched);
				Assert.True(actionExecuted);
			}
			finally
			{
				DispatcherProvider.SetCurrent(null);
			}
		}

		[Fact]
		public void MauiAppBuild_BridgeUsesApplicationDispatcher()
		{
			var dispatcherStub = new DispatcherStub(
				isInvokeRequired: () => false,
				invokeOnMainThread: action => action());

			var dispatcherProvider = new TestDispatcherProvider(dispatcherStub);
			DispatcherProvider.SetCurrent(dispatcherProvider);

			try
			{
				var builder = MauiApp.CreateBuilder();
				using var app = builder.Build();

				// Verify the bridge connected the dispatcher's IsDispatchRequired to MainThread.IsMainThread
				Assert.True(MainThread.IsMainThread);
			}
			finally
			{
				DispatcherProvider.SetCurrent(null);
			}
		}

		class TestDispatcherProvider : IDispatcherProvider
		{
			readonly IDispatcher _dispatcher;

			public TestDispatcherProvider(IDispatcher dispatcher)
			{
				_dispatcher = dispatcher;
			}

			public IDispatcher? GetForCurrentThread() => _dispatcher;
		}
	}
}
