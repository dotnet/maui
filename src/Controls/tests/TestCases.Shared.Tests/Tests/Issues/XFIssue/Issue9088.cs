#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS
// On Catalyst, Swipe actions not supported in Appium.
// On Windows, StackLayout AutomationId not works in Automation. 
using Xunit;
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

	[Fact]
	[Category(UITestCategories.Shell)]
	public void Issue9088SwipeViewConfictWithShellMenuSwipeInFromLeft()
	{
		App.WaitForElement(SwipeViewId);

		App.SwipeRightToLeft(SwipeViewId);
		Assert.Equal("1", App.WaitForElement(LeftCountLabelId).GetText());

		App.SwipeRightToLeft(SwipeViewId, 0.67, 250);
		Assert.Equal("2", App.WaitForElement(LeftCountLabelId).GetText());

		App.SwipeRightToLeft(SwipeViewId, 0.67, 200);
		Assert.Equal("3", App.WaitForElement(LeftCountLabelId).GetText());


		App.SwipeLeftToRight(SwipeViewId, 0.67, 200);
		Assert.Equal("1", App.WaitForElement(RightCountLabelId).GetText());

		App.SwipeLeftToRight(SwipeViewId, 0.67, 250);
		Assert.Equal("2", App.WaitForElement(RightCountLabelId).GetText());

		App.SwipeLeftToRight(SwipeViewId, 0.67, 500);
		Assert.Equal("3", App.WaitForElement(RightCountLabelId).GetText());


		App.SwipeRightToLeft(SwipeViewId);
		Assert.Equal("4", App.WaitForElement(LeftCountLabelId).GetText());

		App.SwipeLeftToRight(SwipeViewId);
		Assert.Equal("4", App.WaitForElement(RightCountLabelId).GetText());

		App.SwipeRightToLeft(SwipeViewId);
		Assert.Equal("5", App.WaitForElement(LeftCountLabelId).GetText());

		App.SwipeLeftToRight(SwipeViewId);
		Assert.Equal("5", App.WaitForElement(RightCountLabelId).GetText());

		App.SwipeLeftToRight(SwipeViewId);
		Assert.Equal("6", App.WaitForElement(RightCountLabelId).GetText());

		App.SwipeRightToLeft(SwipeViewId);
		Assert.Equal("6", App.WaitForElement(LeftCountLabelId).GetText());
	}
}
#endif