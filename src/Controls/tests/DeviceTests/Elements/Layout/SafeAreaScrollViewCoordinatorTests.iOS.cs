using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	/// <summary>
	/// Covers how <c>SafeAreaScrollViewCoordinator</c> moves the system top inset between MAUI's
	/// manual fallback and UIKit for an edge-extended, vendor-style scroll view.
	///
	/// UIKit's safe-area propagation into such a scroll view resolves asynchronously, so the handoff
	/// routinely lands in the middle of a scroll gesture. The scroll view here reports the safe area
	/// and the interaction state that the coordinator reads, which places the handoff at an exact
	/// moment instead of waiting for real UIKit timing to reproduce it.
	/// </summary>
	[Category(TestCategory.Layout)]
	public class SafeAreaScrollViewCoordinatorTests : ControlsHandlerTestBase
	{
		// An expanded iOS 26 large-title band. Only stability across a single test matters.
		const double SystemTopInset = 96;
		const double PageWidth = 390;
		const double PageHeight = 844;

		[Fact]
		public Task LateSafeAreaHandoffDuringDragNeverStacksBothSystemInsets() =>
			RunCoordinatorTest((coordinator, host, scrollView) =>
			{
				// UIKit has not propagated a safe area yet, so MAUI supplies the inset itself.
				scrollView.SimulatedSafeAreaTopInset = 0;
				DelegateSafeArea(coordinator, host);

				AssertOwnedByMaui(scrollView);

				// The propagation lands while the user is dragging. Ownership may only move if the
				// delegated inset gives up the system region in the same pass; flipping the behavior
				// to Always while MAUI's copy is still applied stacks both and doubles the inset
				// under the user's finger.
				scrollView.SimulatedInteraction = true;
				scrollView.SimulatedSafeAreaTopInset = (nfloat)SystemTopInset;
				DelegateSafeArea(coordinator, host);

				AssertTopInsetIsCoveredExactlyOnce(scrollView);
			});

		[Fact]
		public Task LateSafeAreaHandoffCompletesWhenIdle() =>
			RunCoordinatorTest((coordinator, host, scrollView) =>
			{
				scrollView.SimulatedSafeAreaTopInset = 0;
				DelegateSafeArea(coordinator, host);

				AssertOwnedByMaui(scrollView);

				scrollView.SimulatedSafeAreaTopInset = (nfloat)SystemTopInset;
				DelegateSafeArea(coordinator, host);

				AssertOwnedByUIKit(scrollView);
				AssertTopInsetIsCoveredExactlyOnce(scrollView);
			});

		[Fact]
		public Task LostSafeAreaPropagationDuringDragKeepsTheTopInsetCovered() =>
			RunCoordinatorTest((coordinator, host, scrollView) =>
			{
				scrollView.SimulatedSafeAreaTopInset = (nfloat)SystemTopInset;
				DelegateSafeArea(coordinator, host);

				AssertOwnedByUIKit(scrollView);

				// The mirror image of the handoff above: UIKit stops supplying the region mid-drag,
				// so MAUI has to take the manual inset back in the same pass or the rows slide up
				// underneath the navigation bar.
				scrollView.SimulatedInteraction = true;
				scrollView.SimulatedSafeAreaTopInset = 0;
				DelegateSafeArea(coordinator, host);

				AssertOwnedByMaui(scrollView);
				AssertTopInsetIsCoveredExactlyOnce(scrollView);
			});

		[Fact]
		public Task ShrinkingSystemInsetDuringDragKeepsTheCurrentInset() =>
			RunCoordinatorTest((coordinator, host, scrollView) =>
			{
				scrollView.SimulatedSafeAreaTopInset = 0;
				DelegateSafeArea(coordinator, host);

				AssertOwnedByMaui(scrollView);

				// Rotating mid-drag restarts the layout epoch with a shorter system inset. Shrinking
				// the inset under the user's finger yanks the content, so the smaller value waits
				// for the gesture to end.
				scrollView.SimulatedInteraction = true;
				DelegateSafeArea(
					coordinator,
					host,
					pageBounds: new Rect(0, 0, PageHeight, PageWidth),
					systemTopInset: 0);

				AssertOwnedByMaui(scrollView);
				AssertTopInsetIsCoveredExactlyOnce(scrollView);
			});

		[Fact]
		public Task PinnedTopologyRestoresOriginalOwnershipAndDefersEdgeTransitionDuringDrag() =>
			RunNestedCoordinatorTest((coordinator, host, scrollView) =>
			{
				// Edge-extended content starts at the page edge while its MAUI host is arranged
				// below the safe area, so MAUI initially supplies the system inset.
				scrollView.Frame = new CoreGraphics.CGRect(0, 0, PageWidth, PageHeight);
				DelegateSafeArea(coordinator, host);
				AssertOwnedByMaui(scrollView);

				// Once the vendor container's constraints settle at the safe-area edge, the
				// coordinator must restore the scroll view's original adjustment behavior without
				// forgetting which native view and topology it is tracking.
				scrollView.Frame = new CoreGraphics.CGRect(0, SystemTopInset, PageWidth, PageHeight - SystemTopInset);
				DelegateSafeArea(coordinator, host);
				Assert.Equal(
					UIScrollViewContentInsetAdjustmentBehavior.Automatic,
					scrollView.ContentInsetAdjustmentBehavior);
				Assert.InRange((double)scrollView.ContentInset.Top, -0.5, 0.5);

				// A constraint change back to edge-extended must not change ownership under the
				// user's finger. It is applied on the first idle arrange instead.
				scrollView.SimulatedInteraction = true;
				scrollView.Frame = new CoreGraphics.CGRect(0, 0, PageWidth, PageHeight);
				DelegateSafeArea(coordinator, host);
				Assert.Equal(
					UIScrollViewContentInsetAdjustmentBehavior.Automatic,
					scrollView.ContentInsetAdjustmentBehavior);
				Assert.InRange((double)scrollView.ContentInset.Top, -0.5, 0.5);

				scrollView.SimulatedInteraction = false;
				DelegateSafeArea(coordinator, host);
				AssertOwnedByMaui(scrollView);
			});

		/// <summary>
		/// Mirrors <c>ContentPage.ApplyCrossPlatformArrangeSafeArea</c>: the host is arranged inside
		/// the safe area, then the coordinator receives the full, unadjusted page bounds so it can
		/// expand the scroll view back underneath the navigation bar.
		/// </summary>
		static void DelegateSafeArea(
			SafeAreaScrollViewCoordinator coordinator,
			IView host,
			Rect? pageBounds = null,
			double? systemTopInset = null)
		{
			var bounds = pageBounds ?? new Rect(0, 0, PageWidth, PageHeight);
			var topInset = systemTopInset ?? SystemTopInset;

			host.Arrange(new Rect(
				bounds.X,
				bounds.Y + topInset,
				bounds.Width,
				bounds.Height - topInset));

			Assert.True(coordinator.TryDelegate(host, host, bounds, topInset, topInset));
		}

		static void AssertOwnedByMaui(SimulatedSafeAreaScrollView scrollView)
		{
			Assert.Equal(
				UIScrollViewContentInsetAdjustmentBehavior.Never,
				scrollView.ContentInsetAdjustmentBehavior);
			Assert.InRange(
				(double)scrollView.ContentInset.Top,
				SystemTopInset - 0.5,
				SystemTopInset + 0.5);
		}

		static void AssertOwnedByUIKit(SimulatedSafeAreaScrollView scrollView)
		{
			Assert.Equal(
				UIScrollViewContentInsetAdjustmentBehavior.Always,
				scrollView.ContentInsetAdjustmentBehavior);
			Assert.InRange((double)scrollView.ContentInset.Top, -0.5, 0.5);
		}

		static void AssertTopInsetIsCoveredExactlyOnce(SimulatedSafeAreaScrollView scrollView)
		{
			Assert.InRange(scrollView.EffectiveTopInset, SystemTopInset - 0.5, SystemTopInset + 0.5);
			Assert.InRange(
				(double)scrollView.VerticalScrollIndicatorInsets.Top,
				(double)scrollView.ContentInset.Top - 0.5,
				(double)scrollView.ContentInset.Top + 0.5);
		}

		Task RunCoordinatorTest(
			Action<SafeAreaScrollViewCoordinator, IView, SimulatedSafeAreaScrollView> assertions) =>
			InvokeOnMainThreadAsync(() =>
			{
				var host = new SimulatedSafeAreaScrollViewStub();
				var handler = CreateHandler<SimulatedSafeAreaScrollViewStubHandler>(host);
				var coordinator = new SafeAreaScrollViewCoordinator();

				try
				{
					assertions(coordinator, host, handler.PlatformView);
				}
				finally
				{
					coordinator.Reset();
				}
			});

		Task RunNestedCoordinatorTest(
			Action<SafeAreaScrollViewCoordinator, IView, SimulatedSafeAreaScrollView> assertions) =>
			InvokeOnMainThreadAsync(() =>
			{
				var host = new SimulatedNestedSafeAreaScrollViewStub();
				var handler = CreateHandler<SimulatedNestedSafeAreaScrollViewStubHandler>(host);
				var rootView = new UIView(new CoreGraphics.CGRect(0, 0, PageWidth, PageHeight));
				var coordinator = new SafeAreaScrollViewCoordinator();

				rootView.AddSubview(handler.PlatformView);

				try
				{
					assertions(coordinator, host, handler.PlatformView.ScrollView);
				}
				finally
				{
					coordinator.Reset();
					handler.PlatformView.RemoveFromSuperview();
				}
			});
	}

	class SimulatedSafeAreaScrollViewStub : View
	{
	}

	class SimulatedSafeAreaScrollViewStubHandler
		: ViewHandler<SimulatedSafeAreaScrollViewStub, SimulatedSafeAreaScrollView>
	{
		public static readonly IPropertyMapper<SimulatedSafeAreaScrollViewStub, SimulatedSafeAreaScrollViewStubHandler> Mapper =
			new PropertyMapper<SimulatedSafeAreaScrollViewStub, SimulatedSafeAreaScrollViewStubHandler>(ViewMapper);

		public SimulatedSafeAreaScrollViewStubHandler()
			: base(Mapper)
		{
		}

		protected override SimulatedSafeAreaScrollView CreatePlatformView() => new();
	}

	class SimulatedNestedSafeAreaScrollViewStub : View
	{
	}

	class SimulatedNestedSafeAreaScrollViewStubHandler
		: ViewHandler<SimulatedNestedSafeAreaScrollViewStub, SimulatedNestedSafeAreaScrollViewContainer>
	{
		public static readonly IPropertyMapper<SimulatedNestedSafeAreaScrollViewStub, SimulatedNestedSafeAreaScrollViewStubHandler> Mapper =
			new PropertyMapper<SimulatedNestedSafeAreaScrollViewStub, SimulatedNestedSafeAreaScrollViewStubHandler>(ViewMapper);

		public SimulatedNestedSafeAreaScrollViewStubHandler()
			: base(Mapper)
		{
		}

		protected override SimulatedNestedSafeAreaScrollViewContainer CreatePlatformView() => new();
	}

	class SimulatedNestedSafeAreaScrollViewContainer : UIView
	{
		public SimulatedNestedSafeAreaScrollViewContainer()
		{
			ScrollView = new SimulatedSafeAreaScrollView
			{
				AlwaysBounceVertical = true,
				ContentInsetAdjustmentBehavior = UIScrollViewContentInsetAdjustmentBehavior.Automatic
			};
			AddSubview(ScrollView);
		}

		public SimulatedSafeAreaScrollView ScrollView { get; }
	}

	/// <summary>
	/// A plain vendor-style <see cref="UIScrollView"/>: the topology the coordinator has to inset by
	/// hand, because it never implements MAUI's delegated-inset interface. Safe area and interaction
	/// state are simulated so a test can place the UIKit handoff at an exact moment.
	/// </summary>
	class SimulatedSafeAreaScrollView : UIScrollView
	{
		public bool SimulatedInteraction { get; set; }

		public nfloat SimulatedSafeAreaTopInset { get; set; }

		public override bool Dragging => SimulatedInteraction;

		public override bool Tracking => SimulatedInteraction;

		public override bool Decelerating => false;

		public override UIEdgeInsets SafeAreaInsets => new(SimulatedSafeAreaTopInset, 0, 0, 0);

		/// <summary>
		/// How far down the content is actually pushed: the inset MAUI applies, plus the copy UIKit
		/// adds once the scroll view is switched to <c>Always</c>. Derived from the two values MAUI
		/// writes rather than read back from <c>AdjustedContentInset</c>, which UIKit computes from
		/// its own view-hierarchy state instead of the simulated safe area.
		/// </summary>
		public double EffectiveTopInset =>
			(double)ContentInset.Top +
			(ContentInsetAdjustmentBehavior == UIScrollViewContentInsetAdjustmentBehavior.Always
				? (double)SimulatedSafeAreaTopInset
				: 0);
	}
}
