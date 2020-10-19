using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
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
						FlyoutHeaderTemplate = new DataTemplate(() => new Label() { Text = "Header Template" });
						FlyoutFooterTemplate = new DataTemplate(() => new Label() { Text = "Footer Template" });
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
						FlyoutHeader = new Label() { Text = "Header View" };
						FlyoutFooter = new Label() { Text = "Footer View" };
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
					headerLabel.BackgroundColor = Color.LightBlue;
					footerLabel.BackgroundColor = Color.LightBlue;

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
		}


#if UITEST

		[Test]
		public void FlyoutTests()
		{
			RunningApp.WaitForElement("PageLoaded");
			ShowFlyout();

			// Verify Header an Footer show up at all
			OpenFlyout("ToggleHeaderFooter");
			RunningApp.WaitForElement("Header View");
			RunningApp.WaitForElement("Footer View");

			// Verify Template takes priority over header footer
			OpenFlyout("ToggleHeaderFooterTemplate");
			RunningApp.WaitForElement("Header Template");
			RunningApp.WaitForElement("Footer Template");
			RunningApp.WaitForNoElement("Header View");
			RunningApp.WaitForNoElement("Footer View");

			// Verify turning off Template shows Views again
			OpenFlyout("ToggleHeaderFooterTemplate");
			RunningApp.WaitForElement("Header View");
			RunningApp.WaitForElement("Footer View");
			RunningApp.WaitForNoElement("Header Template");
			RunningApp.WaitForNoElement("Footer Template");

			// Verify turning off header/footer clear out views correctly
			OpenFlyout("ToggleHeaderFooter");
			RunningApp.WaitForNoElement("Header Template");
			RunningApp.WaitForNoElement("Footer Template");
			RunningApp.WaitForNoElement("Header View");
			RunningApp.WaitForNoElement("Footer View");

			// verify header and footer react to size changes
			OpenFlyout("ResizeHeaderFooter");
			var headerSizeSmall = RunningApp.WaitForElement("Header View")[0].Rect;
			var footerSizeSmall = RunningApp.WaitForElement("Footer View")[0].Rect;
			OpenFlyout("ResizeHeaderFooter");
			var headerSizeLarge = RunningApp.WaitForElement("Header View")[0].Rect;
			var footerSizeLarge = RunningApp.WaitForElement("Footer View")[0].Rect;

			OpenFlyout("ResizeHeaderFooter");
			var headerSizeSmall2 = RunningApp.WaitForElement("Header View")[0].Rect;
			var footerSizeSmall2 = RunningApp.WaitForElement("Footer View")[0].Rect;

			Assert.Greater(headerSizeLarge.Height, headerSizeSmall.Height);
			Assert.Greater(footerSizeLarge.Height, footerSizeSmall.Height);
			Assert.AreEqual(headerSizeSmall2.Height, headerSizeSmall.Height);
			Assert.AreEqual(footerSizeSmall2.Height, footerSizeSmall.Height);
		}

		void OpenFlyout(string text)
		{
			RunningApp.Tap(text);

#if __WINDOWS__
			// UWP closes the flyout after selecting an item
			System.Threading.Thread.Sleep(1000);
			ShowFlyout();
#endif
		}


#endif
	}
}
