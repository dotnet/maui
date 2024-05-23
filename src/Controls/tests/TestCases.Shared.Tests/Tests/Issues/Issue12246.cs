#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
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
		[Category(UITestCategories.Compatibility)]
		[FailsOnIOS]
		public void UnfocusingPasswordDoesNotHang()
		{
			App.WaitForElement(Entry);
			App.WaitForElement(Password);

			App.EnterText(Entry, "test");
			App.DismissKeyboard();
			App.EnterText(Password, "test");

			App.Tap(Entry);
			App.DismissKeyboard();
			App.WaitForElement(Success);
		}
	}
}
#endif