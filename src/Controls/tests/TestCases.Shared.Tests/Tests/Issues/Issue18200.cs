using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue18200 : _IssuesUITest
{
	public Issue18200(TestDevice device)
		: base(device)
	{ }

	public override string Issue => "Flyout Page SetCollapseStyle doesn't have any change";

	[Test]
	[Category(UITestCategories.FlyoutPage)]
	public void VerifyFlyoutCollapseStyleBehaviorChanges()
	{
		App.WaitForElement("CollapseStyleButton");
		App.Tap("CollapseStyleButton");
		App.TapFlyoutPageIcon();
	}
}
