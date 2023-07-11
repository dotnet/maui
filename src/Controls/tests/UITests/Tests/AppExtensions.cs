using System.Text.RegularExpressions;
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
			var timeout = TimeSpan.FromSeconds(15);
			TimeSpan retryFrequency = TimeSpan.FromMilliseconds(500);

			DateTime start = DateTime.Now;
			long elapsed = 0;

			while (elapsed <= timeout.Ticks)
			{
				elapsed = DateTime.Now.Subtract(start).Ticks;
				
				var elementText = app.Query(automationId).FirstOrDefault()?.Text;
				if (elementText != null && elementText.Contains(text, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}

				Task.Delay(retryFrequency.Milliseconds).Wait();
			}
			
			return false;
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
			NavigateTo(app, string.Empty);
		}

		public static void NavigateBack(this IApp app)
		{
			app.Back();
		}
	}
}