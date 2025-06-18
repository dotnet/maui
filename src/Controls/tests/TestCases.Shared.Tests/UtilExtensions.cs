using System.Drawing;
using NUnit.Framework;
using UITest.Appium;
using UITest.Appium.NUnit;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
	public static class UtilExtensions
	{
		const string goToTestButtonId = "GoToTestButton";

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
				testBase.App.FindElement("NavigationViewBackButton").Click();
			}
		}

		public static void NavigateToGallery(this IApp app, string page)
		{
			app.WaitForElement(goToTestButtonId, "Timed out waiting for Go To Test button to appear", TimeSpan.FromMinutes(2));
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
			app.Tap(goToTestButtonId);

			app.WaitForNoElement(goToTestButtonId, "Timed out waiting for Go To Test button to disappear", TimeSpan.FromMinutes(1));
		}

		public static int CenterX(this Rectangle rect)
		{
			return rect.X + rect.Width / 2;
		}

		public static int CenterY(this Rectangle rect)
		{
			return rect.Y + rect.Height / 2;
		}

		public static void AssertMemoryTest(this IApp app)
		{
			try
			{
				app.WaitForElement("Success", timeout: TimeSpan.FromSeconds(10));
			}
			catch
			{
				var failure = app.FindElement("Failed")?.GetText();
				if (failure is not null)
				{
					Assert.Fail(failure);
				}

				throw;
			}
		}
	}
}