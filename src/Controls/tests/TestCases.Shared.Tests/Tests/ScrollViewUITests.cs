using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
	[Category(UITestCategories.ScrollView)]
	public class ScrollToUITests : UITest
	{
		const string LayoutGallery = "ScrollView Gallery";

		public ScrollToUITests(TestDevice device)
			: base(device)
		{
		}

		protected override void FixtureSetup()
		{
			base.FixtureSetup();
			App.NavigateToGallery(LayoutGallery);
		}

#if ANDROID
		[Test]
		[Description("Scroll element to the start")]
		[FailsOnIOSWhenRunningOnXamarinUITest("This test is failing, likely due to product issue")]
		[FailsOnMacWhenRunningOnXamarinUITest("This test is failing, likely due to product issue")]
		[FailsOnWindowsWhenRunningOnXamarinUITest("This test is failing, likely due to product issue")]
		public void ScrollToElement1Start()
		{
			if (Device == TestDevice.Android)
			{
				App.WaitForElement("WaitForStubControl");

				// 1. Tap a button to scroll to the start position.
				App.Tap("Start");
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
		[FailsOnIOSWhenRunningOnXamarinUITest("This test is failing, likely due to product issue")]
		[FailsOnMacWhenRunningOnXamarinUITest("This test is failing, likely due to product issue")]
		[FailsOnWindowsWhenRunningOnXamarinUITest("This test is failing, likely due to product issue")]
		public void ScrollToElement2Center()
		{
			if (Device == TestDevice.Android)
			{
				App.WaitForElement("WaitForStubControl");

				// 1. Tap a button to scroll to the center position.
				App.Tap("Center");
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
		[FailsOnIOSWhenRunningOnXamarinUITest("This test is failing, likely due to product issue")]
		[FailsOnMacWhenRunningOnXamarinUITest("This test is failing, likely due to product issue")]
		[FailsOnWindowsWhenRunningOnXamarinUITest("This test is failing, likely due to product issue")]
		public void ScrollToElement3End()
		{
			if (Device == TestDevice.Android)
			{
				App.WaitForElement("WaitForStubControl");

				// 1. Tap a button to scroll to the end.
				App.Tap("End");

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
		[FailsOnIOSWhenRunningOnXamarinUITest("This test is failing, likely due to product issue")]
		[FailsOnMacWhenRunningOnXamarinUITest("This test is failing, likely due to product issue")]
		[FailsOnWindowsWhenRunningOnXamarinUITest("This test is failing, likely due to product issue")]
		public void ScrollToY()
		{
			if (Device == TestDevice.Android)
			{
				App.WaitForElement("WaitForStubControl");

				// 1. Tap a button to scroll 100 px.
				App.Tap("Scroll100");
			}
			else
			{
				Assert.Ignore("This test is failing, likely due to product issue");
			}
		}

		// ScrollToYTwice (src\Compatibility\ControlGallery\src\UITests.Shared\Tests\ScrollViewUITests.cs)
		[Test]
		[Description("ScrollTo Y = 100")]
		[FailsOnIOSWhenRunningOnXamarinUITest("This test is failing, likely due to product issue")]
		[FailsOnMacWhenRunningOnXamarinUITest("This test is failing, likely due to product issue")]
		[FailsOnWindowsWhenRunningOnXamarinUITest("This test is failing, likely due to product issue")]
		public void ScrollToYTwice()
		{
			if (Device == TestDevice.Android)
			{
				App.WaitForElement("WaitForStubControl");

				// 1. Tap a button to scroll 100 px.
				App.Tap("Scroll100");
				App.WaitForNoElement("completed");

				// 2. Repeat.
				App.Tap("Scroll100");
				App.WaitForNoElement("completed");
			}
			else
			{
				Assert.Ignore("This test is failing, likely due to product issue");
			}
		}
#endif

#if ANDROID || IOS
		[Test]
		[Description("Scroll down the ScrollView using a gesture")]
		[FailsOnMacWhenRunningOnXamarinUITest("This test is failing, likely due to product issue")]
		[FailsOnWindowsWhenRunningOnXamarinUITest("This test is failing, likely due to product issue")]
		public void ScrollUpAndDownWithGestures()
		{
			App.ScrollDown("thescroller", ScrollStrategy.Gesture, 0.75);
			App.Screenshot("Element scrolled down");

			App.ScrollUp("thescroller", ScrollStrategy.Gesture, 0.75);
			App.Screenshot("Element scrolled up");
		}
#endif
	}
}