using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34611 : _IssuesUITest
{
	public Issue34611(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Entry and Editor BackgroundColor not reset to Null";

	[Test]
	[Category(UITestCategories.Entry)]
	public void EntryAndEditorBackgroundColorShouldResetToDefaultWhenSetToNull()
	{
		App.WaitForElement("ApplyBackgroundColorButton");
		App.Tap("ApplyBackgroundColorButton");
		App.WaitForElement("TestEntry");
		App.Tap("ResetToDefaultButton");
		VerifyScreenshot();
	}
}
