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

		// Awaits one main-looper turn posted through the given view, so a runnable the
		// listener posted earlier through any attached view is guaranteed to have run
		static Task NextLooperTurnAsync(AView view)
		{
			var turn = new TaskCompletionSource<bool>();
			view.Post(() => turn.SetResult(true));
			return turn.Task;
		}

		// Records inset dispatches that make it through the listener's gate (bypassing the
		// safe-area math entirely) and any RequestApplyInsets the animation-end release issues
		sealed class InsetRecordingView : AView, IHandleWindowInsets
		{
			public InsetRecordingView(AContext context) : base(context)
			{
			}

			public int HandledCount { get; private set; }

			public int RequestApplyInsetsCount { get; private set; }

			public WindowInsetsCompat? HandleWindowInsets(AView view, WindowInsetsCompat insets)
			{
				HandledCount++;
				return insets;
			}

			public void ResetWindowInsets(AView view)
			{
			}

			public override void RequestApplyInsets()
			{
				RequestApplyInsetsCount++;
				base.RequestApplyInsets();
			}
		}

		[Fact]
		public async Task ViewTrackedBeforeImeAnimationStaysGatedAndAttachedViewBypassesGateOnce()
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
				// must bypass the gate for its initial apply (#37012)
				listener.NotifyViewAttached(exempt);
				listener.OnApplyWindowInsets(exempt, insets);
				Assert.Equal(1, exempt.HandledCount);

				// The exemption is one-shot: once padded, the view is gated like the rest
				// for the remainder of the animation
				listener.OnApplyWindowInsets(exempt, insets);
				Assert.Equal(1, exempt.HandledCount);

				listener.OnEnd(CreateImeAnimation());
			});
		}

		[Fact]
		public async Task GateExemptionIsClearedWhenTheAnimationEnds()
		{
			await InvokeOnMainThreadAsync(() =>
			{
				var listener = new MauiWindowInsetListener();
				var view = new InsetRecordingView(MauiContext.Context!);
				var insets = CreateInsets();

				listener.OnPrepare(CreateImeAnimation());
				listener.NotifyViewAttached(view);
				listener.OnEnd(CreateImeAnimation());

				// Assert between OnEnd and the next OnPrepare, so this discriminates the
				// clear in OnEnd specifically rather than passing on either clear site
				listener.OnPrepare(CreateImeAnimation());
				listener.OnApplyWindowInsets(view, insets);
				Assert.Equal(0, view.HandledCount);

				listener.OnEnd(CreateImeAnimation());
			});
		}

		[Fact]
		public async Task GateReleasesSynchronouslyWhenNoAttachedViewIsAvailable()
		{
			await InvokeOnMainThreadAsync(() =>
			{
				var listener = new MauiWindowInsetListener();
				var detached = new InsetRecordingView(MauiContext.Context!);
				var other = new InsetRecordingView(MauiContext.Context!);
				var insets = CreateInsets();

				// The listener knows about a view that carries padding, but it is detached
				listener.TrackView(detached);

				listener.OnPrepare(CreateImeAnimation());
				listener.OnApplyWindowInsets(detached, insets);
				Assert.Equal(0, detached.HandledCount);

				// Posting the release through a detached view would park the runnable until
				// that view re-attaches (possibly never), leaving every view sharing this
				// listener without safe-area updates. It must release synchronously instead.
				listener.OnEnd(CreateImeAnimation());

				listener.OnApplyWindowInsets(other, insets);
				Assert.Equal(1, other.HandledCount);
			});
		}

		[Fact]
		public async Task ReapplyStaysOwedWhenTheAnimationEndsWithNoUsablePoster()
		{
			var container = await InvokeOnMainThreadAsync(() =>
				new global::Android.Widget.LinearLayout(MauiContext.Context!));

			await InvokeOnMainThreadAsync(async () =>
			{
				await container.AttachAndRun(async () =>
				{
					var listener = new MauiWindowInsetListener();
					var detached = new InsetRecordingView(MauiContext.Context!);
					var attached = new InsetRecordingView(MauiContext.Context!);
					var insets = CreateInsets();

					container.AddView(attached);

					try
					{
						// Only a detached view is known, so the animation has to end without
						// being able to issue the re-apply it owes
						listener.TrackView(detached);
						listener.OnPrepare(CreateImeAnimation());
						listener.OnApplyWindowInsets(detached, insets);
						listener.OnEnd(CreateImeAnimation());
						await NextLooperTurnAsync(container);

						Assert.Equal(0, detached.RequestApplyInsetsCount);

						// The settled insets are still owed, so the next animation that *can*
						// reach an attached view must deliver them rather than the request
						// having been silently dropped
						listener.TrackView(attached);
						listener.OnPrepare(CreateImeAnimation());
						listener.OnEnd(CreateImeAnimation());
						await NextLooperTurnAsync(container);

						Assert.Equal(1, attached.RequestApplyInsetsCount);
					}
					finally
					{
						container.RemoveView(attached);
					}
				});
			});
		}

		[Fact]
		public async Task GatedDispatchesTriggerOneReapplyWhenTheAnimationEnds()
		{
			await InvokeOnMainThreadAsync(async () =>
			{
				var container = new global::Android.Widget.LinearLayout(MauiContext.Context!);

				await container.AttachAndRun(async () =>
				{
					var listener = new MauiWindowInsetListener();
					var first = new InsetRecordingView(MauiContext.Context!);
					var second = new InsetRecordingView(MauiContext.Context!);
					var insets = CreateInsets();

					container.AddView(first);
					container.AddView(second);
					listener.TrackView(first);

					try
					{
						listener.OnPrepare(CreateImeAnimation());

						listener.OnApplyWindowInsets(first, insets);
						listener.OnApplyWindowInsets(second, insets);
						Assert.Equal(0, first.HandledCount);
						Assert.Equal(0, second.HandledCount);

						// The release is posted through an attached view; sequence on the same
						// looper instead of racing a fixed delay
						listener.OnEnd(CreateImeAnimation());
						await NextLooperTurnAsync(container);

						// A single RequestApplyInsets reaches the ViewRootImpl and re-dispatches
						// insets across the whole hierarchy, so exactly one call is expected —
						// not one per gated view
						Assert.Equal(1, first.RequestApplyInsetsCount);
						Assert.Equal(0, second.RequestApplyInsetsCount);

						// And the gate must be open again for every view, not just the poster
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
			});
		}

		[Fact]
		public async Task StaleGateReleaseDoesNotOpenTheGateOfTheNextAnimation()
		{
			await InvokeOnMainThreadAsync(async () =>
			{
				var container = new global::Android.Widget.LinearLayout(MauiContext.Context!);

				await container.AttachAndRun(async () =>
				{
					var listener = new MauiWindowInsetListener();
					var view = new InsetRecordingView(MauiContext.Context!);
					var insets = CreateInsets();

					container.AddView(view);
					listener.TrackView(view);

					try
					{
						// Animation 1 ends with a gated dispatch, so its gate release is
						// posted for the next looper turn...
						listener.OnPrepare(CreateImeAnimation());
						listener.OnApplyWindowInsets(view, insets);
						listener.OnEnd(CreateImeAnimation());

						// ...but animation 2 starts before that runnable runs
						// (a hide immediately followed by a show)
						listener.OnPrepare(CreateImeAnimation());
						await NextLooperTurnAsync(container);

						// The stale release from animation 1 must not have opened the
						// gate in the middle of animation 2
						listener.OnApplyWindowInsets(view, insets);
						Assert.Equal(0, view.HandledCount);

						listener.OnEnd(CreateImeAnimation());
						await NextLooperTurnAsync(container);

						listener.OnApplyWindowInsets(view, insets);
						Assert.Equal(1, view.HandledCount);
					}
					finally
					{
						container.RemoveView(view);
					}
				});
			});
		}
	}
}
