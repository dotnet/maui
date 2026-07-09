using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue36322 : _IssuesUITest
{
	public Issue36322(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Shell.TitleView is not centered on initial load on Windows";

	[Test]
	[Category(UITestCategories.TitleView)]
	public void ShellTitleViewIsCenteredOnInitialLoad()
	{
		App.WaitForElement("Issue36322Instructions");
		App.WaitForElement("Issue36322TitleView");

		VerifyScreenshot();
	}
}
