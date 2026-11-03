using System;
using System.Threading.Tasks;
using AndroidX.DrawerLayout.Widget;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Shell)]
	public partial class ShellTests
	{
		[Fact]
		public async Task FlyoutContentResizesWhenFooterGrowsAndShrinks()
		{
			const double warningHeight = 200;
			const double tolerance = 1;
			var endMarker = new Label { Text = "End of flyout content", HeightRequest = 48 };
			var content = new VerticalStackLayout { Spacing = 0 };
			for (int index = 0; index < 40; index++)
				content.Add(new Label { Text = $"Flyout item {index}", HeightRequest = 48 });
			content.Add(endMarker);

			var scrollView = new ScrollView { Content = content };
			var warning = new Label { Text = "Warning", HeightRequest = warningHeight, IsVisible = false };
			var footer = new VerticalStackLayout
			{
				Spacing = 0,
				Children = { warning, new Label { Text = "Footer", HeightRequest = 48 } }
			};
			var contentTemplate = new DataTemplate(() => scrollView);
			var footerTemplate = new DataTemplate(() => footer);

			await RunShellTest(shell =>
			{
				shell.FlyoutContentTemplate = contentTemplate;
				shell.FlyoutFooterTemplate = footerTemplate;
			}, async shell =>
			{
				var shellContext = (IShellContext)shell.Handler;
				await OpenFlyout(shellContext);
				await OnLoadedAsync(scrollView);
				await OnLoadedAsync(footer);
				await AssertionExtensions.AssertEventually(() =>
					scrollView.Height > 0 && footer.Height > 0 && endMarker.Height > 0
					&& scrollView.ContentSize.Height > scrollView.Height
					&& Math.Abs(scrollView.ContentSize.Height - content.Height) <= tolerance);

				var platformScrollView = Assert.IsType<MauiScrollView>(scrollView.ToPlatform());
				var initialViewport = GetFrameRelativeToFlyout(shellContext, scrollView);
				var initialFooter = GetFrameRelativeToFlyout(shellContext, footer);
				Assert.True(content.Height > initialViewport.Height, "Content must require scrolling.");
				await AssertBottomReachable();

				for (int cycle = 0; cycle < 3; cycle++)
				{
					warning.IsVisible = true;
					await AssertViewport(warningHeight);
					await AssertBottomReachable();

					warning.IsVisible = false;
					await AssertViewport(0);
					await AssertBottomReachable();
				}

				Assert.Same(contentTemplate, shell.FlyoutContentTemplate);
				Assert.Same(footerTemplate, shell.FlyoutFooterTemplate);
				Assert.Same(platformScrollView, scrollView.ToPlatform());

				async Task AssertViewport(double heightChange)
				{
					await AssertionExtensions.AssertEventually(() =>
					{
						var viewport = GetFrameRelativeToFlyout(shellContext, scrollView);
						var footerFrame = GetFrameRelativeToFlyout(shellContext, footer);
						return Math.Abs(footerFrame.Height - initialFooter.Height - heightChange) <= tolerance
							&& Math.Abs(viewport.Height - initialViewport.Height + heightChange) <= tolerance
							&& Math.Abs(viewport.Bottom - footerFrame.Top) <= tolerance
							&& Math.Abs(viewport.Top - initialViewport.Top) <= tolerance
							&& Math.Abs(footerFrame.Bottom - initialFooter.Bottom) <= tolerance;
					}, message: $"Flyout viewport did not follow the footer height change of {heightChange}.");
				}

				async Task AssertBottomReachable()
				{
					await scrollView.ScrollToAsync(0, 0, false);
					await AssertionExtensions.AssertEventually(() => platformScrollView.ScrollY == 0);
					// Clamp to the native scroll limit rather than relying on element-relative ScrollTo positioning.
					await scrollView.ScrollToAsync(0, scrollView.ContentSize.Height, false);
					await AssertionExtensions.AssertEventually(() =>
						platformScrollView.ScrollY > 0 && !platformScrollView.CanScrollVertically(1),
						message: "Flyout content did not reach the native bottom scroll limit.");

					var viewport = GetFrameRelativeToFlyout(shellContext, scrollView);
					var marker = GetFrameRelativeToFlyout(shellContext, endMarker);
					var footerFrame = GetFrameRelativeToFlyout(shellContext, footer);
					Assert.True(marker.Top >= viewport.Top - tolerance,
						$"The last item is clipped at the top: marker={marker}, viewport={viewport}.");
					Assert.True(Math.Abs(marker.Bottom - viewport.Bottom) <= tolerance,
						$"The last item must align with the viewport bottom without a gap: marker={marker}, viewport={viewport}, nativePaddingBottom={platformScrollView.PaddingBottom}.");
					Assert.True(marker.Bottom <= footerFrame.Top + tolerance,
						$"The footer obscures the last item: marker={marker}, footer={footerFrame}.");
				}
			});
		}

		[Fact(DisplayName = "FlyoutIsPresented=true sets the visible status of the Shell Flyout.")]
		public async Task FlyoutIsPresentedOpenDrawer()
		{
			await RunShellTest(shell =>
			{
				shell.FlyoutContent = new VerticalStackLayout() { new Label() { Text = "Flyout Content" } };
			},
			async shell =>
			{
#if ANDROID || IOS || MACCATALYST
				var shellContext = (IShellContext)shell.Handler;
#endif
				// 1. Set FlyoutIsPresented=true to make the Shell Flyout visible.
				shell.FlyoutIsPresented = true;

				var dl = GetDrawerLayout(shellContext) as DrawerLayout;
				Assert.NotNull(dl);

				await AssertionExtensions.AssertEventually(() =>
				{
					// 2. Check that the Flyout has size.
					var flyoutFrame = GetFlyoutFrame(shellContext);
					return flyoutFrame.Width > 0 && flyoutFrame.Height > 0 && dl.IsOpen;
				});
			});
		}
	}
}
