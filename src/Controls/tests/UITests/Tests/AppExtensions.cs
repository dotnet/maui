using System.Text.RegularExpressions;
using OpenQA.Selenium.Support.UI;
using TestUtils.Appium.UITests;
using Xamarin.UITest;

namespace Microsoft.Maui.AppiumTests
{
	internal static class AppExtensions
	{
		const string goToTestButtonId = "GoToTestButton";

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

		public static bool WaitForTextToBePresentInElement(this IApp app, string automationId, string text)
		{
			if (app is IApp2 app2)
			{
				return app2.WaitForTextToBePresentInElement(automationId, text);
			}

			throw new InvalidOperationException("Not supported on IApp");
		}

		public static void NavigateToGallery(this IApp app, string page)
		{
			app.WaitForElement(goToTestButtonId, "Timed out waiting for Go To Test button to appear", TimeSpan.FromMinutes(2));
			var text = Regex.Match(page, "'(?<text>[^']*)'").Groups["text"].Value;
			NavigateTo(app, text);
		}

		public static void NavigateToIssues(this IApp app)
		{
			app.WaitForElement(goToTestButtonId, "Timed out waiting for Go To Test button to appear", TimeSpan.FromMinutes(2));

			app.WaitForElement("SearchBar");
			app.ClearText("SearchBar");

			app.Tap(goToTestButtonId);
			app.WaitForElement("TestCasesIssueList");
		}

		public static void NavigateBack(this IApp app)
		{
			app.Back();
		}
	}
}