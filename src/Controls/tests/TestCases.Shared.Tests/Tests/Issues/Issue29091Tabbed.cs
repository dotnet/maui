using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29091Tabbed : _IssuesUITest
{
	public Issue29091Tabbed(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Tabbed - Auto Resize chrome icons on iOS to make it more consistent with other platforms - TabBar";

	[Test]
	[Category(UITestCategories.TabbedPage)]
	public void TabBarIconsShouldAutoscaleTabbedPage()
	{
		App.WaitForElement("Tab1Loaded");
		VerifyScreenshot();
	}
}