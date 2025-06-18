using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue1 : _IssuesUITest
{
	public override string Issue => "Page Busy indicator should be visible";

	public Issue1(TestDevice device)
	: base(device)
	{ }

	[Test]
	[Category(UITestCategories.Page)]
	public void VerifyPageBusyIndicatorIsVisible()
	{
		App.WaitForElement("SetIsBusyButton");
		App.Tap("SetIsBusyButton");
		App.PageBusyIndicatorIsVisible();
		App.Tap("SetIsBusyButton");
		App.PageBusyIndicatorIsNotVisible();
	}
}