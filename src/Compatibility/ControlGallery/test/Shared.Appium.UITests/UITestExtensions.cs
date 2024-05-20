using System.Drawing;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace UITests
{
	public static class UITestExtensions
	{
		const string GoToTestButtonId = "GoToTestButton";

		public static void Back(this UITestContextBase testBase)
		{
			if (testBase.Device == TestDevice.Android)
			{
				var query = testBase.App.Query.ByAccessibilityId("Navigate up").First();
				query.Click();
			}
			else if (testBase.Device == TestDevice.iOS || testBase.Device == TestDevice.Mac)
			{
				// Get the first NavigationBar we can find and the first button in it (the back button), index starts at 1
				var queryBy = testBase.App.Query.ByClass("XCUIElementTypeNavigationBar").First().ByClass("XCUIElementTypeButton").First();
				queryBy.Click();
			}
			else
			{
				testBase.RunningApp.FindElement("NavigationViewBackButton").Click();
			}
		}

		public static void NavigateToGallery(this IApp app, string page)
		{
			app.WaitForElement(GoToTestButtonId, "Timed out waiting for Go To Test button to appear", TimeSpan.FromMinutes(2));
			NavigateTo(app, page);
		}

		public static void NavigateTo(this IApp app, string text)
		{
			app.WaitForElement("SearchBar");
			app.ClearText("SearchBar");
			if (!string.IsNullOrWhiteSpace(text))
			{
				app.EnterText("SearchBar", text);
			}
			app.Tap(GoToTestButtonId);

			app.WaitForNoElement(GoToTestButtonId, "Timed out waiting for Go To Test button to disappear", TimeSpan.FromMinutes(1));
		}

		public static void NavigateToIssues(this IApp app)
		{
			app.WaitForElement(GoToTestButtonId, "Timed out waiting for Go To Test button to appear", TimeSpan.FromMinutes(2));

			app.WaitForElement("SearchBar");
			app.ClearText("SearchBar");

			app.Tap(GoToTestButtonId);
			app.WaitForElement("TestCasesIssueList");
		}

		public static void IgnoreIfPlatforms(this UITestBase? test, IEnumerable<TestDevice> devices, string? message = null)
		{
			foreach (var device in devices)
			{
				test?.IgnoreIfPlatform(device, message);
			}
		}

		public static void IgnoreIfPlatform(this UITestBase? test, TestDevice device, string? message = null)
		{
			if (test != null && test.Device == device)
			{
				if (string.IsNullOrEmpty(message))
					Assert.Ignore();
				else
					Assert.Ignore(message);
			}
		}

		public static int CenterX(this Rectangle rect)
		{
			return rect.X + rect.Width / 2;
		}

		public static int CenterY(this Rectangle rect)
		{
			return rect.Y + rect.Height / 2;
		}
	}
}