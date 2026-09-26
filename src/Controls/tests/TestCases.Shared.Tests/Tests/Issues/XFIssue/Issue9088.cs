#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS
// On Catalyst, Swipe actions not supported in Appium.
// On Windows, StackLayout AutomationId not works in Automation. 
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue9088 : _IssuesUITest
{
	const string ContentPageTitle = "Item1";
	const string SwipeViewId = "SwipeViewId";
	const string LeftCountLabelId = "LeftCountLabel";
	const string RightCountLabelId = "RightCountLabel";
	public Issue9088(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Bug] SwipeView items conflict with Shell menu swipe in from left, on real iOS devices";

	[Test]
	[Category(UITestCategories.Shell)]
	public void Issue9088SwipeViewConfictWithShellMenuSwipeInFromLeft()
	{
		App.WaitForElement(SwipeViewId);
		var contentX = App.WaitForElementAndGetRect("SwipeContentLabel").X;

		App.SwipeRightToLeft(SwipeViewId);
		AssertSwipeCompleted(LeftCountLabelId, "1");

		App.SwipeRightToLeft(SwipeViewId, 0.67, 250);
		AssertSwipeCompleted(LeftCountLabelId, "2");

		App.SwipeRightToLeft(SwipeViewId, 0.67, 200);
		AssertSwipeCompleted(LeftCountLabelId, "3");


		App.SwipeLeftToRight(SwipeViewId, 0.67, 200);
		AssertSwipeCompleted(RightCountLabelId, "1");

		App.SwipeLeftToRight(SwipeViewId, 0.67, 250);
		AssertSwipeCompleted(RightCountLabelId, "2");

		App.SwipeLeftToRight(SwipeViewId, 0.67, 500);
		AssertSwipeCompleted(RightCountLabelId, "3");


		App.SwipeRightToLeft(SwipeViewId);
		AssertSwipeCompleted(LeftCountLabelId, "4");

		App.SwipeLeftToRight(SwipeViewId);
		AssertSwipeCompleted(RightCountLabelId, "4");

		App.SwipeRightToLeft(SwipeViewId);
		AssertSwipeCompleted(LeftCountLabelId, "5");

		App.SwipeLeftToRight(SwipeViewId);
		AssertSwipeCompleted(RightCountLabelId, "5");

		App.SwipeLeftToRight(SwipeViewId);
		AssertSwipeCompleted(RightCountLabelId, "6");

		App.SwipeRightToLeft(SwipeViewId);
		AssertSwipeCompleted(LeftCountLabelId, "6");

		void AssertSwipeCompleted(string countLabel, string count)
		{
			App.RetryAssert(() =>
			{
				Assert.That(App.WaitForElement(countLabel).GetText(), Is.EqualTo(count));
				Assert.That(App.WaitForElementAndGetRect("SwipeContentLabel").X,
					Is.EqualTo(contentX).Within(1), "Wait for the closing animation before the next gesture.");
			});
		}
	}
}
#endif