#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS
// On Catalyst, Swipe actions not supported in Appium.
// [Windows] Swiping does not work with SwipeItemView in SwipeView , see https://github.com/dotnet/maui/issues/27436
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue27367 : _IssuesUITest
	{
		const string SwipeItem = "SwipeItem";
		const string SwipeItemView = "SwipeItemView";
		public Issue27367(TestDevice device) : base(device) { }

		public override string Issue => "[Android] Right SwipeView items are not visible in the SwipeView";

		[Test, Order(1)]
		[Category(UITestCategories.SwipeView)]
		public void TextInRightSwipeItemIsVisibleOnRightSwipe()
		{
			App.WaitForElement(SwipeItem);
			App.SwipeRightToLeft(SwipeItem);
#if ANDROID
			//Random VisualTestFailedException failures occurred on CI for Android when taking a screenshot, even after adding a delay. 
			//To ensure the element is visible, WaitForElement has been added.
			App.WaitForElement("Right");
#else
			// CI captures a screenshot before the swipe is fully completed, resulting in a slight visual difference.
			// Even using Thread.Sleep or WaitForElement sometimes fails, so adding Task.Delay working fine.
			Task.Delay(500);
			VerifyScreenshot();
#endif
		}

		[Test, Order(2)]
		[Category(UITestCategories.SwipeView)]
		public void CustomViewInRightSwipeItemViewIsVisibleOnRightSwipe()
		{
			App.WaitForElement(SwipeItemView);
			App.SwipeRightToLeft(SwipeItemView);
#if ANDROID
			App.WaitForElement("Entry");
#else
			Task.Delay(500);
			VerifyScreenshot();
#endif
		}
	}
}
#endif