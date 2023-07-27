using Microsoft.Maui.Appium;
using NUnit.Framework;

namespace Microsoft.Maui.AppiumTests
{
	class ScrollViewUITests : UITestBase
	{
		const string ScrollViewGallery = "* marked:'ScrollView Gallery'";

		public ScrollViewUITests(TestDevice device)
			: base(device)
		{
		}

		protected override void FixtureSetup()
		{
			base.FixtureSetup();
			App.NavigateToGallery(ScrollViewGallery);
		}

		protected override void FixtureTeardown()
		{
			base.FixtureTeardown();
			App.NavigateBack();
		}

		[Test]
		[Description("Scroll element to the start")]
		public void ScrollToElement1Start()
		{
			App.Tap(c => c.Marked("Start"));
			App.WaitForElement(c => c.Marked("the scrollto button"));
			App.Screenshot("Element is  on the top");
		}

		[Test]
		[Description("Scroll element to the center")]
		public void ScrollToElement2Center()
		{
			App.Tap(c => c.Marked("Center"));
			App.WaitForElement(c => c.Marked("the scrollto button"));
			App.WaitForElement(c => c.Marked("the before"));
			App.WaitForElement(c => c.Marked("the after"));
			App.Screenshot("Element is in the center");
		}

		[Test]
		[Description("Scroll element to the end")]
		public void ScrollToElement3End()
		{
			App.Tap(c => c.Marked("End"));
			App.WaitForElement(c => c.Marked("the scrollto button"));
			App.Screenshot("Element is in the end");
		}
	}
}