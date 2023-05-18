using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 4484, "[Android] ImageButton inside NavigationView.TitleView throw exception during device rotation",
		PlatformAffected.Android)]
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
	[NUnit.Framework.Category(UITestCategories.Image)]
#endif
	public class Issue4484 : TestNavigationPage
	{
		protected override void Init()
		{
			ContentPage page = new ContentPage();

			NavigationPage.SetTitleView(page,
				new StackLayout()
				{
					Orientation = StackOrientation.Horizontal,
					Children =
					{
						new Button(){ ImageSource = "bank.png", AutomationId="bank"},
						new Image(){Source = "bank.png"},
						new ImageButton{Source = "bank.png"}
					}
				});

			page.Content = new StackLayout()
			{
				Children =
				{
					new Label()
					{
						Text = "You should see 3 images. Rotate device. If it doesn't crash then test has passed",
						AutomationId = "Instructions"
					}
				}
			};

			PushAsync(page);
		}

#if UITEST
		[Test]
		public void RotatingDeviceDoesntCrashTitleView()
		{
			RunningApp.WaitForElement("Instructions");
			RunningApp.SetOrientationLandscape();
			RunningApp.WaitForElement("Instructions");
			RunningApp.SetOrientationPortrait();
			RunningApp.WaitForElement("Instructions");
		}
#endif
	}
}
