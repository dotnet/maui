using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue12246 : _IssuesUITest
{

	const string Entry = "Entry";
	const string Password = "Password";
	const string Success = "Success";

	public Issue12246(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Bug] iOS 14 App freezes when password is entered after email";

	[Test]
	[Category(UITestCategories.Entry)]
	public void UnfocusingPasswordDoesNotHang()
	{
		App.WaitForElement(Entry);

		App.EnterText(Entry, "test");
		App.DismissKeyboard();
		App.Tap(Password);
		App.EnterText(Password, "test");

		App.Tap(Entry);
		App.DismissKeyboard();
		App.WaitForElement(Success);
	}
}