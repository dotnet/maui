using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue17550 : _IssuesUITest
{
	public Issue17550(TestDevice device) : base(device) { }
	public override string Issue => "Changing Shell.NavBarIsVisible does not update the nav bar on Mac / iOS";

	[Test, Order(1)]
	[Category(UITestCategories.Shell)]
	public void VerifyNavBarStatusAtInitialLoading()
	{
		App.WaitForElement("NavBarToggleButton");
		VerifyScreenshot();
	}

	[Test, Order(2)]
	[Category(UITestCategories.Shell)]
	public void VerifyNavBarStatusAtRuntime()
	{
		App.WaitForElement("NavBarToggleButton");
		App.Tap("NavBarToggleButton");
		VerifyScreenshot();
	}
}
