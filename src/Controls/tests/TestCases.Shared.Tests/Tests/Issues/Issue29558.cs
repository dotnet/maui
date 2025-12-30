#if ANDROID ///This test only runs on Android because UnFocus behavior differs across platforms: on Android the input view should remain visible after UnFocus, whereas on iOS it automatically hides.

using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29558 : _IssuesUITest
{
	public Issue29558(TestDevice testDevice) : base(testDevice) { }

	public override string Issue => "Input controls should not change keyboard visibility";

	[Test]
	[Category(UITestCategories.SoftInput)]
	public void Issue29558InputViewShouldNotHideOnUnfocusCall()
	{
		bool isKeyboardVisible;
		App.WaitForElement("unfocusButton");

		List<string> inputControls = ["entry", "editor", "searchBar", "searchHandler"];
		foreach (var control in inputControls)
		{
			App.Click(control);
			isKeyboardVisible = App.IsKeyboardShown();
			Assert.That(isKeyboardVisible, Is.True);
			App.Click("unfocusButton");
			isKeyboardVisible = App.IsKeyboardShown();
			Assert.That(isKeyboardVisible, Is.True);
			App.DismissKeyboard();
			isKeyboardVisible = App.IsKeyboardShown();
			Assert.That(isKeyboardVisible, Is.False);
		}
	}
}
#endif