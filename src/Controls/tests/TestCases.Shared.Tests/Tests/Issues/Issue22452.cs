using System.Diagnostics;
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue22452 : _IssuesUITest
{
	public Issue22452(TestDevice device) : base(device) { }

	public override string Issue => "Fix error when running new template maui app on iOS";

	[Fact]
	[Category(UITestCategories.Shell)]
	public void NavigationBetweenFlyoutItems()
	{
		App.WaitForElement("TabContent");
		VerifyScreenshot();
	}
}