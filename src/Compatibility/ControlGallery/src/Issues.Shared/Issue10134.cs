using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.Shell)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 10134, "Shell Top Tabbar focus issue", PlatformAffected.iOS)]
	public class Issue10134 : TestShell
	{
		protected override void Init()
		{
			ContentPage page1 = AddTopTab("Tab 1");
			page1.Title = "Top Bar Page 1";

			for (int i = 2; i < 20; i++)
			{
				AddTopTab($"Tab {i}");
			}

			page1.Content =
				new StackLayout()
				{
					Children =
					{
						new Label()
						{
							Text = "Scroll and click on any of the currently non visible tabs. After clicking, if the Top Tabs don't scroll back to the beginninig the test has passed"
						}
					}
				};
		}

#if UITEST && __SHELL__
		[Test]
		[Compatibility.UITests.FailsOnMauiIOS]
		public void TopTabsDontScrollBackToStartWhenSelected() 
		{
			var element1 = RunningApp.WaitForElement("Tab 1", "Shell hasn't loaded")[0].Rect;
			RunningApp.WaitForNoElement("Tab 12", "Tab shouldn't be visible");

			Xamarin.UITest.Queries.AppRect element2 = element1;

			for (int i = 2; i < 20; i++)
			{
				var results = RunningApp.Query($"Tab {i}");

				if (results.Length == 0)
					break;

				element2 = results[0].Rect;
			}

			RunningApp.DragCoordinates(element2.CenterX, element2.CenterY, element1.CenterX, element1.CenterY);

			RunningApp.WaitForNoElement("Tab 1");
			bool testPassed = false;

			// figure out what tabs are visible
			for (int i = 20; i > 1; i--)
			{
				var results = RunningApp.Query($"Tab {i}");

				if (results.Length > 0)
				{
					RunningApp.Tap($"Tab {i}");
					RunningApp.WaitForElement($"Tab {i}");
					testPassed = true;
					break;
				}
			}

			RunningApp.WaitForNoElement("Tab 1");
			Assert.IsTrue(testPassed);
		}
#endif

	}
}
