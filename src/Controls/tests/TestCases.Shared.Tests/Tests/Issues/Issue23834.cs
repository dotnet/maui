using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue23834 : _IssuesUITest
{
	public Issue23834(TestDevice device) : base(device) { }

	public override string Issue => "Flyout Item misbehavior";

	[Test]
	[Category(UITestCategories.FlyoutPage)]
	public void Issue23834FlyoutMisbehavior()
	{
		App.WaitForElement("button");
		App.Tap("button");
		App.ShowFlyout();
		VerifyScreenshot();
	}
}