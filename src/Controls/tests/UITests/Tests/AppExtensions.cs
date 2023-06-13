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
			app.EnterText("SearchBar", text);

			app.Tap(goToTestButtonId);

			app.WaitForNoElement(goToTestButtonId, "Timed out waiting for Go To Test button to disappear", TimeSpan.FromMinutes(1));
		}

		public static void NavigateToGallery(this IApp app, string page)
		{
			app.WaitForElement(goToTestButtonId, "Timed out waiting for Go To Test button to appear", TimeSpan.FromMinutes(2));
			var text = Regex.Match(page, "'(?<text>[^']*)'").Groups["text"].Value;
			NavigateTo(app, text);
		}

		public static void NavigateBack(this IApp app)
		{
			app.Back();
		}
	}
}