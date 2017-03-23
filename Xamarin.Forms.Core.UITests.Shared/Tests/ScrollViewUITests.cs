using NUnit.Framework;

namespace Xamarin.Forms.Core.UITests
{
	[TestFixture]
	[Category(UITestCategories.ScrollView)]
	internal class ScrollViewGalleryTests : BaseTestFixture
	{
		public ScrollViewGalleryTests ()
		{
		}

		protected override void NavigateToGallery ()
		{
			App.NavigateToGallery (GalleryQueries.ScrollViewGallery);
		}

		[Test]
		[Description ("Scroll element to the start")]
		public void ScrollToElement1Start ()
		{
			//var scroller = App.Query (c => c.Marked ("thescroller"))[0];
			//need to extract the contentOffset on iOS
			App.Tap(c=> c.Marked("Start"));
			App.WaitForElement (c => c.Marked ("the scrollto button"));
			//Assert.Equals (App.Query (c => c.Marked ("the before")).Length, 0);
			App.Screenshot ("Element is  on the top");
		}

		[Test]
		[Description ("Scroll element to the center")]
		public void ScrollToElement2Center ()
		{
			App.Tap(c=> c.Marked("Center"));
			App.WaitForElement (c => c.Marked ("the scrollto button"));
			App.WaitForElement (c => c.Marked ("the before"));
			App.WaitForElement (c => c.Marked ("the after"));
			App.Screenshot ("Element is in the center");
		}

		[Test]
		[Description ("Scroll element to the end")]
		public void ScrollToElement3End ()
		{
			App.Tap(c=> c.Marked("End"));
			App.WaitForElement (c => c.Marked ("the scrollto button"));
			//Assert.Equals (App.Query (c => c.Marked ("the after")).Length, 0);
			App.Screenshot ("Element is in the end");
		}

		[Test]
		[Description ("ScrollTo Y = 100")]
		public void ScrollToY ()
		{
			App.Tap(c=> c.Marked("Scroll to 100"));
		}

		[Test]
		[Description ("ScrollTo Y = 100 no animation")]
		public void ScrollToYNoAnim ()
		{
			App.ScrollDown ();
			App.ScrollDown ();
			App.Tap (c => c.Marked ("Scroll to 100 no anim"));
		}
	}
}
