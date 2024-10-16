using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue12246 : _IssuesUITest
{
	public Issue12246(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Bug] iOS 14 App freezes when password is entered after email";

	// [Test]
	// [Category(UITestCategories.Entry)]
	// public void UnfocusingPasswordDoesNotHang()
	// {
	// 	RunningApp.WaitForElement(Entry);
	// 	RunningApp.WaitForElement(Password);

	// 	RunningApp.EnterText(Entry, "test");
	// 	RunningApp.DismissKeyboard();
	// 	RunningApp.EnterText(Password, "test");

	// 	RunningApp.Tap(Entry);
	// 	RunningApp.DismissKeyboard();
	// 	RunningApp.WaitForElement(Success);
	// }
}