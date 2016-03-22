using NUnit.Framework;
using Xamarin.UITest;
using System;
using System.Threading;

using Xamarin.UITest.Android;
using Xamarin.UITest.iOS;
using Xamarin.UITest.Queries;

namespace Xamarin.Forms.Core.UITests
{
	[TestFixture]
	[Category ("Cells")]
	internal class ContextActionsListUITests : BaseTestFixture
	{

		public ContextActionsListUITests ()
		{
			ShouldResetPerFixture = false;
		}

		protected override void NavigateToGallery ()
		{
			App.NavigateToGallery (GalleryQueries.ContextActionsListGallery);
		}

		const string cell0 = "Subject Line 0";
		const string cell1 = "Subject Line 1";
		const string move = "Move";
		const string delete = "Delete";
		const string clear = "Clear Items";
		const string mark = "Mark";

		[Test]
		public void ContextActionsShow ()
		{
			if (App is AndroidApp) {
				// mark is an icon on android
				App.TouchAndHold (q => q.Marked (cell0));
				App.WaitForElement (q => q.Marked (delete));
				App.Screenshot ("I have actions!");
			} else if (App is iOSApp) {
				Assert.Inconclusive ("Not tested on iOS yet");
			}
		}

		[Test]
		public void ContextActionsDelete ()
		{
			if (App is AndroidApp) {
				// mark is an icon on android
				App.TouchAndHold (q => q.Marked (cell0));
				App.WaitForElement (q => q.Marked (delete));
				App.Screenshot ("I have actions!");

				App.Tap (q => q.Marked (delete));
				App.WaitForNoElement (q => q.Marked (cell0));
				App.Screenshot ("Deleted cell 0");

			} else if (App is iOSApp) {
				Assert.Inconclusive ("Not tested on iOS yet");
			}
		}

		[Test]
		public void PopoverDismiss()
		{
			if (App is iOSApp) {
				var app = ((iOSApp)App);
				if (app.Device.IsTablet) {
					var screenBounds = App.Query (PlatformQueries.Root)[0].Rect;
					var cellBounds = App.Query (q => q.Marked (cell0))[0].Rect;
					App.DragCoordinates (screenBounds.Width - 10, cellBounds.CenterY, 10, cellBounds.CenterY);
					App.Screenshot("I see context actions");
					App.Tap (q => q.Marked ("More"));
					App.Screenshot ("Should see Popover");
					App.TapCoordinates (50, 50);
					App.Screenshot ("I should not crash");
				} else {
					Assert.Inconclusive("Not testing iOS Phone");
				}
			} else {
				Assert.Inconclusive ("Not testing on Android");
			}
		}
	}

	[TestFixture]
	[Category ("Cells")]
	internal class ContextActionsTableUITests : BaseTestFixture
	{
		public ContextActionsTableUITests ()
		{
			ShouldResetPerFixture = false;
		}

		protected override void NavigateToGallery ()
		{
			App.NavigateToGallery (GalleryQueries.ContextActionsTableGallery);
		}

		const string cell0 = "Subject Line 0";
		const string cell1 = "Subject Line 1";
		const string move = "Move";
		const string delete = "Delete";
		const string clear = "Clear Items";
		const string mark = "Mark";
		const string cellWithNoContextActions = "I have no ContextActions";

		[Test]
		public void ContextActionsShowAndReset ()
		{
			if (App is AndroidApp) {
				// mark is an icon on android
				App.TouchAndHold (q => q.Marked (cell0));
				App.WaitForElement (q => q.Marked (delete));
				App.Screenshot ("I have actions!");
				
				App.Tap (q => q.Marked (cellWithNoContextActions));
				App.WaitForNoElement (q => q.Marked (delete));
				App.Screenshot ("Actions should be gone");

			} else if (App is iOSApp) {
				Assert.Inconclusive ("Not tested on iOS yet");
			}
		}
	}
}

