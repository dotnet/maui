#if !MACCATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue21645 : _IssuesUITest
{
	public Issue21645(TestDevice device) : base(device) { }

	public override string Issue => "Android - Navigation Title disappears if Flyout is set to a new window";

	[Test]
	[Category(UITestCategories.FlyoutPage)]
	public void VerifyToolbarButtonVisibilityWhenFlyoutReset()
	{
		App.WaitForElement("ResetFlyoutButton");
		App.Click("ResetFlyoutButton");
		VerifyScreenshot();
	}
}
#endif