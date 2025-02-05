using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.Navigation)]
public class SwipeBackNavCrash : _IssuesUITest
{
	public SwipeBackNavCrash(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Swipe back nav crash";

	[Test]
	public void SwipeBackNavCrashTestsAllElementsPresent()
	{
		App.WaitForElement("Page One");
		App.WaitForElement("Go to second page");
	}

	[Test]
	public void SwipeBackNavCrashTestsGoToSecondPage()
	{
		App.WaitForElement("Page One");
		App.Tap("Go to second page");
	}

#if !MACCATALYST //SwipeActions not working in Catalyst
	[Test]
	public void SwipeBackNavCrashTestsSwipeBackDoesNotCrash()
	{
		App.WaitForElement("Swipe lightly left and right to crash this page");
		App.SwipeLeftToRight();
		App.WaitForElement("Swipe lightly left and right to crash this page");
	}
#endif

}