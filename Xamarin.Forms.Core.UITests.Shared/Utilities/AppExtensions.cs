using System;
using System.Linq;

using Xamarin.UITest;
using Xamarin.UITest.Queries;
using System.Text.RegularExpressions;

namespace Xamarin.Forms.Core.UITests
{
	internal static class AppExtensions
	{
		public static AppRect ScreenBounds (this IApp app)
		{
			return app.Query (Queries.Root ()).First().Rect;
		}

		public static void NavigateBack (this IApp app)
		{
			app.Back();
		}

		public static void NavigateToGallery (this IApp app, string page)
		{
			const string goToTestButtonQuery = "* marked:'GoToTestButton'";

			app.WaitForElement(q => q.Raw(goToTestButtonQuery), "Timed out waiting for Go To Test button to disappear", TimeSpan.FromSeconds(10));

			var text = Regex.Match (page, "'(?<text>[^']*)'").Groups["text"].Value;

			app.WaitForElement("SearchBar");
			app.EnterText (q => q.Raw ("* marked:'SearchBar'"), text);

			app.Tap (q => q.Raw (goToTestButtonQuery));
			app.WaitForNoElement (o => o.Raw (goToTestButtonQuery), "Timed out waiting for Go To Test button to disappear", TimeSpan.FromMinutes(2));
		}
	}
}
