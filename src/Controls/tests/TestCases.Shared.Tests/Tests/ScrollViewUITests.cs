using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
	[Category(UITestCategories.ScrollView)]
	public class ScrollToUITests : CoreGalleryBasePageTest
	{
		const string LayoutGallery = "ScrollView Gallery";
		protected override bool ResetAfterEachTest => true;
		public ScrollToUITests(TestDevice device)
			: base(device)
		{
		}

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(LayoutGallery);
		}


		[Test]
		[Description("Scroll element to the start")]
		public void ScrollToElement1Start()
		{
			App.WaitForElement("WaitForStubControl");

			// 1. Tap a button to scroll to the start position.
			App.Tap("Start");
			App.WaitForElement("the scrollto button");

			// 2. Verify that the scroll has moved to the correct position.
			VerifyScreenshot();
		}

		[Test]
		[Description("Scroll element to the center")]
		public void ScrollToElement2Center()
		{
			App.WaitForElement("WaitForStubControl");

			// 1. Tap a button to scroll to the center position.
			App.Tap("CenterPosition");
			App.WaitForElement("the scrollto button");

			// 2. Verify that the scroll has moved to the correct position.
			App.WaitForElement("the before");
			App.WaitForElement("the after");
			VerifyScreenshot();
		}

		[Test]
		[Description("Scroll element to the end")]
		public void ScrollToElement3End()
		{
			App.WaitForElement("WaitForStubControl");

			// 1. Tap a button to scroll to the end.
			App.Tap("End");

			// 2. Verify that the scroll has moved to the correct position.
			App.WaitForElement("the scrollto button");
			VerifyScreenshot();
		}

		[Test]
		[Description("ScrollTo Y = 100")]
		public void ScrollToY()
		{
			App.WaitForElement("WaitForStubControl");

			// 1. Tap a button to scroll 100 px.
			App.Tap("Scroll100");
		}

		// ScrollToYTwice (src\Compatibility\ControlGallery\src\UITests.Shared\Tests\ScrollViewUITests.cs)
		[Test]
		[Description("ScrollTo Y = 100")]
		[FailsOnIOSWhenRunningOnXamarinUITest("This test is failing, likely due to product issue, More Information: https://github.com/dotnet/maui/issues/27250")]
		[FailsOnMacWhenRunningOnXamarinUITest("This test is failing, likely due to product issue, More Information: https://github.com/dotnet/maui/issues/27250")]
		[FailsOnWindowsWhenRunningOnXamarinUITest("This test is failing, likely due to product issue, More Information: https://github.com/dotnet/maui/issues/27250")]
		public void ScrollToYTwice()
		{
			App.WaitForElement("WaitForStubControl");

			// 1. Tap a button to scroll 100 px.
			App.Tap("Scroll100");
			App.WaitForElement("completed");

			// 2. Repeat.
			App.Tap("Scroll100");
			App.WaitForElement("completed");
		}

		[Test]
		[Description("Scroll down the ScrollView using a gesture")]
		public void ScrollUpAndDownWithGestures()
		{
			var text = App.WaitForElement("WaitForStubControl").GetText();
			App.ScrollDown("thescroller", ScrollStrategy.Gesture, 0.75);
			var text1 = App.WaitForElement("WaitForStubControl").GetText();
			Assert.That(text1, Is.Not.EqualTo(text));
			App.ScrollUp("thescroller", ScrollStrategy.Gesture, 0.75);
			var text2 = App.WaitForElement("WaitForStubControl").GetText();
			Assert.That(text2, Is.Not.EqualTo(text1));
		}
	}
}