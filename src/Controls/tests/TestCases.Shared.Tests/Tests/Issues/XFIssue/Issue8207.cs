using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue8207 : _IssuesUITest
{
	public Issue8207(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Bug] Shell Flyout Items on UWP aren't showing the Title";

	[Test]
	[Category(UITestCategories.Shell)]
	public void FlyoutItemShouldShowTitle()
	{
		App.TapInShellFlyout("Dashboard");
		App.WaitForElement("Control");
	}
}