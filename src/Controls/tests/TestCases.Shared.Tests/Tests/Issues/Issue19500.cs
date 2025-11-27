using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue19500 : _IssuesUITest
{
	public override string Issue => "[iOS] Editor is not be able to scroll if IsReadOnly is true";

	const string yPositionLabel = "yPositionLabel";

	public Issue19500(TestDevice device) : base(device)
	{
	}

	[Test]
	[Category(UITestCategories.Editor)]
	public void TextInEditorShouldScroll()
	{
		var yPosLabel = App.WaitForElement(yPositionLabel);
		App.ScrollDown("editor");
#if MACCATALYST
		App.ScrollDown("editor"); // To make sure the editor is scrolled down
		var yPos = yPosLabel.GetText();
		Assert.That(yPos,Is.GreaterThan("0")); // The Y position should be greater than 0 after scrolling down
#else
		// The test passes if the text inside the editor scrolls down
		VerifyScreenshot();
#endif
	}
}