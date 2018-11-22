using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 4484, "[Android] ImageButton inside NavigationView.TitleView throw exception during device rotation",
		PlatformAffected.Android)]
#if UITEST
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
						new Button(){ Image = "bank", AutomationId="bank"},
						new Image(){Source = "bank"},
						new ImageButton{Source = "bank"}
					}
				});

			page.Content = new StackLayout()
			{
				Children =
				{
					new Label()
					{
						Text = "Rotate device. If it doesn't crash then test has passed",
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
