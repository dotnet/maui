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
	[Issue(IssueTracker.Github, 12652, "[Bug] NullReferenceException in the Shell on UWP when navigating back to Shell Section with multiple content items",
		PlatformAffected.UWP)]
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github10000)]
	[NUnit.Framework.Category(UITestCategories.Shell)]
#endif
	public class Issue12652 : TestShell
	{
		protected override void Init()
		{
			AddBottomTab("Main 1")
				.Content = new StackLayout()
				{
					Children =
					{
						new Label()
						{
							Text = @"Click on the tabs in the following order
									Top 3,
									Main 2,
									Main 1,
									Top 3,
									Main 2.
									If nothing crashes test has passed.",
							AutomationId = "TopTabPage2"
						}
					}
				};

			AddBottomTab("Main 2")
				.Content = new StackLayout()
				{
					Children =
					{
						new Label()
						{
							Text = "Hello From Page 2",
							AutomationId = "TopTabPage2"
						}
					}
				};


			AddTopTab("Top 2");

			AddTopTab("Top 3")
				.Content = new StackLayout()
				{
					Children =
					{
						new Label()
						{
							Text = "Hello From Page 3",
							AutomationId = "TopTabPage3"
						}
					}
				};
		}


#if UITEST
		[Test]
		public void NavigatingBackToAlreadySelectedTopTabDoesntCrash()
		{
			var location = RunningApp.WaitForElement("Top 3")[0];
			RunningApp.TapCoordinates(location.Rect.CenterX, location.Rect.CenterY);
			RunningApp.WaitForElement("TopTabPage3");
			RunningApp.Tap("Main 2");
			RunningApp.WaitForElement("TopTabPage2");
			RunningApp.Tap("Main 1");

			RunningApp.TapCoordinates(location.Rect.CenterX, location.Rect.CenterY);
			RunningApp.WaitForElement("TopTabPage3");
			RunningApp.Tap("Main 2");
			RunningApp.WaitForElement("TopTabPage2");
			RunningApp.Tap("Main 1");
			RunningApp.TapCoordinates(location.Rect.CenterX, location.Rect.CenterY);
			RunningApp.WaitForElement("TopTabPage3");
		}
#endif
	}
}
