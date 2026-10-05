#if MACCATALYST || WINDOWS  // Native window controls (minimize, maximize, close) are applicable only on Windows and MacCatalyst
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;
namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32476 : _IssuesUITest
{
	public Issue32476(TestDevice testDevice) : base(testDevice)
	{
	}
	public override string Issue => "Binding RTL FlowDirection in Shell causes Flyout MenuIcon and native window controls to overlap";
	[Test]
	[Category(UITestCategories.Shell)]
	public void ShellRTLFlowDirectionShouldNotCauseOverlap()
	{
		App.WaitForElement("ToggleButton");
		App.Tap("ToggleButton");
		App.WaitForElement("FlowDirection is now RTL");

		if (App.GetTestDevice() == TestDevice.Mac)
		{
			App.WaitForElement(() =>
			{
				var menu = App.FindElement("OK");
				var windowBounds = App.FindElement(AppiumQuery.ByXPath("//XCUIElementTypeWindow[@identifier='SceneWindow']")).GetRect();
				return menu.GetRect().Left > windowBounds.Left + windowBounds.Width / 2
					? menu : null;
			}, "The native flyout button did not move to the right after switching to RTL");
		}

		VerifyScreenshot(includeTitleBar: true);
	}
}
#endif
