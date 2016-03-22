using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

using Xamarin.UITest;
using Xamarin.UITest.Android;
using Xamarin.UITest.iOS;
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
			app.Tap (Queries.NavigationBarBackButton ());
		}

		public static void NavigateToGallery (this IApp app, string page)
		{
			var text = Regex.Match (page, "'(?<text>[^']*)'").Groups["text"].Value;
			app.EnterText (q => q.Raw ("* marked:'SearchBar'"), text);
			//var searchBar = app.Query (q => q.Raw ("* marked:'SearchBar'")).Single ();
			Thread.Sleep(10000);

			app.Tap (q => q.Raw ("* marked:'GoToTestButton'"));
			app.WaitForNoElement (o => o.Raw ("* marked:'GoToTestButton'"), "Timed out", TimeSpan.FromMinutes(2));

			//app.Screenshot ("Navigating to gallery ...");
			//var galleryListViewBounds = app.Query (Queries.GalleryListView)[0].Rect;
			//app.ScrollForElement (page, new Drag (galleryListViewBounds, Drag.Direction.BottomToTop, Drag.DragLength.Long));
			//app.Tap (q => q.Raw (page));
			//app.Screenshot ("At gallery!");
		}

		public static void NavigateToTestCases (this IApp app, string testCase)
		{
			app.Tap (q => q.Button ("Go to Test Cases"));
			app.WaitForElement (q => q.Raw ("* marked:'TestCasesIssueList'"));

			app.EnterText (q => q.Raw ("* marked:'SearchBarGo'"), testCase);

			app.WaitForElement (q => q.Raw ("* marked:'SearchButton'"));
			app.Tap (q => q.Raw ("* marked:'SearchButton'"));

			//app.NavigateToTestCase(testCase);
		}

		public static void NavigateToTestCase (this IApp app, string testCase)
		{
			string testCasesQuery = "* marked:'" + testCase + "'";
			var testCaseIssue = app.Query (q => q.Raw ("* marked:'TestCasesIssueList'")).FirstOrDefault ();
			if (testCaseIssue != null) {
				AppRect scrollRect = testCaseIssue.Rect;
				app.ScrollForElement (testCasesQuery, new Drag (scrollRect, Drag.Direction.BottomToTop, Drag.DragLength.Long));
				app.Tap (q => q.Raw (testCasesQuery));
			} else {
				Debug.WriteLine (string.Format ("Failed to find test case {0}", testCase));
			}
		}

		public static bool RectsEqual (AppRect rectOne, AppRect rectTwo)
		{
			const float equalsTolerance = 0.1f;

			bool areEqual =
				(Math.Abs (rectOne.X - rectTwo.X) < equalsTolerance) &&
				(Math.Abs (rectOne.Y - rectTwo.Y) < equalsTolerance) &&
				(Math.Abs (rectOne.Width - rectTwo.Width) < equalsTolerance) &&
				(Math.Abs (rectOne.Height - rectTwo.Height) < equalsTolerance) &&
				(Math.Abs (rectOne.CenterX - rectTwo.CenterX) < equalsTolerance) &&
				(Math.Abs (rectOne.CenterY - rectTwo.CenterY) < equalsTolerance);

			return areEqual;
		}

		public static void WaitForAnimating (this IApp app, Func<AppQuery, AppQuery> query) 
		{
			// Right now only checks if bounds are moving
			const int pollingRate = 200;
			const int timeout = 5000;
			var sw = new Stopwatch ();

			var previousState = app.Query (query).First ().Rect;

			sw.Start ();
			while (true) {

				var newState = app.Query (query).First ().Rect;

				if (RectsEqual (previousState, newState))
					break;

				previousState = newState;

				if (sw.ElapsedMilliseconds >= timeout)
					throw new Exception("Timed out");

				Thread.Sleep (pollingRate);
			}
			sw.Stop ();
		}
	}
}
