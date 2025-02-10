#if TEST_FAILS_ON_CATALYST // Using SwipeLeftToRight leads to exception of type 'OpenQA.Selenium.WebDriverException'. "Only pointer type 'mouse' is supported.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue8461 : _IssuesUITest
{
	const string ButtonId = "PageButtonId";
	const string LayoutId = "LayoutId";
	const string InstructionsLabel = "InstructionsLabel";

	public Issue8461(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Bug] [iOS] [Shell] Nav Stack consistency error";

	// [Test]
	// [Category(UITestCategories.Navigation)]

	public void ShellSwipeToDismiss()
	{
		App.WaitForElement(ButtonId);
		Assert.That(App.FindElements(ButtonId).Count, Is.EqualTo(1));

		App.Tap(ButtonId);

		App.WaitForElement(InstructionsLabel);
		Assert.That(App.FindElements(InstructionsLabel).Count, Is.EqualTo(1));

		// Swipe in from left across 1/2 of screen width
		App.SwipeLeftToRight(LayoutId, 0.99, 500, false);
		// Swipe in from left across full screen width
		App.SwipeLeftToRight(LayoutId);

		// On Android, swiping causes flyout items to overlap with the back arrow.
		// Touch actions may need to be performed twice to work around this issue.
#if ANDROID
		App.TapBackArrow();
#endif
		App.TapBackArrow();

		App.WaitForElement(ButtonId);
		Assert.That(App.FindElements(ButtonId).Count, Is.EqualTo(1));
	}
}
#endif