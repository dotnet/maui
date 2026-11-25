using System;
using System.Threading.Tasks;
using CoreGraphics;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.ScrollView)]
	public class MauiScrollViewTests : TestBase
	{
		[Theory]
		[InlineData(UIModalPresentationStyle.PageSheet, false, 36, 601)]
		[InlineData(UIModalPresentationStyle.PageSheet, true, 36, 580)]
		[InlineData(UIModalPresentationStyle.PageSheet, true, 0, 580)]
		[InlineData(UIModalPresentationStyle.FullScreen, true, 36, 601)]
		[InlineData(UIModalPresentationStyle.FullScreen, false, 36, 601)]
		[InlineData(UIModalPresentationStyle.Automatic, false, 36, 601)]
		public Task AutomaticInsetStabilizationRequiresPresentedPageSheet(
			UIModalPresentationStyle style, bool presented, int adjustedTop, int expectedHeight) =>
			InvokeOnMainThreadAsync(() =>
			{
				using var presenter = new UIViewController();
				using var controller = new PresentationControllerStub
				{
					ModalPresentationStyle = style,
					Presenter = presented ? presenter : null
				};
				var layout = new SafeAreaLayoutStub();
				using var scrollView = CreateScrollView(layout, controller, adjustedTop);

				Arrange(scrollView);

				Assert.Equal(expectedHeight, (double)scrollView.ContentSize.Height);
			});

		[Theory]
		[InlineData(UIModalPresentationStyle.FullScreen, 601)]
		[InlineData(UIModalPresentationStyle.PageSheet, 580)]
		public Task ChildControllerStyleDoesNotOverrideModalPresentation(
			UIModalPresentationStyle parentStyle, int expectedHeight) =>
			InvokeOnMainThreadAsync(() =>
			{
				using var presenter = new UIViewController();
				using var parent = new PresentationControllerStub
				{
					ModalPresentationStyle = parentStyle,
					Presenter = presenter
				};
				using var child = new PresentationControllerStub
				{
					ModalPresentationStyle = UIModalPresentationStyle.PageSheet,
					Presenter = presenter
				};
				parent.AddChildViewController(child);
				var layout = new SafeAreaLayoutStub();
				using var scrollView = CreateScrollView(layout, child, 36);
				try
				{
					Arrange(scrollView);

					Assert.Equal(expectedHeight, (double)scrollView.ContentSize.Height);
				}
				finally
				{
					child.RemoveFromParentViewController();
				}
			});

		[Fact]
		public Task PresentationChangesAreNotCachedUntilMovedToWindow() =>
			InvokeOnMainThreadAsync(() =>
			{
				using var presenter = new UIViewController();
				using var controller = new PresentationControllerStub
				{
					ModalPresentationStyle = UIModalPresentationStyle.PageSheet
				};
				var layout = new SafeAreaLayoutStub();
				using var scrollView = CreateScrollView(layout, controller, 36);

				Arrange(scrollView);
				Assert.Equal(601, (double)scrollView.ContentSize.Height);

				controller.Presenter = presenter;
				Arrange(scrollView);
				Assert.Equal(580, (double)scrollView.ContentSize.Height);

				controller.Presenter = null;
				Arrange(scrollView);
				Assert.Equal(601, (double)scrollView.ContentSize.Height);
			});

		[Theory]
		[InlineData(false, 88, 328, 88)]
		[InlineData(true, 0, 481, 329)]
		public Task RtlInitialOffsetAccountsForManualHorizontalSafeArea(
			bool nativeInsetsCoverAllEdges, int expectedManualHorizontalInset, int expectedContentWidth, int expectedOffset) =>
			InvokeOnMainThreadAsync(() =>
			{
				var rawInsets = new UIEdgeInsets(0, 44, 34, 44);
				using var scrollView = CreateScrollView(new SafeAreaLayoutStub(), null, 0);
				scrollView.ContentInsetAdjustmentBehavior = UIScrollViewContentInsetAdjustmentBehavior.Automatic;
				scrollView.SafeAreaInsetsValue = rawInsets;
				scrollView.SystemInset = nativeInsetsCoverAllEdges ? rawInsets : new UIEdgeInsets(0, 0, 34, 0);

				// Establish layout before RTL so the offset uses the updated safe-area bounds.
				Arrange(scrollView);
				scrollView.SemanticContentAttribute = UISemanticContentAttribute.ForceRightToLeft;
				Arrange(scrollView);

				Assert.Equal(UIUserInterfaceLayoutDirection.RightToLeft, scrollView.EffectiveUserInterfaceLayoutDirection);
				Assert.Equal(expectedManualHorizontalInset, scrollView.SafeAreaBakedIntoContent.HorizontalThickness);
				Assert.Equal(expectedContentWidth, (double)scrollView.ContentSize.Width);
				Assert.Equal(expectedOffset, (double)scrollView.ContentOffset.X);
			});

		[Theory]
		[InlineData(UIModalPresentationStyle.PageSheet, 580)]
		[InlineData(UIModalPresentationStyle.FullScreen, 601)]
		public Task NativePresentationControlsInsetStabilization(UIModalPresentationStyle style, int expectedHeight) =>
			InvokeOnMainThreadAsync(async () =>
			{
				var scene = UIApplication.SharedApplication.GetKeyWindow().WindowScene;
				using var window = new UIWindow(scene);
				using var presenter = new UIViewController();
				using var controller = new UIViewController { ModalPresentationStyle = style };
				window.RootViewController = presenter;
				window.Hidden = false;
				try
				{
					await presenter.PresentViewControllerAsync(controller, false);
					Assert.NotNull(controller.PresentingViewController);
					var layout = new SafeAreaLayoutStub();
					using var scrollView = CreateScrollView(layout, null, 36);
					controller.View.AddSubview(scrollView);
					try
					{
						Arrange(scrollView);

						Assert.Equal(expectedHeight, (double)scrollView.ContentSize.Height);
					}
					finally
					{
						scrollView.RemoveFromSuperview();
					}
				}
				finally
				{
					await presenter.DismissViewControllerAsync(false);
					window.Hidden = true;
				}
			});

		static InsetScrollView CreateScrollView(SafeAreaLayoutStub layout, UIResponder responder, int adjustedTop)
		{
			var scrollView = new InsetScrollView
			{
				Frame = new CGRect(0, 0, 240, 600),
				View = layout,
				Responder = responder,
				SystemInset = new UIEdgeInsets(adjustedTop, 0, 0, 0)
			};
			((ICrossPlatformLayoutBacking)scrollView).CrossPlatformLayout = layout;
			return scrollView;
		}

		static void Arrange(MauiScrollView scrollView)
		{
			((IPlatformMeasureInvalidationController)scrollView).InvalidateMeasure(false);
			scrollView.LayoutSubviews();
		}

		class InsetScrollView : MauiScrollView
		{
			public UIResponder Responder { get; set; }
			public UIEdgeInsets SystemInset { get; set; }
			public UIEdgeInsets SafeAreaInsetsValue { get; set; } = new UIEdgeInsets(36, 0, 0, 0);
			public override UIResponder NextResponder => Responder ?? base.NextResponder;
			public override UIEdgeInsets SafeAreaInsets => SafeAreaInsetsValue;
			public override UIEdgeInsets AdjustedContentInset => SystemInset;
		}

		class PresentationControllerStub : UIViewController
		{
			public UIViewController Presenter { get; set; }
			public override UIViewController PresentingViewController => Presenter;
			public override UIResponder NextResponder => ParentViewController ?? base.NextResponder;
		}

		class SafeAreaLayoutStub : StubBase, ICrossPlatformLayout, ISafeAreaView2
		{
			public bool HasExplicitSafeAreaEdges => false;
			public Thickness SafeAreaInsets { private get; set; }
			public SafeAreaRegions GetSafeAreaRegionsForEdge(int edge) => SafeAreaRegions.Default;
			public Size CrossPlatformMeasure(double widthConstraint, double heightConstraint) => new Size(240, 580);
			public Size CrossPlatformArrange(Rect bounds) => new Size(240, 580);
		}
	}
}
