using System;
using System.Linq;

using Xamarin.UITest;
using Xamarin.UITest.Queries;
using System.Text.RegularExpressions;
using System.Threading;
using Xamarin.Forms.Controls.Issues;
#if __IOS__
using Xamarin.UITest.iOS;
#endif

namespace Xamarin.UITest
{
	internal static class AppExtensions
	{
		public static T[] QueryUntilPresent<T>(
			this IApp app,
			Func<T[]> func,
			int retryCount = 10,
			int delayInMs = 2000)
		{
			var results = func();

			int counter = 0;
			while ((results == null || results.Length == 0) && counter < retryCount)
			{
				Thread.Sleep(delayInMs);
				results = func();
				counter++;
			}

			return results;
		}

		public static bool IsApiHigherThan(this IApp app, int apiLevel, string apiLabelId = "ApiLevel")
		{
			var api = Convert.ToInt32(app.WaitForElement("ApiLabel")[0].ReadText());

			if (api < apiLevel)
				return false;

			return true;
		}

		public static void TapOverflowMenuButton(this IApp app)
		{
#if __ANDROID__
			// show secondary menu
			// When running these tests as release/d8/r8/AndroidX the runner was having trouble locating "OverflowMenuButton"
			// so we search through the ActionMenu for the button
			var menuElements = app.WaitForElement(c => c.Class("ActionMenuView").Descendant());
			var menuElement = menuElements.Where(x => x.Class.Contains("OverflowMenuButton")).FirstOrDefault();

			if (menuElement != null)
			{
				app.Tap(c => c.Class(menuElement.Class));
			}
			else
			{
				app.WaitForElement(c => c.Class("OverflowMenuButton"));
				app.Tap(c => c.Class("OverflowMenuButton"));
			}
#endif
		}

		public static bool IsTablet(this IApp app)
		{
#if __IOS__
			if (app is Xamarin.Forms.Controls.ScreenshotConditionalApp sca)
			{
				return sca.IsTablet;
			}
			else if (app is iOSApp iOSApp)
			{
				return iOSApp.Device.IsTablet;
			}
#endif
			return false;
		}

		public static bool IsPhone(this IApp app)
		{
#if __IOS__
			if (app is Xamarin.Forms.Controls.ScreenshotConditionalApp sca)
			{
				return sca.IsPhone;
			}
			else if (app is iOSApp iOSApp)
			{
				return iOSApp.Device.IsPhone;
			}
#endif
			return true;
		}

		public static TResult[] InvokeFromElement<TResult>(this IApp app, string element, string methodName) =>
			app.Query(c => c.Marked(element).Invoke(methodName).Value<TResult>());

#if __IOS__
		public static void SendAppToBackground(this IApp app, TimeSpan timeSpan)
		{
			if(app is Xamarin.Forms.Controls.ScreenshotConditionalApp sca)
			{
				sca.SendAppToBackground(timeSpan);
				Thread.Sleep(timeSpan.Add(TimeSpan.FromSeconds(2)));
			}
		}
#endif
	}
}

namespace Xamarin.Forms.Core.UITests
{
	internal static class AppExtensions
	{
		public static AppRect ScreenBounds(this IApp app)
		{
			return app.Query(Queries.Root()).First().Rect;
		}

		public static void NavigateBack(this IApp app)
		{
			app.Back();
		}

		public static void NavigateToGallery(this IApp app, string page)
		{
			NavigateToGallery(app, page, null);
		}

		public static void NavigateToGallery(this IApp app, string page, string visual)
		{
			const string goToTestButtonQuery = "* marked:'GoToTestButton'";

			app.WaitForElement(q => q.Raw(goToTestButtonQuery), "Timed out waiting for Go To Test button to appear", TimeSpan.FromMinutes(2));

			var text = Regex.Match(page, "'(?<text>[^']*)'").Groups["text"].Value;

			app.WaitForElement("SearchBar");
			app.EnterText(q => q.Raw("* marked:'SearchBar'"), text);
			app.DismissKeyboard();

			if (!String.IsNullOrWhiteSpace(visual))
			{
				app.ActivateContextMenu($"{text}AutomationId");
				app.Tap("Select Visual");
				app.Tap("Material");
			}
			else
			{
				app.Tap(q => q.Raw(goToTestButtonQuery));
			}

			app.WaitForNoElement(o => o.Raw(goToTestButtonQuery), "Timed out waiting for Go To Test button to disappear", TimeSpan.FromMinutes(2));
		}


		public static AppResult[] QueryNTimes(this IApp app, Func<AppQuery, AppQuery> elementQuery, int numberOfTries, Action onFail)
		{
			int tryCount = 0;
			var elements = app.Query(elementQuery);
			while (elements.Length == 0 && tryCount < numberOfTries)
			{
				elements = app.Query(elementQuery);
				tryCount++;
				if (elements.Length == 0 && onFail != null)
					onFail();
			}

			return elements;
		}
	}
}