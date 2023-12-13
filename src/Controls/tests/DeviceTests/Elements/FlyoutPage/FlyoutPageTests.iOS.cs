using System;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Compatibility;
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
			if (isRtl && System.OperatingSystem.IsIOSVersionAtLeast(17))
			{
				//skip till we figure the 1 pixel issue 
				return;
			}
			SetupBuilder();
			var flyoutLabel = new Label() { Text = "Content" };
			var flyoutPage = await InvokeOnMainThreadAsync(() => new FlyoutPage()
			{
				FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover,
				IsPresented = true,
				Detail = new ContentPage()
				{
					Title = "Detail",
					Content = new Label()
				},
				Flyout = new ContentPage()
				{
					Title = "Flyout",
					Content = flyoutLabel
				},
				FlowDirection = (isRtl) ? FlowDirection.RightToLeft : FlowDirection.LeftToRight
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

				flyoutPage.IsPresented = false;

				await Task.Yield();

				bool flyoutHasExpectedBounds()
				{
					var flyoutBounds = flyoutPage.Flyout.GetBoundingBox();
					return
						-flyoutBounds.Width == flyoutBounds.X || //ltr
						screenBounds.Width == flyoutBounds.X;    //rtl
				}

				await AssertEventually(flyoutHasExpectedBounds);

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
	}
}