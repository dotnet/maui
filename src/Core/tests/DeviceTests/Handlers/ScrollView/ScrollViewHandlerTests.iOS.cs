using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using ObjCRuntime;
using UIKit;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ScrollViewHandlerTests : CoreHandlerTestBase<ScrollViewHandler, ScrollViewStub>
	{
		[Fact]
		public async Task ContentInitializesCorrectly()
		{
			EnsureHandlerCreated(builder => { builder.ConfigureMauiHandlers(handlers => { handlers.AddHandler<EntryStub, EntryHandler>(); }); });

			bool result = await InvokeOnMainThreadAsync(() =>
			{
				var entry = new EntryStub() { Text = "In a ScrollView" };

				var scrollView = new ScrollViewStub()
				{
					Content = entry
				};

				var scrollViewHandler = CreateHandler(scrollView);
				return scrollViewHandler.PlatformView.FindDescendantView<MauiTextField>() is not null;
			});

			Assert.True(result, $"Expected (but did not find) a {nameof(MauiTextField)} in the Subviews array");
		}

		[Fact]
		public async Task ScrollViewContentSizeSet()
		{
			EnsureHandlerCreated(builder => { builder.ConfigureMauiHandlers(handlers => { handlers.AddHandler<EntryStub, EntryHandler>(); }); });

			var scrollView = new ScrollViewStub();
			var entry = new EntryStub() { Text = "In a ScrollView" };
			scrollView.Content = entry;

			var scrollViewHandler = await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler(scrollView);

				// Setting an arbitrary value so we can verify that the handler is setting
				// the UIScrollView's ContentSize property during AttachAndRun
				handler.PlatformView.ContentSize = new CoreGraphics.CGSize(100, 100);
				return handler;
			});

			await InvokeOnMainThreadAsync(async () =>
			{
				await scrollViewHandler.PlatformView.AttachAndRun(() =>
				{
					// Verify that the ContentSize values have been modified
					Assert.NotEqual(100, scrollViewHandler.PlatformView.ContentSize.Height);
					Assert.NotEqual(100, scrollViewHandler.PlatformView.ContentSize.Width);
				});
			});
		}

		// ── ComputeSafeArea unit tests (Issue #35410) ──────────────────────────────────
		// These tests call the static helper directly with controlled inputs, verifying
		// that each CIAB mode picks the correct edge ownership without requiring a live
		// UIScrollView or a notch device.

		[Fact]
		public void ComputeSafeArea_Never_UsesDeviceInsetForAllEdges()
		{
			// landscape-left scenario: notch is on the left
			var aci = new UIEdgeInsets(top: 0, left: 0, bottom: 0, right: 0);
			var deviceInset = new UIEdgeInsets(top: 20, left: 44, bottom: 0, right: 0);

			var result = MauiScrollView.ComputeSafeArea(
				aci,
				UIScrollViewContentInsetAdjustmentBehavior.Never,
				deviceInset);

			Assert.Equal(44, result.Left);
			Assert.Equal(20, result.Top);
			Assert.Equal(0, result.Bottom);
			Assert.Equal(0, result.Right);
		}

		[Fact]
		public void ComputeSafeArea_Automatic_AciZero_UsesDeviceInset()
		{
			var aci = UIEdgeInsets.Zero;
			var deviceInset = new UIEdgeInsets(top: 20, left: 44, bottom: 0, right: 0);

			var result = MauiScrollView.ComputeSafeArea(
				aci,
				UIScrollViewContentInsetAdjustmentBehavior.Automatic,
				deviceInset);

			Assert.Equal(44, result.Left);
			Assert.Equal(20, result.Top);
		}

		[Fact]
		public void ComputeSafeArea_Automatic_LandscapeLeft_UsesDeviceInsetForHorizontal()
		{
			// In landscape-left with CIAB.Automatic:
			//   UIKit does NOT add left/right to ACI → SACI.Left = 0
			//   but device SafeAreaInsets.Left = 44 (notch)
			var aci = new UIEdgeInsets(top: 20, left: 0, bottom: 0, right: 0); // UIKit-reported
			var deviceInset = new UIEdgeInsets(top: 20, left: 44, bottom: 0, right: 0); // actual notch

			var result = MauiScrollView.ComputeSafeArea(
				aci,
				UIScrollViewContentInsetAdjustmentBehavior.Automatic,
				deviceInset);

			// Left must come from deviceInset (44), not aci (0) — this is the bug fix
			Assert.Equal(44, result.Left);
			Assert.Equal(0, result.Right);
			// Top/Bottom come from aci (UIKit-owned)
			Assert.Equal(20, result.Top);
			Assert.Equal(0, result.Bottom);
		}

		[Fact]
		public void ComputeSafeArea_Automatic_LandscapeRight_UsesDeviceInsetForRight()
		{
			var aci = new UIEdgeInsets(top: 20, left: 0, bottom: 0, right: 0);
			var deviceInset = new UIEdgeInsets(top: 20, left: 0, bottom: 0, right: 44);

			var result = MauiScrollView.ComputeSafeArea(
				aci,
				UIScrollViewContentInsetAdjustmentBehavior.Automatic,
				deviceInset);

			Assert.Equal(0, result.Left);
			Assert.Equal(44, result.Right);
			Assert.Equal(20, result.Top);
		}

		[Fact]
		public void ComputeSafeArea_Always_UsesAciForAllEdges()
		{
			// With CIAB.Always UIKit puts left/right into ACI, so SACI.Left == SafeAreaInsets.Left
			var aci = new UIEdgeInsets(top: 20, left: 44, bottom: 0, right: 0);
			var deviceInset = new UIEdgeInsets(top: 20, left: 44, bottom: 0, right: 0);

			var result = MauiScrollView.ComputeSafeArea(
				aci,
				UIScrollViewContentInsetAdjustmentBehavior.Always,
				deviceInset);

			Assert.Equal(44, result.Left);
			Assert.Equal(20, result.Top);
		}

		[Fact]
		public void ComputeSafeArea_Always_AciZero_UsesDeviceInset()
		{
			var aci = UIEdgeInsets.Zero;
			var deviceInset = new UIEdgeInsets(top: 20, left: 44, bottom: 0, right: 0);

			var result = MauiScrollView.ComputeSafeArea(
				aci,
				UIScrollViewContentInsetAdjustmentBehavior.Always,
				deviceInset);

			Assert.Equal(44, result.Left);
			Assert.Equal(20, result.Top);
		}

		[Fact]
		public void ComputeSafeArea_Automatic_Portrait_NoNotchEdge()
		{
			var aci = new UIEdgeInsets(top: 44, left: 0, bottom: 34, right: 0);
			var deviceInset = new UIEdgeInsets(top: 44, left: 0, bottom: 34, right: 0);

			var result = MauiScrollView.ComputeSafeArea(
				aci,
				UIScrollViewContentInsetAdjustmentBehavior.Automatic,
				deviceInset);

			Assert.Equal(0, result.Left);
			Assert.Equal(0, result.Right);
			Assert.Equal(44, result.Top);
			Assert.Equal(34, result.Bottom);
		}

		[Fact]
		public void ComputeSafeArea_Automatic_HorizontalScroll_LandscapeLeft_UsesAciForHorizontal()
		{
			// For horizontal scroll views, UIKit DOES include L/R in ACI under Automatic mode.
			// MAUI should therefore read L/R from ACI (UIKit-owned) and T/B from deviceInset (MAUI-owned).
			// aci.Left = 44 because UIKit populated it; device.Left = 44 (notch) — equal, so result is the same value.
			// The key is that T/B are device-sourced, not ACI-sourced (UIKit doesn't populate T/B for horizontal scroll).
			var aci = new UIEdgeInsets(top: 0, left: 44, bottom: 0, right: 0); // UIKit adds L/R for horizontal
			var deviceInset = new UIEdgeInsets(top: 20, left: 44, bottom: 0, right: 0);

			var result = MauiScrollView.ComputeSafeArea(
				aci,
				UIScrollViewContentInsetAdjustmentBehavior.Automatic,
				deviceInset,
				isHorizontalScroll: true);

			// L/R come from ACI (UIKit-owned for horizontal scroll)
			Assert.Equal(44, result.Left);
			Assert.Equal(0, result.Right);
			// T/B come from deviceInset (MAUI-owned; UIKit doesn't apply T/B to ACI for horizontal scroll)
			Assert.Equal(20, result.Top);
			Assert.Equal(0, result.Bottom);
		}

		[Fact]
		public void ComputeSafeArea_Automatic_VerticalScroll_AciTopNotDoubledWithDeviceTop()
		{
			// Regression guard: for vertical scroll in Automatic mode, T/B come from ACI (normAci),
			// NOT from deviceInset. Previously a raw-(double) cast was used; confirm ToSafeAreaInsets()
			// normalization is now applied consistently (values within tolerance become exactly 0).
			const double noise = 3.5e-15; // representative UIKit floating-point noise
			var aci = new UIEdgeInsets(top: (nfloat)(44 + noise), left: 0, bottom: 0, right: 0);
			var deviceInset = new UIEdgeInsets(top: 44, left: 0, bottom: 0, right: 0);

			var result = MauiScrollView.ComputeSafeArea(
				aci,
				UIScrollViewContentInsetAdjustmentBehavior.Automatic,
				deviceInset);

			// Top must be exactly 44, not 44 + 3.5e-15 — normalization suppresses the noise
			Assert.Equal(44, result.Top);
			Assert.Equal(0, result.Left);
		}
	}
}

