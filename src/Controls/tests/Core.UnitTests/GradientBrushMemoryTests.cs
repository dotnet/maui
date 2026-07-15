using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class GradientBrushMemoryTests : BaseTestFixture
	{
		[MethodImpl(MethodImplOptions.NoInlining)]
		static WeakReference CreateBrushWithSharedGradientStops(GradientStopCollection sharedStops, object bindingContext = null)
		{
			var brush = new LinearGradientBrush { GradientStops = sharedStops };
			if (bindingContext is not null)
				brush.BindingContext = bindingContext;

			return new WeakReference(brush);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static (WeakReference Brush, WeakReference BindingContext) CreateBrushWithNewBindingContext(
			GradientStopCollection sharedStops)
		{
			var bindingContext = new GradientStop { Offset = 0.25f };
			var brush = new LinearGradientBrush
			{
				BindingContext = bindingContext,
				GradientStops = sharedStops
			};

			return (new WeakReference(brush), new WeakReference(bindingContext));
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static WeakReference CreateBrushWithHandlerAndSharedGradientStops(
			GradientStopCollection sharedStops,
			object bindingContext,
			IMauiContext mauiContext)
		{
			var handler = new ElementHandlerStub();
			handler.SetMauiContext(mauiContext);
			var brush = new LinearGradientBrush
			{
				BindingContext = bindingContext,
				Handler = handler,
				GradientStops = sharedStops
			};

			return new WeakReference(brush);
		}

		static IMauiContext CreateMauiContext(IDispatcher dispatcher)
		{
			var services = Substitute.For<IServiceProvider>();
			services.GetService(typeof(IDispatcher)).Returns(dispatcher);
			var mauiContext = Substitute.For<IMauiContext>();
			mauiContext.Services.Returns(services);
			return mauiContext;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static WeakReference CreateFinalizerBlocker(
			ManualResetEventSlim finalizerEntered,
			ManualResetEventSlim releaseFinalizer)
		{
			var blocker = new FinalizerBlocker(finalizerEntered, releaseFinalizer);
			return new WeakReference(blocker);
		}

		[Fact]
		public async Task GradientBrushDoesNotLeakWhenSharingGradientStops()
		{
			// A long-lived/shared GradientStopCollection, exactly as the issue describes.
			var sharedStops = new GradientStopCollection
			{
				new GradientStop()
			};
			var weakBrush = CreateBrushWithSharedGradientStops(sharedStops);

			Assert.False(await weakBrush.WaitForCollect(), "LinearGradientBrush should not be alive!");
			GC.KeepAlive(sharedStops);
		}

		[Fact]
		public async Task CollectedBrushDoesNotLeaveStaleGradientStopParent()
		{
			ApplicationExtensions.CreateAndSetMockApplication();
			try
			{
				var stop = new GradientStop();
				var sharedStops = new GradientStopCollection { stop };
				var weakBrush = CreateBrushWithSharedGradientStops(sharedStops);

				Assert.False(await weakBrush.WaitForCollect(), "LinearGradientBrush should not be alive!");
				Assert.Null(stop.Parent);
				Assert.DoesNotContain(MockApplication.MockLogger.Messages,
					message => message.Contains("RealParent", StringComparison.Ordinal));
				GC.KeepAlive(sharedStops);
			}
			finally
			{
				Application.ClearCurrent();
			}
		}

		[Fact]
		public async Task CollectedBrushClearsGradientStopInheritedBindingContext()
		{
			var bindingContext = new GradientStop { Offset = 0.25f };
			var stop = new GradientStop();
			stop.SetBinding(GradientStop.OffsetProperty, nameof(GradientStop.Offset));
			var sharedStops = new GradientStopCollection { stop };
			var weakBrush = CreateBrushWithSharedGradientStops(sharedStops, bindingContext);

			Assert.Same(bindingContext, stop.BindingContext);
			Assert.Equal(0.25f, stop.Offset);

			bindingContext.Offset = 0.5f;
			Assert.Equal(0.5f, stop.Offset);

			Assert.False(await weakBrush.WaitForCollect(), "LinearGradientBrush should not be alive!");
			Assert.Null(stop.Parent);
			Assert.Null(stop.BindingContext);
			Assert.Equal(0f, stop.Offset);

			bindingContext.Offset = 0.75f;
			Assert.Equal(0f, stop.Offset);
			GC.KeepAlive(bindingContext);
			GC.KeepAlive(sharedStops);
		}

		[Fact]
		public async Task CollectedBrushClearsGradientStopBindingContextPropertyBindingSource()
		{
			var inheritedContext = new GradientStop { Offset = 0.25f };
			var stop = new GradientStop();
			stop.SetBinding(BindableObject.BindingContextProperty, nameof(GradientStop.Offset));
			var sharedStops = new GradientStopCollection { stop };
			var weakBrush = CreateBrushWithSharedGradientStops(sharedStops, inheritedContext);
			int bindingContextChanged = 0;
			stop.BindingContextChanged += (_, _) => bindingContextChanged++;

			Assert.Equal(0.25f, stop.BindingContext);
			Assert.False(await weakBrush.WaitForCollect(), "LinearGradientBrush should not be alive!");
			Assert.Null(stop.Parent);
			Assert.Null(stop.BindingContext);
			Assert.Equal(1, bindingContextChanged);

			inheritedContext.Offset = 0.5f;
			Assert.Null(stop.BindingContext);
			GC.KeepAlive(inheritedContext);
			GC.KeepAlive(sharedStops);
		}

		[Fact]
		public async Task CollectedBrushWithoutDispatcherReleasesBindingContextPropertyBindingSource()
		{
			GradientStop stop;
			GradientStopCollection sharedStops;
			WeakReference weakBrush;
			WeakReference weakBindingContext;
			DispatcherProviderStubOptions.SkipDispatcherCreation = true;
			try
			{
				stop = new GradientStop();
				stop.SetBinding(BindableObject.BindingContextProperty, nameof(GradientStop.Offset));
				sharedStops = new GradientStopCollection { stop };
				(weakBrush, weakBindingContext) = CreateBrushWithNewBindingContext(sharedStops);
			}
			finally
			{
				DispatcherProviderStubOptions.SkipDispatcherCreation = false;
			}

			int bindingContextChanged = 0;
			stop.BindingContextChanged += (_, _) => bindingContextChanged++;

			Assert.False(await weakBrush.WaitForCollect(), "LinearGradientBrush should not be alive!");
			Assert.False(await weakBindingContext.WaitForCollect(), "Inherited binding context should not be alive!");
			Assert.Equal(0, bindingContextChanged);
			Assert.Equal(0.25f, stop.GetValue(BindableObject.BindingContextProperty));

			Assert.Null(stop.BindingContext);
			Assert.Equal(1, bindingContextChanged);
			GC.KeepAlive(sharedStops);
		}

		[Fact]
		public async Task CollectedBrushWithoutDispatcherReleasesMultiBindingContextPropertyBindingSource()
		{
			GradientStop stop;
			GradientStopCollection sharedStops;
			WeakReference weakBrush;
			WeakReference weakBindingContext;
			DispatcherProviderStubOptions.SkipDispatcherCreation = true;
			try
			{
				stop = new GradientStop();
				stop.SetBinding(BindableObject.BindingContextProperty, new MultiBinding
				{
					StringFormat = "{0}",
					Bindings =
					{
						new Binding(nameof(GradientStop.Offset))
					}
				});
				sharedStops = new GradientStopCollection { stop };
				(weakBrush, weakBindingContext) = CreateBrushWithNewBindingContext(sharedStops);
			}
			finally
			{
				DispatcherProviderStubOptions.SkipDispatcherCreation = false;
			}

			int bindingContextChanged = 0;
			stop.BindingContextChanged += (_, _) => bindingContextChanged++;

			Assert.False(await weakBrush.WaitForCollect(), "LinearGradientBrush should not be alive!");
			Assert.False(await weakBindingContext.WaitForCollect(), "Inherited binding context should not be alive!");
			Assert.Equal(0, bindingContextChanged);

			Assert.Equal(string.Empty, stop.BindingContext);
			Assert.Equal(1, bindingContextChanged);
			GC.KeepAlive(sharedStops);
		}

		[Fact]
		public async Task CollectedBrushDispatchesGradientStopInheritedBindingContextCleanup()
		{
			var dispatchedCleanup = new TaskCompletionSource<Action>(TaskCreationOptions.RunContinuationsAsynchronously);
			DispatcherProviderStubOptions.IsInvokeRequired = () => true;
			DispatcherProviderStubOptions.InvokeOnMainThread = action => dispatchedCleanup.TrySetResult(action);

			GradientStop bindingContext;
			GradientStop stop;
			GradientStopCollection sharedStops;
			WeakReference weakBrush;
			try
			{
				bindingContext = new GradientStop { Offset = 0.25f };
				stop = new GradientStop();
				stop.SetBinding(GradientStop.OffsetProperty, nameof(GradientStop.Offset));
				sharedStops = new GradientStopCollection { stop };
				weakBrush = CreateBrushWithSharedGradientStops(sharedStops, bindingContext);
			}
			finally
			{
				DispatcherProviderStubOptions.IsInvokeRequired = null;
				DispatcherProviderStubOptions.InvokeOnMainThread = null;
			}

			int bindingContextChanged = 0;
			stop.BindingContextChanged += (_, _) => bindingContextChanged++;

			Assert.False(await weakBrush.WaitForCollect(), "LinearGradientBrush should not be alive!");
			var cleanup = await dispatchedCleanup.Task.WaitAsync(TimeSpan.FromSeconds(5));

			Assert.Equal(0, bindingContextChanged);
			Assert.Equal(0.25f, stop.Offset);

			cleanup();

			Assert.Equal(1, bindingContextChanged);
			Assert.Null(stop.BindingContext);
			Assert.Equal(0f, stop.Offset);
			GC.KeepAlive(bindingContext);
			GC.KeepAlive(sharedStops);
		}

		[Fact]
		public async Task CollectedBrushUsesDispatcherAttachedAfterGradientStopCreation()
		{
			var dispatchedCleanup = new TaskCompletionSource<Action>(TaskCreationOptions.RunContinuationsAsynchronously);
			var dispatcher = new DispatcherStub(
				() => true,
				action => dispatchedCleanup.TrySetResult(action));
			var mauiContext = CreateMauiContext(dispatcher);
			var handler = Substitute.For<IElementHandler>();
			handler.MauiContext.Returns(mauiContext);

			GradientStop bindingContext;
			GradientStop stop;
			GradientStopCollection sharedStops;
			WeakReference weakBrush;
			DispatcherProviderStubOptions.SkipDispatcherCreation = true;
			try
			{
				bindingContext = new GradientStop { Offset = 0.25f };
				stop = new GradientStop();
				stop.SetBinding(GradientStop.OffsetProperty, nameof(GradientStop.Offset));
				sharedStops = new GradientStopCollection { stop };
				weakBrush = CreateBrushWithSharedGradientStops(sharedStops, bindingContext);
			}
			finally
			{
				DispatcherProviderStubOptions.SkipDispatcherCreation = false;
			}

			stop.Handler = handler;
			int bindingContextChanged = 0;
			stop.BindingContextChanged += (_, _) => bindingContextChanged++;

			Assert.False(await weakBrush.WaitForCollect(), "LinearGradientBrush should not be alive!");
			var cleanup = await dispatchedCleanup.Task.WaitAsync(TimeSpan.FromSeconds(5));

			Assert.Equal(0, bindingContextChanged);
			Assert.Equal(0.25f, stop.Offset);

			cleanup();

			Assert.Equal(1, bindingContextChanged);
			Assert.Null(stop.BindingContext);
			Assert.Equal(0f, stop.Offset);
			GC.KeepAlive(bindingContext);
			GC.KeepAlive(sharedStops);
		}

		[Fact]
		public async Task CollectedBrushUsesDispatcherAvailableWhenParentIsAssigned()
		{
			var dispatchedCleanup = new TaskCompletionSource<Action>(TaskCreationOptions.RunContinuationsAsynchronously);
			var dispatcher = new DispatcherStub(
				() => true,
				action => dispatchedCleanup.TrySetResult(action));
			var mauiContext = CreateMauiContext(dispatcher);

			GradientStop bindingContext;
			GradientStop stop;
			GradientStopCollection sharedStops;
			WeakReference weakBrush;
			DispatcherProviderStubOptions.SkipDispatcherCreation = true;
			try
			{
				bindingContext = new GradientStop { Offset = 0.25f };
				stop = new GradientStop();
				stop.SetBinding(GradientStop.OffsetProperty, nameof(GradientStop.Offset));
				sharedStops = new GradientStopCollection { stop };
				weakBrush = CreateBrushWithHandlerAndSharedGradientStops(
					sharedStops,
					bindingContext,
					mauiContext);
			}
			finally
			{
				DispatcherProviderStubOptions.SkipDispatcherCreation = false;
			}

			int bindingContextChanged = 0;
			stop.BindingContextChanged += (_, _) => bindingContextChanged++;

			Assert.False(await weakBrush.WaitForCollect(), "LinearGradientBrush should not be alive!");
			var cleanup = await dispatchedCleanup.Task.WaitAsync(TimeSpan.FromSeconds(5));

			Assert.Equal(0, bindingContextChanged);
			Assert.Equal(0.25f, stop.Offset);

			cleanup();

			Assert.Equal(1, bindingContextChanged);
			Assert.Null(stop.BindingContext);
			Assert.Equal(0f, stop.Offset);
			GC.KeepAlive(bindingContext);
			GC.KeepAlive(sharedStops);
		}

		[Fact]
		public async Task BindingContextAccessBeforeDispatchedCleanupDoesNotRunCallbacks()
		{
			var dispatchedCleanup = new TaskCompletionSource<Action>(TaskCreationOptions.RunContinuationsAsynchronously);
			DispatcherProviderStubOptions.IsInvokeRequired = () => true;
			DispatcherProviderStubOptions.InvokeOnMainThread = action => dispatchedCleanup.TrySetResult(action);

			GradientStop bindingContext;
			GradientStop stop;
			GradientStopCollection sharedStops;
			WeakReference weakBrush;
			try
			{
				bindingContext = new GradientStop { Offset = 0.25f };
				stop = new GradientStop();
				stop.SetBinding(GradientStop.OffsetProperty, nameof(GradientStop.Offset));
				sharedStops = new GradientStopCollection { stop };
				weakBrush = CreateBrushWithSharedGradientStops(sharedStops, bindingContext);
			}
			finally
			{
				DispatcherProviderStubOptions.IsInvokeRequired = null;
				DispatcherProviderStubOptions.InvokeOnMainThread = null;
			}

			int bindingContextChanged = 0;
			stop.BindingContextChanged += (_, _) => Interlocked.Increment(ref bindingContextChanged);

			Assert.False(await weakBrush.WaitForCollect(), "LinearGradientBrush should not be alive!");
			var cleanup = await dispatchedCleanup.Task.WaitAsync(TimeSpan.FromSeconds(5));

			Assert.Null(await Task.Run(() => stop.BindingContext));
			int callbacksBeforeCleanup = Volatile.Read(ref bindingContextChanged);

			cleanup();

			Assert.Equal(0, callbacksBeforeCleanup);
			Assert.Equal(1, Volatile.Read(ref bindingContextChanged));
			Assert.Null(stop.BindingContext);
			Assert.Equal(0f, stop.Offset);
			GC.KeepAlive(bindingContext);
			GC.KeepAlive(sharedStops);
		}

		[Fact]
		public async Task ParentAccessAfterCollectedBrushClearsGradientStopInheritedBindingContext()
		{
			var bindingContext = new GradientStop { Offset = 0.25f };
			var stop = new GradientStop();
			stop.SetBinding(GradientStop.OffsetProperty, nameof(GradientStop.Offset));
			var sharedStops = new GradientStopCollection { stop };
			var weakBrush = CreateBrushWithSharedGradientStops(sharedStops, bindingContext);
			int bindingContextChanged = 0;
			stop.BindingContextChanged += (_, _) => bindingContextChanged++;

			Assert.False(await weakBrush.WaitForCollect(), "LinearGradientBrush should not be alive!");
			Assert.Equal(0, bindingContextChanged);
			Assert.Equal(0.25f, stop.Offset);

			Assert.Null(stop.Parent);

			Assert.Equal(1, bindingContextChanged);
			Assert.Equal(0f, stop.Offset);

			bindingContext.Offset = 0.75f;
			Assert.Equal(0f, stop.Offset);
			Assert.Null(stop.BindingContext);
			GC.KeepAlive(bindingContext);
			GC.KeepAlive(sharedStops);
		}

		[Fact]
		public async Task FailedDispatchLeavesGradientStopInheritedBindingContextCleanupPending()
		{
			bool dispatchRequired = true;
			DispatcherProviderStubOptions.IsInvokeRequired = () => dispatchRequired;
			DispatcherProviderStubOptions.DispatchResult = () => false;

			GradientStop bindingContext;
			GradientStop stop;
			GradientStopCollection sharedStops;
			WeakReference weakBrush;
			try
			{
				bindingContext = new GradientStop { Offset = 0.25f };
				stop = new GradientStop();
				stop.SetBinding(GradientStop.OffsetProperty, nameof(GradientStop.Offset));
				sharedStops = new GradientStopCollection { stop };
				weakBrush = CreateBrushWithSharedGradientStops(sharedStops, bindingContext);
			}
			finally
			{
				DispatcherProviderStubOptions.IsInvokeRequired = null;
				DispatcherProviderStubOptions.DispatchResult = null;
			}

			int bindingContextChanged = 0;
			stop.BindingContextChanged += (_, _) => bindingContextChanged++;

			Assert.False(await weakBrush.WaitForCollect(), "LinearGradientBrush should not be alive!");
			Assert.Equal(0, bindingContextChanged);
			Assert.Equal(0.25f, stop.Offset);

			Assert.Null(stop.Parent);

			Assert.Equal(0, bindingContextChanged);
			Assert.Equal(0.25f, stop.Offset);

			dispatchRequired = false;
			Assert.Null(stop.Parent);

			Assert.Equal(1, bindingContextChanged);
			Assert.Equal(0f, stop.Offset);

			bindingContext.Offset = 0.75f;
			Assert.Equal(0f, stop.Offset);
			Assert.Null(stop.BindingContext);
			GC.KeepAlive(bindingContext);
			GC.KeepAlive(sharedStops);
		}

		[Fact]
		public void ParentAccessBeforeSubscriptionFinalizerClearsInheritedBindingContext()
		{
			using var finalizerEntered = new ManualResetEventSlim();
			using var releaseFinalizer = new ManualResetEventSlim();
			CreateFinalizerBlocker(finalizerEntered, releaseFinalizer);
			GC.Collect();

			Assert.True(finalizerEntered.Wait(TimeSpan.FromSeconds(5)), "Finalizer blocker did not start.");

			var bindingContext = new GradientStop { Offset = 0.25f };
			var stop = new GradientStop();
			stop.SetBinding(GradientStop.OffsetProperty, nameof(GradientStop.Offset));
			var sharedStops = new GradientStopCollection { stop };
			var weakBrush = CreateBrushWithSharedGradientStops(sharedStops, bindingContext);

			try
			{
				GC.Collect();

				Assert.False(weakBrush.IsAlive, "LinearGradientBrush should not be alive!");
				Assert.Null(stop.Parent);
				Assert.Null(stop.BindingContext);
				Assert.Equal(0f, stop.Offset);

				bindingContext.Offset = 0.75f;
				Assert.Equal(0f, stop.Offset);
			}
			finally
			{
				releaseFinalizer.Set();
				GC.WaitForPendingFinalizers();
			}

			GC.KeepAlive(bindingContext);
			GC.KeepAlive(sharedStops);
		}

		[Fact]
		public async Task CollectingPreviousBrushPreservesCurrentGradientStopParent()
		{
			var stop = new GradientStop();
			var firstStops = new GradientStopCollection { stop };
			var weakFirstBrush = CreateBrushWithSharedGradientStops(firstStops);
			var secondBindingContext = new object();
			var secondBrush = new LinearGradientBrush
			{
				BindingContext = secondBindingContext,
				GradientStops = new GradientStopCollection { stop }
			};

			Assert.Same(secondBrush, stop.Parent);
			Assert.False(await weakFirstBrush.WaitForCollect(), "Previous LinearGradientBrush should not be alive!");
			Assert.Same(secondBrush, stop.Parent);
			Assert.Same(secondBindingContext, stop.BindingContext);
			GC.KeepAlive(firstStops);
			GC.KeepAlive(secondBrush);
		}

		[Fact]
		public async Task CollectingPreviousBrushPreservesCurrentGradientStopBindingContextPropertyBindingSource()
		{
			var stop = new GradientStop();
			stop.SetBinding(BindableObject.BindingContextProperty, nameof(GradientStop.Offset));
			var firstBindingContext = new GradientStop { Offset = 0.25f };
			var firstStops = new GradientStopCollection { stop };
			var weakFirstBrush = CreateBrushWithSharedGradientStops(firstStops, firstBindingContext);
			var secondBindingContext = new GradientStop { Offset = 0.5f };
			var secondBrush = new LinearGradientBrush
			{
				BindingContext = secondBindingContext,
				GradientStops = new GradientStopCollection { stop }
			};

			Assert.Same(secondBrush, stop.Parent);
			Assert.Equal(0.5f, stop.BindingContext);
			Assert.False(await weakFirstBrush.WaitForCollect(), "Previous LinearGradientBrush should not be alive!");
			Assert.Same(secondBrush, stop.Parent);
			Assert.Equal(0.5f, stop.BindingContext);
			GC.KeepAlive(firstStops);
			GC.KeepAlive(firstBindingContext);
			GC.KeepAlive(secondBrush);
			GC.KeepAlive(secondBindingContext);
		}

		[Fact]
		public async Task ClearingManualBindingContextAfterCollectedBrushDoesNotRestoreStaleBindingValue()
		{
			var inheritedContext = new GradientStop { Offset = 0.25f };
			var stop = new GradientStop();
			stop.SetBinding(BindableObject.BindingContextProperty, nameof(GradientStop.Offset));
			var sharedStops = new GradientStopCollection { stop };
			var weakBrush = CreateBrushWithSharedGradientStops(sharedStops, inheritedContext);
			var manualBindingContext = stop.BindingContext;
			stop.BindingContext = manualBindingContext;
			int bindingContextChanged = 0;
			stop.BindingContextChanged += (_, _) => bindingContextChanged++;

			Assert.False(await weakBrush.WaitForCollect(), "LinearGradientBrush should not be alive!");
			Assert.Null(stop.Parent);
			Assert.Same(manualBindingContext, stop.BindingContext);
			Assert.Equal(0, bindingContextChanged);

			stop.ClearValue(BindableObject.BindingContextProperty);

			Assert.Null(stop.BindingContext);
			Assert.Equal(1, bindingContextChanged);
			GC.KeepAlive(inheritedContext);
			GC.KeepAlive(sharedStops);
		}

		[Fact]
		public async Task ReplacingBindingContextBindingWhileCleanupIsPendingUsesNullInheritedSource()
		{
			var dispatchedCleanups = new ConcurrentQueue<Action>();
			DispatcherProviderStubOptions.IsInvokeRequired = () => true;
			DispatcherProviderStubOptions.InvokeOnMainThread = dispatchedCleanups.Enqueue;

			GradientStop inheritedContext;
			GradientStop stop;
			GradientStopCollection sharedStops;
			WeakReference weakBrush;
			try
			{
				inheritedContext = new GradientStop { Offset = 0.25f };
				stop = new GradientStop();
				stop.SetBinding(BindableObject.BindingContextProperty, nameof(GradientStop.Offset));
				sharedStops = new GradientStopCollection { stop };
				weakBrush = CreateBrushWithSharedGradientStops(sharedStops, inheritedContext);
			}
			finally
			{
				DispatcherProviderStubOptions.IsInvokeRequired = null;
				DispatcherProviderStubOptions.InvokeOnMainThread = null;
			}

			int bindingContextChanged = 0;
			stop.BindingContextChanged += (_, _) => bindingContextChanged++;

			Assert.False(await weakBrush.WaitForCollect(), "LinearGradientBrush should not be alive!");
			Assert.True(dispatchedCleanups.TryDequeue(out var cleanup));

			stop.SetBinding(BindableObject.BindingContextProperty, nameof(GradientStop.Offset));

			Assert.Null(stop.GetValue(BindableObject.BindingContextProperty));
			Assert.Equal(1, bindingContextChanged);

			cleanup();

			Assert.Null(stop.GetValue(BindableObject.BindingContextProperty));
			Assert.Equal(1, bindingContextChanged);
			GC.KeepAlive(inheritedContext);
			GC.KeepAlive(sharedStops);
		}

		[Fact]
		public async Task RemovingBindingContextBindingWhileCleanupIsPendingClearsStoredBindingValue()
		{
			var dispatchedCleanups = new ConcurrentQueue<Action>();
			DispatcherProviderStubOptions.IsInvokeRequired = () => true;
			DispatcherProviderStubOptions.InvokeOnMainThread = dispatchedCleanups.Enqueue;

			GradientStop inheritedContext;
			GradientStop stop;
			GradientStopCollection sharedStops;
			WeakReference weakBrush;
			try
			{
				inheritedContext = new GradientStop { Offset = 0.25f };
				stop = new GradientStop();
				stop.SetBinding(BindableObject.BindingContextProperty, nameof(GradientStop.Offset));
				sharedStops = new GradientStopCollection { stop };
				weakBrush = CreateBrushWithSharedGradientStops(sharedStops, inheritedContext);
			}
			finally
			{
				DispatcherProviderStubOptions.IsInvokeRequired = null;
				DispatcherProviderStubOptions.InvokeOnMainThread = null;
			}

			var manualBindingContext = stop.BindingContext;
			stop.BindingContext = manualBindingContext;
			int bindingContextChanged = 0;
			stop.BindingContextChanged += (_, _) => bindingContextChanged++;

			Assert.False(await weakBrush.WaitForCollect(), "LinearGradientBrush should not be alive!");
			Assert.True(dispatchedCleanups.TryDequeue(out var cleanup));

			stop.RemoveBinding(BindableObject.BindingContextProperty);

			Assert.Same(manualBindingContext, stop.BindingContext);
			Assert.Equal(0, bindingContextChanged);

			cleanup();
			stop.ClearValue(BindableObject.BindingContextProperty);

			Assert.Null(stop.BindingContext);
			Assert.Equal(1, bindingContextChanged);
			GC.KeepAlive(inheritedContext);
			GC.KeepAlive(sharedStops);
		}

		[Fact]
		public async Task StaleDispatchedCleanupDoesNotClearNewPendingBindingContextCleanup()
		{
			var dispatchedCleanups = new ConcurrentQueue<Action>();
			DispatcherProviderStubOptions.IsInvokeRequired = () => true;
			DispatcherProviderStubOptions.InvokeOnMainThread = dispatchedCleanups.Enqueue;

			GradientStop stop;
			GradientStop firstBindingContext;
			GradientStopCollection firstStops;
			WeakReference weakFirstBrush;
			try
			{
				stop = new GradientStop();
				stop.SetBinding(BindableObject.BindingContextProperty, nameof(GradientStop.Offset));
				firstBindingContext = new GradientStop { Offset = 0.25f };
				firstStops = new GradientStopCollection { stop };
				weakFirstBrush = CreateBrushWithSharedGradientStops(firstStops, firstBindingContext);
			}
			finally
			{
				DispatcherProviderStubOptions.IsInvokeRequired = null;
				DispatcherProviderStubOptions.InvokeOnMainThread = null;
			}

			int bindingContextChanged = 0;
			stop.BindingContextChanged += (_, _) => bindingContextChanged++;

			Assert.False(await weakFirstBrush.WaitForCollect(), "First LinearGradientBrush should not be alive!");
			Assert.True(dispatchedCleanups.TryDequeue(out var firstCleanup));

			var secondBindingContext = new GradientStop { Offset = 0.5f };
			var secondStops = new GradientStopCollection { stop };
			var weakSecondBrush = CreateBrushWithSharedGradientStops(secondStops, secondBindingContext);
			while (dispatchedCleanups.TryDequeue(out var duplicateFirstCleanup))
				duplicateFirstCleanup();
			int changesAfterReparenting = bindingContextChanged;

			Assert.False(await weakSecondBrush.WaitForCollect(), "Second LinearGradientBrush should not be alive!");
			Assert.True(dispatchedCleanups.TryDequeue(out var secondCleanup));

			firstCleanup();

			Assert.Equal(changesAfterReparenting, bindingContextChanged);
			Assert.Equal(0.5f, stop.GetValue(BindableObject.BindingContextProperty));

			secondCleanup();

			Assert.Equal(changesAfterReparenting + 1, bindingContextChanged);
			Assert.Null(stop.GetValue(BindableObject.BindingContextProperty));
			GC.KeepAlive(firstStops);
			GC.KeepAlive(firstBindingContext);
			GC.KeepAlive(secondStops);
			GC.KeepAlive(secondBindingContext);
		}

		[Fact]
		public async Task GradientStopChangesStillInvalidateAfterGc()
		{
			var stop = new GradientStop();
			var brush = new LinearGradientBrush
			{
				GradientStops = new GradientStopCollection { stop }
			};
			bool invalidated = false;
			brush.InvalidateGradientBrushRequested += (_, __) => invalidated = true;

			await TestHelpers.Collect();

			stop.Offset = 0.5f;

			Assert.True(invalidated);
			GC.KeepAlive(brush);
		}

		[Fact]
		public async Task SharedGradientStopsInvalidateEachLiveBrushAfterGc()
		{
			var stop = new GradientStop();
			var sharedStops = new GradientStopCollection { stop };
			var firstBrush = new LinearGradientBrush { GradientStops = sharedStops };
			var secondBrush = new LinearGradientBrush { GradientStops = sharedStops };
			int firstInvalidationCount = 0;
			int secondInvalidationCount = 0;
			firstBrush.InvalidateGradientBrushRequested += (_, __) => firstInvalidationCount++;
			secondBrush.InvalidateGradientBrushRequested += (_, __) => secondInvalidationCount++;

			await TestHelpers.Collect();

			stop.Offset = 0.5f;

			Assert.Equal(1, firstInvalidationCount);
			Assert.Equal(1, secondInvalidationCount);
			GC.KeepAlive(firstBrush);
			GC.KeepAlive(secondBrush);
		}

		[Fact]
		public async Task AliveBrushStillInvalidatesAfterSiblingBrushIsCollected()
		{
			var stop = new GradientStop();
			var sharedStops = new GradientStopCollection { stop };
			var weakCollectedBrush = CreateBrushWithSharedGradientStops(sharedStops);
			var aliveBrush = new LinearGradientBrush { GradientStops = sharedStops };
			int invalidationCount = 0;
			aliveBrush.InvalidateGradientBrushRequested += (_, __) => invalidationCount++;

			Assert.False(await weakCollectedBrush.WaitForCollect(), "Sibling LinearGradientBrush should not be alive!");

			stop.Offset = 0.5f;

			Assert.Equal(1, invalidationCount);
			GC.KeepAlive(aliveBrush);
			GC.KeepAlive(sharedStops);
		}

		[Fact]
		public void RemovingAndReplacingGradientStopsMovesSubscriptions()
		{
			var removedStop = new GradientStop { Offset = 0.1f };
			var retainedStop = new GradientStop { Offset = 0.2f };
			var oldStops = new GradientStopCollection { removedStop, retainedStop };
			var brush = new LinearGradientBrush { GradientStops = oldStops };
			int invalidationCount = 0;
			brush.InvalidateGradientBrushRequested += (_, __) => invalidationCount++;

			oldStops.Remove(removedStop);
			invalidationCount = 0;

			removedStop.Offset = 0.3f;
			Assert.Equal(0, invalidationCount);

			retainedStop.Offset = 0.4f;
			Assert.Equal(1, invalidationCount);

			var replacementStop = new GradientStop { Offset = 0.1f };
			brush.GradientStops = new GradientStopCollection { replacementStop };
			invalidationCount = 0;

			retainedStop.Offset = 0.5f;
			oldStops.Add(new GradientStop());

			Assert.Equal(0, invalidationCount);

			replacementStop.Offset = 0.6f;

			Assert.Equal(1, invalidationCount);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void ReplacingGradientStopsInvalidatesBrush(bool replaceWithNull)
		{
			var brush = new LinearGradientBrush
			{
				GradientStops = new GradientStopCollection { new GradientStop() }
			};
			int invalidationCount = 0;
			brush.InvalidateGradientBrushRequested += (_, __) => invalidationCount++;

			brush.GradientStops = replaceWithNull ? null : new GradientStopCollection();

			Assert.Equal(1, invalidationCount);
		}

		[Fact]
		public void NullGradientStopCollectionAllowsBindingContextChange()
		{
			var brush = new LinearGradientBrush { GradientStops = null };
			var bindingContext = new object();

			brush.BindingContext = bindingContext;

			Assert.Same(bindingContext, brush.BindingContext);
		}

		[Fact]
		public void NullGradientStopEntryAllowsBindingContextChange()
		{
			var brush = new LinearGradientBrush
			{
				GradientStops = new GradientStopCollection { null }
			};
			var bindingContext = new object();

			brush.BindingContext = bindingContext;

			Assert.Same(bindingContext, brush.BindingContext);
		}

		[Fact]
		public void SharedGradientStopsPreserveExistingMostRecentlyAssignedParentBehavior()
		{
			var stop = new GradientStop();
			var sharedStops = new GradientStopCollection { stop };
			var firstBindingContext = new object();
			var firstBrush = new LinearGradientBrush
			{
				BindingContext = firstBindingContext,
				GradientStops = sharedStops
			};

			Assert.Same(firstBrush, stop.Parent);
			Assert.Same(firstBindingContext, stop.BindingContext);

			var secondBindingContext = new object();
			var secondBrush = new LinearGradientBrush
			{
				BindingContext = secondBindingContext,
				GradientStops = sharedStops
			};

			Assert.Same(secondBrush, stop.Parent);
			Assert.Same(secondBindingContext, stop.BindingContext);
			GC.KeepAlive(firstBrush);
		}

		[Theory]
		[InlineData(SharedStopDetachment.ClearCollection)]
		[InlineData(SharedStopDetachment.RemoveStop)]
		[InlineData(SharedStopDetachment.ReplaceCollection)]
		public void DetachingSharedStopFromPreviousBrushPreservesCurrentParent(SharedStopDetachment detachment)
		{
			var stop = new GradientStop();
			var firstStops = new GradientStopCollection { stop };
			var secondStops = new GradientStopCollection { stop };
			var firstBrush = new LinearGradientBrush { GradientStops = firstStops };
			var secondBindingContext = new object();
			var secondBrush = new LinearGradientBrush
			{
				BindingContext = secondBindingContext,
				GradientStops = secondStops
			};

			switch (detachment)
			{
				case SharedStopDetachment.ClearCollection:
					firstStops.Clear();
					break;
				case SharedStopDetachment.RemoveStop:
					firstStops.Remove(stop);
					break;
				case SharedStopDetachment.ReplaceCollection:
					firstBrush.GradientStops = new GradientStopCollection();
					break;
			}

			Assert.Same(secondBrush, stop.Parent);
			Assert.Same(secondBindingContext, stop.BindingContext);
		}

		[Fact]
		public void DuplicateGradientStopsPreserveOccurrenceSubscriptions()
		{
			var sharedStop = new GradientStop();
			var stops = new GradientStopCollection { sharedStop, sharedStop };
			var brush = new LinearGradientBrush { GradientStops = stops };
			int invalidationCount = 0;
			brush.InvalidateGradientBrushRequested += (_, __) => invalidationCount++;

			sharedStop.Offset = 0.1f;
			Assert.Equal(2, invalidationCount);
			Assert.Same(brush, sharedStop.Parent);

			stops.Remove(sharedStop);
			invalidationCount = 0;

			sharedStop.Offset = 0.2f;
			Assert.Equal(1, invalidationCount);
			Assert.Same(brush, sharedStop.Parent);

			stops.Remove(sharedStop);
			invalidationCount = 0;

			sharedStop.Offset = 0.3f;
			Assert.Equal(0, invalidationCount);
			Assert.Null(sharedStop.Parent);
		}

		[Theory]
		[InlineData(ValueEqualStopDetachment.RemoveStop)]
		[InlineData(ValueEqualStopDetachment.ReplaceStop)]
		[InlineData(ValueEqualStopDetachment.ReplaceCollection)]
		public void DetachingValueEqualStopClearsRemovedParent(ValueEqualStopDetachment detachment)
		{
			var removedStop = new GradientStop { Offset = 0.5f };
			var equalStop = new GradientStop { Offset = 0.5f };
			var stops = new GradientStopCollection { removedStop, equalStop };
			var brush = new LinearGradientBrush { GradientStops = stops };

			Assert.Equal(removedStop, equalStop);

			switch (detachment)
			{
				case ValueEqualStopDetachment.RemoveStop:
					stops.RemoveAt(0);
					break;
				case ValueEqualStopDetachment.ReplaceStop:
					stops[0] = new GradientStop { Offset = 0.5f };
					break;
				case ValueEqualStopDetachment.ReplaceCollection:
					brush.GradientStops = new GradientStopCollection { equalStop };
					break;
			}

			Assert.Null(removedStop.Parent);
			Assert.All(brush.GradientStops, stop => Assert.Same(brush, stop.Parent));
		}

		[Fact]
		public void RemovingGradientStopAllowsReentrantReuse()
		{
			var stop = new GradientStop();
			var stops = new GradientStopCollection { stop };
			var brush = new LinearGradientBrush { GradientStops = stops };
			bool replaced = false;
			int invalidationCount = 0;
			brush.InvalidateGradientBrushRequested += (_, __) =>
			{
				invalidationCount++;
				if (!replaced && stop.Parent is null)
				{
					replaced = true;
					brush.GradientStops = new GradientStopCollection { stop };
				}
			};

			stops.Remove(stop);

			Assert.True(replaced);
			Assert.Same(brush, stop.Parent);

			invalidationCount = 0;
			stop.Offset = 0.5f;

			Assert.Equal(1, invalidationCount);
		}

		[Fact]
		public void ClearingGradientStopsAllowsReentrantReplacement()
		{
			var brush = new LinearGradientBrush
			{
				GradientStops = new GradientStopCollection { new GradientStop() }
			};
			bool replaced = false;
			int invalidationCount = 0;
			brush.InvalidateGradientBrushRequested += (_, __) =>
			{
				invalidationCount++;
				if (!replaced && brush.GradientStops.Count == 0)
				{
					replaced = true;
					brush.GradientStops = new GradientStopCollection { new GradientStop() };
				}
			};

			brush.GradientStops.Clear();

			Assert.True(replaced);
			Assert.Single(brush.GradientStops);
			// Clear and the reentrant replacement are distinct state changes.
			Assert.Equal(2, invalidationCount);
		}

		[Fact]
		public async Task ClearingAndReusingGradientStopsKeepsNewStopsSubscribed()
		{
			var stops = new GradientStopCollection { new GradientStop() };
			var brush = new LinearGradientBrush
			{
				GradientStops = stops
			};
			bool invalidated = false;
			brush.InvalidateGradientBrushRequested += (_, __) => invalidated = true;

			stops.Clear();
			var newStop = new GradientStop();
			stops.Add(newStop);

			await TestHelpers.Collect();
			invalidated = false;

			newStop.Offset = 0.5f;

			Assert.True(invalidated);
			GC.KeepAlive(brush);
		}

		public enum SharedStopDetachment
		{
			ClearCollection,
			RemoveStop,
			ReplaceCollection,
		}

		public enum ValueEqualStopDetachment
		{
			RemoveStop,
			ReplaceStop,
			ReplaceCollection,
		}

		sealed class FinalizerBlocker
		{
			readonly ManualResetEventSlim _finalizerEntered;
			readonly ManualResetEventSlim _releaseFinalizer;

			public FinalizerBlocker(
				ManualResetEventSlim finalizerEntered,
				ManualResetEventSlim releaseFinalizer)
			{
				_finalizerEntered = finalizerEntered;
				_releaseFinalizer = releaseFinalizer;
			}

			~FinalizerBlocker()
			{
				_finalizerEntered.Set();
				_releaseFinalizer.Wait();
			}
		}
	}
}
