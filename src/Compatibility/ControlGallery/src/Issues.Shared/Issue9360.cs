using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Devices;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 9360, "[Bug] Android Icons no longer customizable via NavigationPageRenderer UpdateMenuItemIcon()",
		PlatformAffected.Android)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.ListView)]
#endif
	public class Issue9360 : TestContentPage
	{

		public class Issue9360NavigationPage : TestNavigationPage
		{
			ContentPage CreateNewPage()
			{
				string text = "This Test is only Relevant on Android";

				if (DeviceInfo.Platform == DevicePlatform.Android)
					text = "Toolbar Item Icon should be a hear";

				ContentPage contentPage = new ContentPage()
				{
					Content = new StackLayout()
					{
						Children =
						{
							new Label() {
								Text = text
							},
							new Button()
							{
								Text = "Push Same Page To see if Icons all load correctly a second time",
								Command = new Command(async () =>
								{
									await PushAsync(CreateNewPage());
								})
							}
						}
					}
				};

				contentPage.ToolbarItems.Add(new ToolbarItem() { Text = "BAD" });
				contentPage.ToolbarItems.Add(new ToolbarItem()
				{
					IconImageSource = ImageSource.FromResource("Microsoft.Maui.Controls.ControlGallery.GalleryPages.crimson.jpg", typeof(Issue9360NavigationPage).Assembly)
				});

				contentPage.ToolbarItems.Add(new ToolbarItem()
				{
					Text = "second",
					Command = new Command(() =>
					{
						contentPage.ToolbarItems[0].IsEnabled = !contentPage.ToolbarItems[0].IsEnabled;
						contentPage.ToolbarItems[2].IconImageSource = "coffee.png";
					})
				});

				return contentPage;
			}

			protected override void Init()
			{

				PushAsync(CreateNewPage());
			}
		}

		protected override void Init()
		{
			Navigation.PushModalAsync(new Issue9360NavigationPage());
		}

#if UITEST && __ANDROID__
[Microsoft.Maui.Controls.Compatibility.UITests.FailsOnMauiAndroid]
		[Test]
		public void NavigationPageRendererMenuItemIconOverrideWorks()
		{
			RunningApp.WaitForElement("HEART");
		}
#endif
	}
}
