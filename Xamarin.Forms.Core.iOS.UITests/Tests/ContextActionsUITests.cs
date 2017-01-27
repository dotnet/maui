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
	[Category(UITestCategories.Cells)]
	internal class ContextActionsListUITests : BaseTestFixture
	{

		public ContextActionsListUITests ()
		{
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


#if __ANDROID__ || __MACOS__
		[Test]
		public void ContextActionsShow ()
		{
			// mark is an icon on android
			App.TouchAndHold (q => q.Marked (cell0));
			App.WaitForElement (q => q.Marked (delete));
			App.Screenshot ("I have actions!");
		}

		[Test]
		public void ContextActionsDelete ()
		{
			// mark is an icon on android
			App.TouchAndHold (q => q.Marked (cell1));
			App.WaitForElement (q => q.Marked (delete));
			App.Screenshot ("I have actions!");

			App.Tap (q => q.Marked (delete));
			App.WaitForNoElement (q => q.Marked (cell1));
			App.Screenshot ("Deleted cell 0");
		}
#endif

#if __IOS__
		[Test]
		public void PopoverDismiss()
		{
			var device = App.Device as iOSDevice;

			if (device == null)
			{
				return;
			}

			if (device.IsTablet) {
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
		}
#endif
	}
	[TestFixture]
	[Category(UITestCategories.Cells)]
	internal class ContextActionsTableUITests : BaseTestFixture
	{
		public ContextActionsTableUITests ()
		{
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

#if __ANDROID__ || __MACOS__
		[Test]
		public void ContextActionsShowAndReset ()
		{
			// mark is an icon on android
			App.TouchAndHold (q => q.Marked (cell0));
			App.WaitForElement (q => q.Marked (delete));
			App.Screenshot ("I have actions!");
				
			App.Tap (q => q.Marked (cellWithNoContextActions));
			App.WaitForNoElement (q => q.Marked (delete));
			App.Screenshot ("Actions should be gone");
		}
#endif
	}
}

