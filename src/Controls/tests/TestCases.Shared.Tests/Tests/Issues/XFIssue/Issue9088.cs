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

		App.SwipeRightToLeft(SwipeViewId);
		Assert.That(App.WaitForElement(LeftCountLabelId).GetText(), Is.EqualTo("1")); 

		App.SwipeRightToLeft(SwipeViewId, 0.67, 250);
		Assert.That(App.WaitForElement(LeftCountLabelId).GetText(), Is.EqualTo("2")); 

		App.SwipeRightToLeft(SwipeViewId, 0.67, 200);
		Assert.That(App.WaitForElement(LeftCountLabelId).GetText(), Is.EqualTo("3")); 


		App.SwipeLeftToRight(SwipeViewId, 0.67, 200);
		Assert.That(App.WaitForElement(RightCountLabelId).GetText(), Is.EqualTo("1")); 

		App.SwipeLeftToRight(SwipeViewId, 0.67, 250);
		Assert.That(App.WaitForElement(RightCountLabelId).GetText(), Is.EqualTo("2")); 

		App.SwipeLeftToRight(SwipeViewId, 0.67, 500);
		Assert.That(App.WaitForElement(RightCountLabelId).GetText(), Is.EqualTo("3")); 


		App.SwipeRightToLeft(SwipeViewId);
		Assert.That(App.WaitForElement(LeftCountLabelId).GetText(), Is.EqualTo("4")); 

		App.SwipeLeftToRight(SwipeViewId);
		Assert.That(App.WaitForElement(RightCountLabelId).GetText(), Is.EqualTo("4")); 

		App.SwipeRightToLeft(SwipeViewId);
		Assert.That(App.WaitForElement(LeftCountLabelId).GetText(), Is.EqualTo("5")); 

		App.SwipeLeftToRight(SwipeViewId);
		Assert.That(App.WaitForElement(RightCountLabelId).GetText(), Is.EqualTo("5")); 

		App.SwipeLeftToRight(SwipeViewId);
		Assert.That(App.WaitForElement(RightCountLabelId).GetText(), Is.EqualTo("6")); 

		App.SwipeRightToLeft(SwipeViewId);
		Assert.That(App.WaitForElement(LeftCountLabelId).GetText(), Is.EqualTo("6")); 
	}
}
#endif