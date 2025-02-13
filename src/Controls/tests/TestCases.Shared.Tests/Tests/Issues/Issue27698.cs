#if IOS // iOS Specific issue.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue27698: _IssuesUITest
{
	public override string Issue => "Editor with AutoSize=EditorAutoSizeOption.TextChanges bad performance on iOS";

	public Issue27698(TestDevice device) : base(device)
	{
	}
	
	[Test]
	[Category(UITestCategories.Editor)]
	public void UpdatedEditorTextVerifyLayout()
	{
		App.WaitForElement("Test001");
		App.EnterText("Test001", "Lorem ipsum dolor sit amet, consectetur adipiscing elit,");
		App.PressEnter();
		App.EnterText("Test001", "sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.");

		for (int i = 0; i < 3; i++)
		{
			App.Tap("UpdateFontSizeButton");
			App.Tap("UpdateCharacterSpacingButton");
		}

		VerifyScreenshot();	
	}
}
#endif