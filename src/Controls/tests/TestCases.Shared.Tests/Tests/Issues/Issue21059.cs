#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue21059 : _IssuesUITest
{
	public override string Issue => "[iOS] Disabled Editors Show Keyboard Partially When Tapped";

	public Issue21059(TestDevice device)
		: base(device)
	{ }

    [Test]
	[Category(UITestCategories.Editor)]
	public async Task ToolbarShouldNotBeVisibleAfterClickingDisabledEditor()
	{
		_ = App.WaitForElement("editor");
		App.Click("editor");

		// The animation needs to finish for the toolbar to be displayed if it doesn't work
		await Task.Delay(500);

		// The test passes if no toolbar is dislayed at the bottom of the page
		VerifyScreenshot();       
	}
}
#endif