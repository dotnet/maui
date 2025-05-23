#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_CATALYST
// [Windows] Swiping does not work with SwipeItemView in SwipeView , see https://github.com/dotnet/maui/issues/27436
// Since the Appium swipe action does not behave correctly on macOS, the test has been disabled on that platform
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
			//Random VisualTestFailedException failures occurred on CI when taking a screenshot, even after adding a delay. 
			//To ensure the element is visible, WaitForElement has been added.
			App.WaitForElement("Right");
		}

		[Test, Order(2)]
		[Category(UITestCategories.SwipeView)]
		public void CustomViewInRightSwipeItemViewIsVisibleOnRightSwipe()
		{
			App.WaitForElement(SwipeItemView);
			App.SwipeRightToLeft(SwipeItemView);
			App.WaitForElement("Entry");
		}
	}
}
#endif