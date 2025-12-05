using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33008 : _IssuesUITest
{
	public override string Issue => "SearchBar and SearchHandler ShowsCancelButton property";

	public Issue33008(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.SearchBar)]
	public void SearchBarShowsCancelButtonWorks()
	{
		App.WaitForElement("TitleLabel");
		App.Tap("SetTextButton");
		App.EnterText("Search...", "Test text");

		var status = App.FindElement("StatusLabel").GetText();
		Assert.That(status, Does.Contain("Text set on all SearchBars"), "Should set text on all SearchBars");

#if IOS || MACCATALYST
		// Verify that the Cancel button is visible on SearchBars where ShowsCancelButton is true
		VerifyScreenshot();
#endif
	}
}
