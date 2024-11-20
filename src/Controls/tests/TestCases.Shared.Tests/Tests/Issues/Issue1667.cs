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
		[FailsOnIOSWhenRunningOnXamarinUITest]
		[FailsOnMacWhenRunningOnXamarinUITest]
		public void TestCursorPositionAndSelection()
		{
			App.WaitForElement("CursorTextEntry");

			App.ClearText("CursorStart");
			App.EnterText("CursorStart", "2");
			App.ClearText("SelectionLength");
			App.EnterText("SelectionLength", "3");
			App.DismissKeyboard();
			App.Tap("Update");
			App.Screenshot("Text selection from char 2 length 3.");

			App.Tap("CursorTextEntry");
			var result = App.WaitForElement("SelectionLength").GetText();
			ClassicAssert.AreEqual("0", result);
		}

		[Test]
		[Category(UITestCategories.Entry)]
		[FailsOnAllPlatformsWhenRunningOnXamarinUITest]
		public void TestCursorColorOniOS()
		{
			App.WaitForElement("CursorTextEntry");
			App.Tap("Red");
			App.Tap("CursorTextEntry");
			App.Screenshot("Cursor is red.");

			App.Tap("Blue");
			App.Tap("CursorTextEntry");
			App.Screenshot("Cursor is blue.");

			App.Tap("Default");
			App.Tap("CursorTextEntry");
			App.Screenshot("Cursor is default color.");
		}
	}
}