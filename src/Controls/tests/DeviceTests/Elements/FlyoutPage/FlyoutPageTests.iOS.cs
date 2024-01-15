using System;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

#if IOS || MACCATALYST
using FlyoutViewHandler = Microsoft.Maui.Controls.Handlers.Compatibility.PhoneFlyoutPageRenderer;
#endif

namespace Microsoft.Maui.DeviceTests
{
	[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
	public partial class FlyoutPageTests
	{
		bool IsPad => UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad;

#if MACCATALYST
		[Fact(DisplayName = "Flyout Page Takes Into Account Safe Area by Default"
#if MACCATALYST
			, Skip = "Fails on Mac Catalyst, fixme"
#endif
		)]
		public async Task FlyoutPageTakesIntoAccountSafeAreaByDefault()
		{
			SetupBuilder();
			var flyoutLabel = new Label() { Text = "Content" };
			var flyoutPage = await InvokeOnMainThreadAsync(() => new FlyoutPage()
			{
				FlyoutLayoutBehavior = FlyoutLayoutBehavior.Split,
				Detail = new ContentPage()
				{
					Title = "Detail",
					Content = new Label()
				},
				Flyout = new ContentPage()
				{
					Title = "Flyout",
					Content = flyoutLabel
				}
			});

			await CreateHandlerAndAddToWindow<PhoneFlyoutPageRenderer>(flyoutPage, async (handler) =>
			{
				var offset = (float)UIApplication.SharedApplication.GetSafeAreaInsetsForWindow().Top;
				await AssertEventually(() => flyoutLabel.ToPlatform().GetLocationOnScreen().Y > 1);
				var flyoutLocation = flyoutLabel.ToPlatform().GetLocationOnScreen();
				Assert.True(Math.Abs(offset - flyoutLocation.Y) < 1.0);
			});
		}
#endif

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task DetailsViewPopOverLayoutIsCorrectForIdiom(bool isRtl)
		{
			SetupBuilder();
			var flyoutLabel = new Label() { Text = "Content" };
			var flyoutLayout = new VerticalStackLayout() { BackgroundColor = Colors.Blue };
			flyoutLayout.Add(flyoutLabel);

			var flyoutPage = await InvokeOnMainThreadAsync(() => new FlyoutPage()
			{
				FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover,
				IsPresented = true,
				Detail = new ContentPage()
				{
					Title = "Detail",
					Content = new Label() { Text = "Detail", BackgroundColor = Colors.Red }
				},
				Flyout = new ContentPage()
				{
					Title = "Flyout",
					Content = flyoutLayout
				},
				FlowDirection = isRtl ? FlowDirection.RightToLeft : FlowDirection.LeftToRight
			});

			await CreateHandlerAndAddToWindow<FlyoutViewHandler>(flyoutPage, async (handler) =>
			{
				await AssertEventually(() => flyoutPage.Flyout.GetBoundingBox().Width > 0);
				var screenBounds = flyoutPage.GetBoundingBox();
				var detailBounds = flyoutPage.Detail.GetBoundingBox();
				var flyoutBounds = flyoutPage.Flyout.GetBoundingBox();

				// When used on an iPad the flyout overlaps the details
				if (IsPad)
				{
					Assert.Equal(0, detailBounds.X);
				}
				else if (isRtl)
				{
					Assert.Equal(-flyoutBounds.Width, detailBounds.X);
				}
				else
				{
					Assert.Equal(flyoutBounds.Width, detailBounds.X);
				}

				if (isRtl)
					Assert.Equal(screenBounds.Width - flyoutBounds.Width, flyoutBounds.X);
				else
					Assert.Equal(0, flyoutBounds.X);

				await CloseFlyout(flyoutPage);

				var detailBoundsNotPresented = flyoutPage.Detail.GetBoundingBox();
				var flyoutBoundsNotPresented = flyoutPage.Flyout.GetBoundingBox();

				if (IsPad)
				{
					Assert.Equal(detailBoundsNotPresented, detailBounds);

					if (isRtl)
						Assert.Equal(screenBounds.Width, flyoutBoundsNotPresented.X);
					else
						Assert.Equal(-flyoutBoundsNotPresented.Width, flyoutBoundsNotPresented.X);
				}
				else
				{
					Assert.Equal(0, detailBoundsNotPresented.X);
				}
			});
		}

		UIView FindPlatformFlyoutView(UIView uiView) =>
			uiView.FindResponder<PhoneFlyoutPageRenderer>()?.View;

		async Task CloseFlyout(FlyoutPage flyoutPage)
		{
			flyoutPage.IsPresented = false;

			await Task.Yield();

			bool flyoutHasExpectedBounds()
			{
				if (IsPad)
				{
					// When used on an iPad the flyout overlaps the details
					var flyoutBounds = flyoutPage.Flyout.GetBoundingBox();
					var screenBounds = flyoutPage.GetBoundingBox();
					return
						-flyoutBounds.Width == flyoutBounds.X || //ltr
						screenBounds.Width == flyoutBounds.X;    //rtl
				}
				else
				{
					// When used on an iPhone the details page just covers the flyout
					// When the flyout opens the details page is moved to the right
					var detailsBound = flyoutPage.Detail.GetBoundingBox();
					return 0 == detailsBound.X;
				}
			}

			await AssertEventually(flyoutHasExpectedBounds);
		}
	}
}