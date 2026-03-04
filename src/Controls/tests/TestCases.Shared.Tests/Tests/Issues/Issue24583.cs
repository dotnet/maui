using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue24583 : _IssuesUITest
{
	public override string Issue => "Text in the Editor control disappeared when reducing the Scale";

	public Issue24583(TestDevice testDevice) : base(testDevice)
	{
	}

	[Test]
	[Category(UITestCategories.Editor)]
	public void TextInEditorShouldBeCorrectlyPositionedAfterResizing()
	{
		App.WaitForElement("button");
		VerifyScreenshot("TextsInEditorsBeforeScaling");
		App.Click("button");
		VerifyScreenshot("TextsInEditorsAfterScaling");
	}
}