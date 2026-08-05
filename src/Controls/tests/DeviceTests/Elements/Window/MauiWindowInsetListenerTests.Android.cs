#nullable enable
using System.Threading.Tasks;
using AndroidX.Core.View;
using Microsoft.Maui.Platform;
using Xunit;
using AContext = Android.Content.Context;
using AInsets = AndroidX.Core.Graphics.Insets;
using AView = Android.Views.View;

namespace Microsoft.Maui.DeviceTests
{
	// Deterministic coverage for the IsImeAnimating gate in MauiWindowInsetListener,
	// driving the listener directly (OnPrepare → NotifyViewAttached → OnApplyWindowInsets →
	// OnEnd) with no emulator IME timing involved. The Issue37012 UI test remains the
	// end-to-end smoke check for the real keyboard scenario.
	[Category(TestCategory.Window)]
	public partial class MauiWindowInsetListenerTests : ControlsHandlerTestBase
	{
		static WindowInsetsAnimationCompat CreateImeAnimation() =>
			new(WindowInsetsCompat.Type.Ime(), null, 250);

		static WindowInsetsCompat CreateInsets() =>
			new WindowInsetsCompat.Builder()
				.SetInsets(WindowInsetsCompat.Type.SystemBars(), AInsets.Of(0, 60, 0, 30))!
				.Build()!;

		// Records inset dispatches that make it through the listener's gate, bypassing
		// the safe-area math entirely
		sealed class InsetRecordingView : AView, IHandleWindowInsets
		{
			public InsetRecordingView(AContext context) : base(context)
			{
			}

			public int HandledCount { get; private set; }

			public WindowInsetsCompat? HandleWindowInsets(AView view, WindowInsetsCompat insets)
			{
				HandledCount++;
				return insets;
			}

			public void ResetWindowInsets(AView view)
			{
			}
		}

		[Fact]
		public async Task ViewTrackedBeforeImeAnimationStaysGatedAndAttachedViewBypassesGate()
		{
			await InvokeOnMainThreadAsync(() =>
			{
				var listener = new MauiWindowInsetListener();
				var gated = new InsetRecordingView(MauiContext.Context!);
				var exempt = new InsetRecordingView(MauiContext.Context!);
				var insets = CreateInsets();

				// Sanity: with no animation in flight, dispatches process normally
				listener.OnApplyWindowInsets(gated, insets);
				Assert.Equal(1, gated.HandledCount);

				listener.OnPrepare(CreateImeAnimation());

				// Negative case: a view that was already tracked before the animation
				// started must remain gated for the whole animation
				listener.OnApplyWindowInsets(gated, insets);
				Assert.Equal(1, gated.HandledCount);

				// A view that (re)attached mid-animation has no valid padding yet and
				// must bypass the gate (#37012)
				listener.NotifyViewAttached(exempt);
				listener.OnApplyWindowInsets(exempt, insets);
				Assert.Equal(1, exempt.HandledCount);

				listener.OnEnd(CreateImeAnimation());
			});
		}

		[Fact]
		public async Task GateExemptionDoesNotOutliveTheAnimation()
		{
			await InvokeOnMainThreadAsync(() =>
			{
				var listener = new MauiWindowInsetListener();
				var view = new InsetRecordingView(MauiContext.Context!);
				var insets = CreateInsets();

				listener.OnPrepare(CreateImeAnimation());
				listener.NotifyViewAttached(view);
				listener.OnEnd(CreateImeAnimation());

				// The exemption was granted during animation N; in animation N+1 the same
				// view is tracked state again and must be gated
				listener.OnPrepare(CreateImeAnimation());
				listener.OnApplyWindowInsets(view, insets);
				Assert.Equal(0, view.HandledCount);

				listener.OnEnd(CreateImeAnimation());
			});
		}

		[Fact]
		public async Task GateReleasesSynchronouslyWhenNoPendingViewIsAttached()
		{
			await InvokeOnMainThreadAsync(() =>
			{
				var listener = new MauiWindowInsetListener();
				var detached = new InsetRecordingView(MauiContext.Context!);
				var other = new InsetRecordingView(MauiContext.Context!);
				var insets = CreateInsets();

				listener.OnPrepare(CreateImeAnimation());
				listener.OnApplyWindowInsets(detached, insets);
				Assert.Equal(0, detached.HandledCount);

				// The only pending view is detached; posting the gate release through it
				// would park the runnable until that view re-attaches (possibly never),
				// leaving every view sharing this listener without safe-area updates.
				// The release must happen synchronously instead.
				listener.OnEnd(CreateImeAnimation());

				listener.OnApplyWindowInsets(other, insets);
				Assert.Equal(1, other.HandledCount);
			});
		}

		[Fact]
		public async Task AllGatedViewsProcessDispatchesAfterAnimationEnds()
		{
			var container = await InvokeOnMainThreadAsync(() => new global::Android.Widget.LinearLayout(MauiContext.Context!));

			await container.AttachAndRun(async () =>
			{
				var listener = new MauiWindowInsetListener();
				var first = new InsetRecordingView(MauiContext.Context!);
				var second = new InsetRecordingView(MauiContext.Context!);
				var insets = CreateInsets();

				container.AddView(first);
				container.AddView(second);

				try
				{
					listener.OnPrepare(CreateImeAnimation());

					listener.OnApplyWindowInsets(first, insets);
					listener.OnApplyWindowInsets(second, insets);
					Assert.Equal(0, first.HandledCount);
					Assert.Equal(0, second.HandledCount);

					// Both views are attached, so the gate release is posted one
					// main-looper turn after OnEnd; yield to let it run
					listener.OnEnd(CreateImeAnimation());
					await Task.Delay(100);

					// The gate must be open again and every previously gated view must
					// process dispatches — not just the one the release was posted on
					listener.OnApplyWindowInsets(first, insets);
					listener.OnApplyWindowInsets(second, insets);
					Assert.Equal(1, first.HandledCount);
					Assert.Equal(1, second.HandledCount);
				}
				finally
				{
					container.RemoveView(first);
					container.RemoveView(second);
				}
			});
		}
	}
}
