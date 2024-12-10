using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue1583 : _IssuesUITest
{
	public Issue1583(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "NavigationPage.TitleIcon broken";

	[Test]
	[Category(UITestCategories.Navigation)]
	[Category(UITestCategories.Compatibility)]
	[FailsOnIOSWhenRunningOnXamarinUITest]
	[FailsOnMacWhenRunningOnXamarinUITest]
	[FailsOnWindowsWhenRunningOnXamarinUITest]
	public void Issue1583TitleIconTest()
	{
		App.WaitForElement("lblHello");
	}
}