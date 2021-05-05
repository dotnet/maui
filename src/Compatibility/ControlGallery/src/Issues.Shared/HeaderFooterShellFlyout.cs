using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Graphics;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.None, 0, "Shell Flyout Header Footer",
		PlatformAffected.All)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.Shell)]
#endif
	public class HeaderFooterShellFlyout : TestShell
	{
		protected override void Init()
		{
			var page = new ContentPage();

			AddFlyoutItem(page, "Flyout Item");
			page.Content = new StackLayout()
			{
				Children =
				{
					new Label()
					{
						Text = "Open the Flyout and Toggle the Header and Footer. If it changes after each click test has passed",
						AutomationId = "PageLoaded"
					}
				}
			};

			Items.Add(new MenuItem()
			{
				Text = "Toggle Header/Footer Template",
				Command = new Command(() =>
				{
					if (FlyoutHeaderTemplate == null)
					{
						FlyoutHeaderTemplate = new DataTemplate(() =>
						{
							return new Label() { Text = "Header Template" };
						});

						FlyoutFooterTemplate = new DataTemplate(() =>
						{
							return new Label() { Text = "Footer Template" };
						});
					}
					else if (FlyoutHeaderTemplate != null)
					{
						FlyoutHeaderTemplate = null;
						FlyoutFooterTemplate = null;
					}
				}),
				AutomationId = "ToggleHeaderFooterTemplate"
			});

			Items.Add(new MenuItem()
			{
				Text = "Toggle Header/Footer View",
				Command = new Command(() =>
				{
					if (FlyoutHeader != null)
					{
						FlyoutHeader = null;
						FlyoutFooter = null;
					}
					else
					{
						FlyoutHeader = new StackLayout()
						{
							Children = {
								new Label() { Text = "Header" }
							},
							AutomationId = "Header View"
						};

						FlyoutFooter = new StackLayout()
						{
							Orientation = StackOrientation.Horizontal,
							Children = {
								new Label() { Text = "Footer" }
							},
							AutomationId = "Footer View"
						};
					}
				}),
				AutomationId = "ToggleHeaderFooter"
			});

			Items.Add(new MenuItem()
			{
				Text = "Resize Header/Footer",
				Command = new Command(async () =>
				{
					FlyoutHeaderTemplate = null;
					FlyoutFooterTemplate = null;
					if (FlyoutHeader == null)
					{
						FlyoutHeader = new StackLayout()
						{
							Children = {
								new Label() { Text = "Header" }
							},
							AutomationId = "Header View"
						};

						FlyoutFooter = new StackLayout()
						{
							Children = {
								new Label() { Text = "Footer" }
							},
							AutomationId = "Footer View"
						};

						await Task.Delay(10);
					}

					var headerLabel = (VisualElement)FlyoutHeader;
					var footerLabel = (VisualElement)FlyoutFooter;
					headerLabel.BackgroundColor = Colors.LightBlue;
					footerLabel.BackgroundColor = Colors.LightBlue;

					if (headerLabel.HeightRequest == 60)
					{
						headerLabel.HeightRequest = 200;
						footerLabel.HeightRequest = 200;
					}
					else
					{
						headerLabel.HeightRequest = 60;
						footerLabel.HeightRequest = 60;
					}
				}),
				AutomationId = "ResizeHeaderFooter"
			});

			if (Device.RuntimePlatform == Device.iOS)
			{
				Items.Add(new MenuItem()
				{
					Text = "Zero Margin Header Test",
					Command = new Command(() =>
					{
						FlyoutHeader =
							new StackLayout()
							{
								AutomationId = "ZeroMarginLayout",
								Margin = 0,
								Children =
								{
								new Label() { Text = "Header View" }
								}
							};

						FlyoutHeaderTemplate = null;
						FlyoutBehavior = FlyoutBehavior.Locked;
					}),
					AutomationId = "ZeroMarginHeader"
				});
			}
		}


#if UITEST

#if __IOS__
		[Test]
		public void FlyoutHeaderWithZeroMarginShouldHaveNoY()
		{
			RunningApp.WaitForElement("PageLoaded");
			this.TapInFlyout("ZeroMarginHeader", makeSureFlyoutStaysOpen: true);
			var layout = RunningApp.WaitForElement("ZeroMarginLayout")[0].Rect.Y;
			Assert.AreEqual(0, layout);
		}
#endif

		[Test]
		public void FlyoutTests()
		{
			RunningApp.WaitForElement("PageLoaded");

			// Verify Header an Footer show up at all
			TapInFlyout("ToggleHeaderFooter", makeSureFlyoutStaysOpen: true);
			RunningApp.WaitForElement("Header View");
			RunningApp.WaitForElement("Footer View");

			// Verify Template takes priority over header footer
			TapInFlyout("ToggleHeaderFooterTemplate", makeSureFlyoutStaysOpen: true);
			RunningApp.WaitForElement("Header Template");
			RunningApp.WaitForElement("Footer Template");
			RunningApp.WaitForNoElement("Header View");
			RunningApp.WaitForNoElement("Footer View");

			// Verify turning off Template shows Views again
			TapInFlyout("ToggleHeaderFooterTemplate", makeSureFlyoutStaysOpen: true);
			RunningApp.WaitForElement("Header View");
			RunningApp.WaitForElement("Footer View");
			RunningApp.WaitForNoElement("Header Template");
			RunningApp.WaitForNoElement("Footer Template");

			// Verify turning off header/footer clear out views correctly
			TapInFlyout("ToggleHeaderFooter", makeSureFlyoutStaysOpen: true);
			RunningApp.WaitForNoElement("Header Template");
			RunningApp.WaitForNoElement("Footer Template");
			RunningApp.WaitForNoElement("Header View");
			RunningApp.WaitForNoElement("Footer View");

			// verify header and footer react to size changes
			TapInFlyout("ResizeHeaderFooter", makeSureFlyoutStaysOpen: true);
			var headerSizeSmall = RunningApp.WaitForElement("Header View")[0].Rect;
			var footerSizeSmall = RunningApp.WaitForElement("Footer View")[0].Rect;
			TapInFlyout("ResizeHeaderFooter", makeSureFlyoutStaysOpen: true);
			var headerSizeLarge = RunningApp.WaitForElement("Header View")[0].Rect;
			var footerSizeLarge = RunningApp.WaitForElement("Footer View")[0].Rect;

			TapInFlyout("ResizeHeaderFooter", makeSureFlyoutStaysOpen: true);
			var headerSizeSmall2 = RunningApp.WaitForElement("Header View")[0].Rect;
			var footerSizeSmall2 = RunningApp.WaitForElement("Footer View")[0].Rect;

			Assert.Greater(headerSizeLarge.Height, headerSizeSmall.Height);
			Assert.Greater(footerSizeLarge.Height, footerSizeSmall.Height);
			Assert.AreEqual(headerSizeSmall2.Height, headerSizeSmall.Height);
			Assert.AreEqual(footerSizeSmall2.Height, footerSizeSmall.Height);
		}
#endif
	}
}
