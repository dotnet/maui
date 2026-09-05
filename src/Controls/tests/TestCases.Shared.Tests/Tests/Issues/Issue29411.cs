#if TEST_FAILS_ON_CATALYST	&& TEST_FAILS_ON_WINDOWS			
// On Catalyst, Swipe actions not supported in Appium.
// On Windows: Dynamic loop changes were not working correctly (see issue-https://github.com/dotnet/maui/issues/29412), but this PR fixes that issue.
// However, when the loop value changes, the current item position is still not maintained.
// Therefore, this test is restricted on Windows.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29411 : _IssuesUITest
{
	public Issue29411(TestDevice device) : base(device) { }

	public override string Issue => "[Android] CarouselView.Loop = false causes crash on Android when changed at runtime";

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyLoopChangeAtRuntimeShouldNotCrash()
	{
		App.WaitForElement("CarouselView");
		App.Tap("Button");
		var text = App.FindElement("Label").GetText();
		Assert.That(text, Is.EqualTo("Item 1"));
		App.SwipeRightToLeft("CarouselView");
		App.WaitForElement("CarouselView");
	}
}

#endif