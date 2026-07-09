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
		App.WaitForElement("commandBar");

#if WINDOWS
		var titleRect = App.WaitForElement("Issue36322TitleView").GetRect();
		var commandBarRect = App.WaitForElement("commandBar").GetRect();

		var titleCenterX = titleRect.X + (titleRect.Width / 2d);
		var commandBarCenterX = commandBarRect.X + (commandBarRect.Width / 2d);

		Assert.That(titleCenterX, Is.EqualTo(commandBarCenterX).Within(2d),
			"TitleView is not centered inside the command bar on initial load.");
#endif

		VerifyScreenshot();
	}
}
