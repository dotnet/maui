using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	[Category(UITestCategories.ScrollView)]
	public class ScrollToUITests : ScrollViewUITests
	{
		public ScrollToUITests(TestDevice device)
			: base(device)
		{
		}

		[Test]
		[Description("Scroll element to the start")]
		public void ScrollToElement1Start()
		{
			if (Device == TestDevice.Android || Device == TestDevice.Mac || Device == TestDevice.iOS)
			{
				App.Click("ScrollViewScrollTo");
				App.WaitForElement("WaitForStubControl");

				// 1. Tap a button to scroll to the start position.
				App.Click("Start");
				App.WaitForNoElement("the scrollto button");

				// 2. Verify that the scroll has moved to the correct position.
				App.Screenshot("Element is on the top");
			}
			else
			{
				Assert.Ignore("This test is failing, likely due to product issue");
			}
		}

		[Test]
		[Description("Scroll element to the center")]
		public void ScrollToElement2Center()
		{
			if (Device == TestDevice.Android || Device == TestDevice.Mac || Device == TestDevice.iOS)
			{
				App.Click("ScrollViewScrollTo");
				App.WaitForElement("WaitForStubControl");

				// 1. Tap a button to scroll to the center position.
				App.Click("Center");
				App.WaitForNoElement("the scrollto button");

				// 2. Verify that the scroll has moved to the correct position.
				App.WaitForNoElement("the before");
				App.WaitForNoElement("the after");

				App.Screenshot("Element is in the center");
			}
			else
			{
				Assert.Ignore("This test is failing, likely due to product issue");
			}
		}

		[Test]
		[Description("Scroll element to the end")]
		public void ScrollToElement3End()
		{
			if (Device == TestDevice.Android || Device == TestDevice.Mac || Device == TestDevice.iOS)
			{
				App.Click("ScrollViewScrollTo");
				App.WaitForElement("WaitForStubControl");

				// 1. Tap a button to scroll to the end.
				App.Click("End");

				// 2. Verify that the scroll has moved to the correct position.
				App.WaitForNoElement("the scrollto button");
				App.Screenshot("Element is in the end");
			}
			else
			{
				Assert.Ignore("This test is failing, likely due to product issue");
			}
		}

		[Test]
		[Description("ScrollTo Y = 100")]
		public void ScrollToY()
		{
			if (Device == TestDevice.Android || Device == TestDevice.Mac || Device == TestDevice.iOS)
			{
				App.Click("ScrollViewScrollTo");
				App.WaitForElement("WaitForStubControl");

				// 1. Tap a button to scroll 100 px.
				App.Click("Scroll100");
			}
			else
			{
				Assert.Ignore("This test is failing, likely due to product issue");
			}
		}

		// ScrollToYTwice (src\Compatibility\ControlGallery\src\UITests.Shared\Tests\ScrollViewUITests.cs)
		[Test]
		[Description("ScrollTo Y = 100")]
		public void ScrollToYTwice()
		{
			if (Device == TestDevice.Android || Device == TestDevice.Mac || Device == TestDevice.iOS)
			{
				App.Click("ScrollViewScrollTo");
				App.WaitForElement("WaitForStubControl");

				// 1. Tap a button to scroll 100 px.
				App.Click("Scroll100");
				App.WaitForNoElement("completed");

				// 2. Repeat.
				App.Click("Scroll100");
				App.WaitForNoElement("completed");
			}
			else
			{
				Assert.Ignore("This test is failing, likely due to product issue");
			}
		}
	}
}