#if IOS // This test is only for iOS because the issue is specifically related to on-screen keyboard behavior which is only available on mobile platforms.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue35902 : _IssuesUITest
{
	public Issue35902(TestDevice device) : base(device) { }

	public override string Issue => "[iOS] Transparent Shell Navigation Bar Breaks After Keyboard Interaction on Secondary Pages";

	[Test]
	[Category(UITestCategories.Shell)]
	public void TransparentShellNavBarShouldRemainTransparentAfterKeyboardDismiss()
	{
		App.WaitForElement("NavigateButton");
		App.Tap("NavigateButton");
		App.WaitForElement("TestEntry");
		App.Tap("TestEntry");
		App.DismissKeyboard();
		App.WaitForElement("TestEntry");
		VerifyScreenshot();
	}
}
#endif
