using System;
using Xamarin.UITest;
using Xamarin.UITest.Android;
using Xamarin.UITest.Queries;
using NUnit.Framework;
using System.Threading;

namespace Xamarin.Forms.UITests
{

	public static class PlatformHelpers
	{

		public static string GetTextForQuery (this IApp app, Func<AppQuery, AppQuery> query)
		{
			AppResult[] elements = app.Query (query);
			if (elements.Length > 1) {
				// Test cloud doesn't support Assert.Fail
				Assert.False (true, "Query returned more than one result");
			}
			return elements [0].Text;
		}
	
		public static bool ScrollDownForElement (this IApp app, Func<AppQuery, AppQuery> query, int scrollNumberLimit)
		{
			// Check if element exists before scrolling
			if (app.Query (query).Length > 0)
				return true;

			int scrollNumber = 0;
			while (app.Query (query).Length == 0) {
				app.ScrollDown ();
				scrollNumber++;
				if (scrollNumber > scrollNumberLimit)
					return false;
			}

			return true;
		}

		public static bool ScrollUpForElement (this IApp app, Func<AppQuery, AppQuery> query, int scrollNumberLimit)
		{
			int scrollNumber = 0;
			while (app.Query (query).Length == 0) {
				app.ScrollUp ();
				scrollNumber++;
				if (scrollNumber > scrollNumberLimit)
					return false;
			}

			return true;
		}

		public static bool DragFromToForElement (this AndroidApp app, int scrollNumberLimit, Func<AppQuery, AppQuery> query, float xStart, float yStart, float xEnd, float yEnd)
		{
			int numberOfScrolls = 0;
			// Element exists
			if (app.Query (query).Length > 0)
				return true;

			while (app.Query (query).Length == 0) {
				DragFromTo (app, xStart, yStart, xEnd, yEnd);
				if (numberOfScrolls > scrollNumberLimit) {
					return false;
				}
				numberOfScrolls++;
			}
			// Element found
			return true;
		}

		public static void SwipeBackNavigation (this AndroidApp app)
		{
			// Do nothing on Android
		}

		public static void DragFromTo (this AndroidApp app, float xStart, float yStart, float xEnd, float yEnd, Speed speed = Speed.Fast)
		{
			// No effect on Android
			app.DragCoordinates (xStart, yStart, xEnd, yEnd);
		}

		public static void KeyboardIsPresent (this AndroidApp app)
		{
			// TODO : Add keyboard detection
//			Thread.Sleep (1000);
//
//			AppRect screenSize = app.MainScreenBounds ();
//			AppRect contentBounds = app.Query (q => q.Raw ("*").Id ("content"))[0].Rect;
//
//			bool keyboardIsShown = false;
//			if ((screenSize.Height - contentBounds.Height) > (screenSize.Height / 4)) {
//				// Determine if keyboard is showing by seeing if content size is shrunk by over 1/4 of screens size
//				keyboardIsShown = true;
//			}
//
//			Assert.IsTrue (keyboardIsShown, "Keyboard should be shown");
			Assert.Inconclusive ("Keyboard should be shown");
		}

		public static void KeyboardIsDismissed (this AndroidApp app)
		{
			// TODO : Add keyboard detection
//			AppRect screenSize = app.MainScreenBounds ();
//			AppRect contentBounds = app.Query (q => q.Raw ("*").Id ("content"))[0].Rect;
//
//			bool keyboardIsShown = false;
//			if ((screenSize.Height - contentBounds.Height) > (screenSize.Height / 4)) {
//				// Determine if keyboard is showing by seeing if content size is shrunk by over 1/4 of screens size
//				keyboardIsShown = true;
//			}
//
//			Assert.IsFalse (keyboardIsShown, "Keyboard should be dismissed");
			Assert.Inconclusive ("Keyboard should be dismissed");
		}

		public static int IndexForElementWithText (this AndroidApp app, Func<AppQuery, AppQuery> query, string text)
		{
			var elements = app.Query (query);
			int index = 0;
			for (int i = 0; i < elements.Length; i++) {
				string labelText = elements[i].Text;
				if (labelText == (text)) {
					index = i;
					break;
				} 
				index++;
			}
			return index == elements.Length ? -1 : index; 
		}
	}
}
