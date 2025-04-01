using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue17782 : _IssuesUITest
{
	public Issue17782(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[ManualMauiTests] New text in the Editor character spacing test sometimes uses the previous spacing";

	[Test]
	[Category(UITestCategories.Editor)]
	public void VerifyEditorCharacterSpacingWithText()
	{
		App.WaitForElement("SliderCharacterSpacing");
		App.Click("ButtonAddEditorText");
		App.Click("ButtonUpdateCharacterSpacing");
		App.EnterText("DynamicCharacterSpacingEditor", "Text");
		App.Click("ButtonResetCharacterSpacing");
		App.EnterText("ResetCharacterSpacingEditor", "Text");
		App.Click("EditorsUnfocusButton");
#if IOS
		App.DismissKeyboard();
#endif
		VerifyScreenshot();
	}
}