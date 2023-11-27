using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
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
		[Description("ScrollTo Y = 100")]
		public void ScrollToY()
		{
			if (Device == TestDevice.Mac || Device == TestDevice.iOS)
			{
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
			if (Device == TestDevice.Mac || Device == TestDevice.iOS)
			{
				App.Click("Scroll100");
				App.WaitForNoElement("completed");
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