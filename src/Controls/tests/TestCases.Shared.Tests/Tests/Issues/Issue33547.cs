#if IOS || ANDROID // This test is only for Android and iOS because the issue is specifically related to on-screen keyboard behavior which is only available on mobile platforms.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33547 : _IssuesUITest
{
	public override string Issue => "[iOS] Shell Page gets moved partially outside of viewport when focusing element on page load";

	public Issue33547(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.Shell)]
	public void PageShouldNotMoveOutsideViewportWhenPressingEnterOnKeyboard()
	{
		App.WaitForElement("NavigationPushButton");
		App.Tap("NavigationPushButton");

		App.WaitForElement("TestEntry");
		App.DismissKeyboard();

		VerifyScreenshot();
	}
}
#endif