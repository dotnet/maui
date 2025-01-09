using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue1667 : _IssuesUITest
	{
		public Issue1667(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Entry: Position and color of caret";

		[Test]
		[Category(UITestCategories.Entry)]
		[Category(UITestCategories.Compatibility)]
		public void TestCursorPositionAndSelection()
		{
			App.WaitForElement("CursorTextEntry");

			App.ClearText("CursorStart");
			App.EnterText("CursorStart", "2");
			App.ClearText("SelectionLength");
			App.EnterText("SelectionLength", "3");
			App.DismissKeyboard();
			App.Tap("Update");
			App.Tap("CursorTextEntry");
			var result = App.WaitForElement("SelectionLength").GetText();
			ClassicAssert.AreEqual("0", result);
		}

// Verify cursor color changes in the Entry field on iOS when buttons are tapped, as specified for DevicePlatform.iOS in the host app sample.
#if IOS
		[Test]
		[Category(UITestCategories.Entry)]
		public void TestCursorColorOniOS()
		{
			App.WaitForElement("CursorTextEntry");
			App.Tap("Red");
			App.Tap("CursorTextEntry");
			VerifyScreenshot("CursorRedColor");

			App.Tap("Blue");
			App.Tap("CursorTextEntry");
			VerifyScreenshot("CursorBlueColor.");

			App.Tap("Default");
			App.Tap("CursorTextEntry");
			VerifyScreenshot("CursorDefaultColor");
		}
#endif
	}
}