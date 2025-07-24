#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_CATALYST //In windows and mac catalyst, keyboard won't visible on focus and unfocus
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29559 : _IssuesUITest
{
	public Issue29559(TestDevice testDevice) : base(testDevice) { }

	public override string Issue => "Add An API to SearchHandler so users can hide or show the softkeyboard";

	[Test]
	[Category(UITestCategories.Shell)]
	public void Issue29559KeyboardDismissonSearchHander()
	{
		App.WaitForElement("ShowKeyboardButton");
		App.Click("ShowKeyboardButton");
		bool IskeyboardVisible = App.IsKeyboardShown();
		Assert.That(IskeyboardVisible, Is.True);

		App.WaitForElement("HideKeyboardButton");
		App.Click("HideKeyboardButton");

		IskeyboardVisible = App.IsKeyboardShown();
		Assert.That(IskeyboardVisible, Is.False);
	}
}
#endif