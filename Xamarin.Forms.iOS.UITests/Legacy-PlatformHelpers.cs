using System;
using Xamarin.UITest;
using Xamarin.UITest.iOS;
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
			return elements [0].Label;
		}

		public static bool ScrollDownForElement (this IApp app, Func<AppQuery, AppQuery> query, int scrollNumberLimit)
		{
			var desiredElement = app.Query (query);
			var tabBarElement = app.Query (q => q.Raw ("TabBar"));
			// Check for elements under a TabbedBar
			// If the element exists and is under the tabbed bar scroll down once
			if (
				tabBarElement.Length > 0 &&
				desiredElement.Length > 0 &&
				(Math.Abs(desiredElement[0].Rect.Y - tabBarElement[0].Rect.Y) <= 75)
			)
				{
				app.ScrollDown ();
				return true;
			}

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

			// Avoid hidden elements under a tabbed controller
			app.ScrollDown ();
			Thread.Sleep (1000);

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

			app.ScrollUp ();

			return true;
		}

		public static bool DragFromToForElement (this iOSApp app, int scrollNumberLimit, Func<AppQuery, AppQuery> query, float xStart, float yStart, float xEnd, float yEnd)
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

		public static void SwipeBackNavigation (this iOSApp app)
		{
			//app.PanCoordinates (0, 125, 75, 125, TimeSpan.FromSeconds (5));
		}
			
		public static void DragFromTo (this iOSApp app, float xStart, float yStart, float xEnd, float yEnd, Speed speed = Speed.Fast)
		{
			//if (speed == Speed.Slow)
			//	app.PanCoordinates (xStart, yStart, xEnd, yEnd, TimeSpan.FromMilliseconds (3000));
			//else
			//	app.PanCoordinates (xStart, yStart, xEnd, yEnd, TimeSpan.FromMilliseconds (1000));

		}
	
		public static void KeyboardIsPresent (this iOSApp app)
		{
			// TODO : Add keyboard detection
			// app.WaitForElement (q => q.Raw ("KBKeyplaneView"));
			Assert.Inconclusive ("Keyboard should be shown");
		}

		public static void KeyboardIsDismissed (this iOSApp app)
		{
			// TODO : Add keyboard detection
			// app.WaitForNoElement (q => q.Raw ("KBKeyplaneView"));
			Assert.Inconclusive ("Keyboard should be dismissed");
		}
			
		public static int IndexForElementWithText (this iOSApp app, Func<AppQuery, AppQuery> query, string text)
		{
			var elements = app.Query (query);
			int index = 0;
			for (int i = 0; i < elements.Length; i++) {
				string labelText = elements[i].Label;
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
