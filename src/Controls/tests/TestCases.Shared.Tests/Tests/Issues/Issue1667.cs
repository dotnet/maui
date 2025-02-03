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
			Assert.That(result, Is.EqualTo("0"));
		}

		// This test is iOS-specific due to platform-specific cursor color setting in the sample.
		// Note: Visual validation of cursor color is required as automation is challenging
		// due to cursor blinking, which prevents reliable image comparison.
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