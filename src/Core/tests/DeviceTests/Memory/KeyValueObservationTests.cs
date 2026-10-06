#if IOS || MACCATALYST

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoreGraphics;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests.Memory
{
	[Category(TestCategory.Memory)]
	public class KeyValueObservationTests : TestBase
	{
		[Fact]
		public async Task ObservesUntilDisposed()
		{
			await InvokeOnMainThreadAsync(() =>
			{
				var view = new UIView();
				var scrollView = new UIScrollView();
				var layerChanges = 0;
				var viewChanges = 0;
				var scrollChanges = 0;

				var layerObservation = KeyValueObservation.ObserveBounds(view.Layer, () => layerChanges++);
				var viewObservation = KeyValueObservation.ObserveFrame(view, () => viewChanges++);
				var scrollObservation = KeyValueObservation.ObserveContentOffset(scrollView, () => scrollChanges++);

				view.Frame = new CGRect(0, 0, 100, 100);
				scrollView.ContentOffset = new CGPoint(0, 10);

				Assert.True(layerChanges > 0, "layer bounds change was not observed");
				Assert.True(viewChanges > 0, "view frame change was not observed");
				Assert.True(scrollChanges > 0, "contentOffset change was not observed");

				layerObservation.Dispose();
				viewObservation.Dispose();
				scrollObservation.Dispose();
				(layerChanges, viewChanges, scrollChanges) = (0, 0, 0);

				view.Frame = new CGRect(0, 0, 200, 200);
				scrollView.ContentOffset = new CGPoint(0, 20);

				Assert.Equal((0, 0, 0), (layerChanges, viewChanges, scrollChanges));
			});
		}

		[Fact]
		public async Task CanBeDisposedFromItsOwnHandler()
		{
			await InvokeOnMainThreadAsync(() =>
			{
				var view = new UIView();
				var changes = 0;
				KeyValueObservation observation = null;
				observation = KeyValueObservation.ObserveBounds(view.Layer, () =>
				{
					changes++;
					observation.Dispose();
				});

				view.Frame = new CGRect(0, 0, 100, 100);
				view.Frame = new CGRect(0, 0, 200, 200);

				Assert.Equal(1, changes);
			});
		}

		[Fact]
		public async Task HandlerReferencingItsObservationDoesNotLeak()
		{
			WeakReference viewReference = null;
			WeakReference observationReference = null;
			WeakReference stateReference = null;

			await InvokeOnMainThreadAsync(() =>
			{
				var view = new UIView();
				var state = new State { View = view };

				// The native side retains the handler it is given: if that handler could reach the observation
				// (here through state), nothing in this graph would ever be collected.
				state.Observation = KeyValueObservation.ObserveBounds(view.Layer, state.OnChanged);
				view.Frame = new CGRect(0, 0, 100, 100);
				Assert.Equal(1, state.Changes);

				viewReference = new(view);
				observationReference = new(state.Observation);
				stateReference = new(state);
			});

			await AssertionExtensions.WaitForGC(viewReference, observationReference, stateReference);
		}

		// A view that never reaches a window keeps its OnLoaded observation until it is collected, and the
		// collector releases the view and the observer in no particular order. With a managed KVO observer
		// that freed the observer while still registered, and on iOS 17 -[UIView dealloc] then crashed
		// notifying it (SIGSEGV in _NSKeyValueObservationInfoGetObservances); iOS 18 skips a dead observer.
		[Fact]
		public async Task OnLoadedOnViewsThatNeverLoadSurvivesCollection()
		{
			var references = new List<WeakReference>();

			await InvokeOnMainThreadAsync(() =>
			{
				for (var i = 0; i < 200; i++)
				{
					var view = new UIView();
					var state = new State { View = view };
					state.Token = view.OnLoaded(state.OnChanged);
					view.Frame = new CGRect(0, 0, 100 + i, 100);

					references.Add(new(view));
					references.Add(new(state));
				}
			});

			await AssertionExtensions.WaitForGC(references.ToArray());
		}

		// The harmful order: the observation is collected while the view it observes lives on. A managed KVO
		// observer is freed still registered, so on iOS 17 the view's next change dereferences freed memory.
		[Fact]
		public async Task OnLoadedTokenCollectedBeforeItsViewLeavesTheViewUsable()
		{
			var views = new List<UIView>();
			var references = new List<WeakReference>();

			await InvokeOnMainThreadAsync(() =>
			{
				for (var i = 0; i < 50; i++)
				{
					var view = new UIView();
					var state = new State();
					state.Token = view.OnLoaded(state.OnChanged);

					views.Add(view);
					references.Add(new(state));
				}
			});

			await AssertionExtensions.WaitForGC(references.ToArray());

			// Finalized native peers are released on the main thread: let that queue drain
			GC.Collect();
			GC.WaitForPendingFinalizers();
			await Task.Delay(500);

			await InvokeOnMainThreadAsync(() =>
			{
				foreach (var view in views)
				{
					view.Frame = new CGRect(0, 0, 100, 100);
				}
			});
		}

		[Fact]
		public async Task OnLoadedFiresThroughTheObservation()
		{
			await InvokeOnMainThreadAsync(async () =>
			{
				// A plain UIView has no MovedToWindow event, so OnLoaded falls back to observing its layer
				var view = new UIView();
				var loaded = new TaskCompletionSource<bool>();
				using var token = view.OnLoaded(() => loaded.TrySetResult(true));

				await view.AttachAndRun(async () =>
				{
					view.Frame = new CGRect(0, 0, 50, 50);
					await loaded.Task.WaitAsync(TimeSpan.FromSeconds(5));
				});

				Assert.True(loaded.Task.IsCompletedSuccessfully);
			});
		}

		[Fact]
		public async Task GraphicsViewLoadsThroughLifecycleEventsNotKvo()
		{
			await InvokeOnMainThreadAsync(async () =>
			{
				// The reported crash came from a PlatformGraphicsView on the KVO fallback; it now raises MovedToWindow itself
				var view = new PlatformTouchGraphicsView();
				Assert.IsAssignableFrom<IUIViewLifeCycleEvents>(view);

				var loaded = new TaskCompletionSource<bool>();
				using var token = view.OnLoaded(() => loaded.TrySetResult(true));

				await view.AttachAndRun(async () =>
				{
					await loaded.Task.WaitAsync(TimeSpan.FromSeconds(5));
				});

				Assert.True(loaded.Task.IsCompletedSuccessfully);
			});
		}

		class State
		{
			public UIView View;
			public KeyValueObservation Observation;
			public IDisposable Token;
			public int Changes;

			public void OnChanged() => Changes++;
		}
	}
}

#endif
