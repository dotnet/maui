using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foundation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;
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
		[Fact(DisplayName = "Flyout Page Takes Into Account Safe Area by Default")]
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
				await AssertionExtensions.Wait(() => flyoutLabel.ToPlatform().GetLocationOnScreen().Y > 1);
				var flyoutLocation = flyoutLabel.ToPlatform().GetLocationOnScreen();
				Assert.True(Math.Abs(offset - flyoutLocation.Y) < 1.0);
			});
		}
#endif

		[Fact]
		public async Task DetailsViewLayoutIsCorrectForIdiom()
		{
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
				}
			});

			await CreateHandlerAndAddToWindow<FlyoutViewHandler>(flyoutPage, async (handler) =>
			{
				var detailBounds = flyoutPage.Detail.GetPlatformViewBounds();
				var flyoutBounds = flyoutPage.Flyout.GetPlatformViewBounds();

				if (IsPad)
				{
					Assert.Equal(0, detailBounds.X);
					Assert.Equal(0, flyoutBounds.X);
				}
				else
				{
					Assert.Equal(flyoutBounds.Width, detailBounds.X);
					Assert.Equal(0, flyoutBounds.X);
				}

				flyoutPage.IsPresented = false;

				await Task.Yield();
				await AssertionExtensions.Wait(() =>
				{
					var flyoutBounds = flyoutPage.Flyout.GetPlatformViewBounds();
					return -flyoutBounds.Width == flyoutBounds.X;
				});

				var detailBoundsNotPresented = flyoutPage.Detail.GetPlatformViewBounds();
				var flyoutBoundsNotPresented = flyoutPage.Flyout.GetPlatformViewBounds();

				if (IsPad)
				{
					Assert.Equal(detailBoundsNotPresented, detailBounds);
				}

				// ensure flyout is off screen
				Assert.Equal(-flyoutBoundsNotPresented.Width, flyoutBoundsNotPresented.X);
			});
		}


		UIView FindPlatformFlyoutView(UIView uiView) =>
			uiView.FindResponder<PhoneFlyoutPageRenderer>()?.View;
	}
}