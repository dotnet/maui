#if ANDROID || IOS // Desktop platforms do not have a soft keyboard, so the test is restricted to Android and iOS.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue27236 : _IssuesUITest
	{
		public Issue27236(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "android allows type into hidden Entry control";

		[Test]
		[Category(UITestCategories.Entry)]
		public void VerifyEntryKeyboardVisibilityToggle()
		{
			App.WaitForElement("ToggleEntryVisibilityButton");
			App.Tap("ToggleEntryVisibilityButton");
			var element = App.WaitForElement("ToggleableEntry");
			element.SendKeys("Hello, Entry");
			App.Tap("ToggleEntryVisibilityButton");
			var keyboardVisible = App.IsKeyboardShown();
			Assert.That(keyboardVisible, Is.False);
		}

		[Test]
		[Category(UITestCategories.Editor)]
		public void VerifyEditorKeyboardVisibilityToggle()
		{
			App.WaitForElement("ToggleEditorVisibilityButton");
			App.Tap("ToggleEditorVisibilityButton");
			var element = App.WaitForElement("ToggleableEditor");
			element.SendKeys("Hello, Editor");
			App.Tap("ToggleEditorVisibilityButton");
			var keyboardVisible = App.IsKeyboardShown();
			Assert.That(keyboardVisible, Is.False);
		}
	}
}
#endif