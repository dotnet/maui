using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace UITests
{
	public class ScrollViewGalleryTests : ViewUITest
	{
		public ScrollViewGalleryTests(TestDevice device) : base(device) { }

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(GalleryQueries.ScrollViewGallery);
		}

		protected override void FixtureTeardown()
		{
			App.Back();
			base.FixtureTeardown();
		}

		[Test]
		[Category(UITestCategories.ScrollView)]
		[Description("Scroll element to the start")]
		public void ScrollToElement1Start()
		{
			App.Tap("Start");
			App.WaitForNoElement("the scrollto button");
			App.Screenshot("Element is  on the top");
		}

		[Test]
		[Category(UITestCategories.ScrollView)]
		[Description("Scroll element to the center")]
		public void ScrollToElement2Center()
		{
			App.Tap("Center");
			App.WaitForNoElement("the scrollto button");
			App.WaitForNoElement("the before");
			App.WaitForNoElement("the after");
			App.Screenshot("Element is in the center");
		}

		[Test]
		[Category(UITestCategories.ScrollView)]
		[Description("Scroll element to the end")]
		public void ScrollToElement3End()
		{
			App.Tap("End");
			App.WaitForNoElement("the scrollto button");
			App.Screenshot("Element is in the end");
		}
	}
}
