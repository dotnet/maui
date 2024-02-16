using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	class ScrollViewUITests : UITest
	{
		const string ScrollViewGallery = "ScrollView Gallery";

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
			this.Back();
		}

		[Test]
		[Category(UITestCategories.ScrollView)]
		[Description("Scroll element to the start")]
		public void ScrollToElement1Start()
		{
			if (Device == TestDevice.Mac || Device == TestDevice.iOS)
			{
				App.Click("Start");
				App.WaitForElement("the scrollto button");
				App.Screenshot("Element is  on the top");
			}
			else
			{
				Assert.Ignore("This test is failing, likely due to product issue");
			}
		}

		[Test]
		[Category(UITestCategories.ScrollView)]
		[Description("Scroll element to the center")]
		public void ScrollToElement2Center()
		{
			if (Device == TestDevice.Mac || Device == TestDevice.iOS)
			{
				App.Click("Center");
				App.WaitForElement("the scrollto button");
				App.WaitForElement("the before");
				App.WaitForElement("the after");
				App.Screenshot("Element is in the center");
			}
			else
			{
				Assert.Ignore("This test is failing, likely due to product issue");
			}
		}

		[Test]
		[Category(UITestCategories.ScrollView)]
		[Description("Scroll element to the end")]
		public void ScrollToElement3End()
		{
			if (Device == TestDevice.Mac || Device == TestDevice.iOS)
			{
				App.Click("End");
				App.WaitForElement("the scrollto button");
				App.Screenshot("Element is in the end");
			}
			else
			{
				Assert.Ignore("This test is failing, likely due to product issue");
			}
		}

		[Test]
		[Category(UITestCategories.ScrollView)]
		[Description("Scroll down the ScrollView using a gesture")]
		public void ScrollUpAndDownWithGestures()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Mac, TestDevice.Windows });

			App.ScrollDown("thescroller", ScrollStrategy.Gesture, 0.75);
			App.Screenshot("Element scrolled down");

			App.ScrollUp("thescroller", ScrollStrategy.Gesture, 0.75);
			App.Screenshot("Element scrolled up");
		}
	}
}