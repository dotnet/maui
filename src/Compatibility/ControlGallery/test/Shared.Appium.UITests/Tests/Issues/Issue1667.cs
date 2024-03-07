using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace UITests
{
	public class Issue1667 : IssuesUITest
	{
		public Issue1667(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Entry: Position and color of caret";

		[Test]
		[Category(UITestCategories.Entry)]
		public void TestCursorPositionAndSelection()
		{
			RunningApp.WaitForElement("CursorTextEntry");

			RunningApp.ClearText("CursorStart");
			RunningApp.EnterText("CursorStart", "2");
			RunningApp.ClearText("SelectionLength");
			RunningApp.EnterText("SelectionLength", "3");
			RunningApp.DismissKeyboard();
			RunningApp.Tap("Update");
			RunningApp.Screenshot("Text selection from char 2 length 3.");

			RunningApp.Tap("CursorTextEntry");
			var result = RunningApp.WaitForElement("SelectionLength").GetText();
			ClassicAssert.AreEqual("0", result);
		}

		[Test]
		[Category(UITestCategories.Entry)]
		public void TestCursorColorOniOS()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForElement("CursorTextEntry");
			RunningApp.Tap("Red");
			RunningApp.Tap("CursorTextEntry");
			RunningApp.Screenshot("Cursor is red.");

			RunningApp.Tap("Blue");
			RunningApp.Tap("CursorTextEntry");
			RunningApp.Screenshot("Cursor is blue.");

			RunningApp.Tap("Default");
			RunningApp.Tap("CursorTextEntry");
			RunningApp.Screenshot("Cursor is default color.");
		}
	}
}