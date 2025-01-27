using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue1426 : _IssuesUITest
{
	public Issue1426(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "SetHasNavigationBar screen height wrong";

	[Test]
	[Category(UITestCategories.LifeCycle)]
	[Category(UITestCategories.Compatibility)]
	public void Issue1426Test()
	{	
		App.WaitForElement("CoffeeImageId");
		App.WaitForElement("NextButtonID");
		VerifyScreenshot("Issue1426Test_BeforeTap");
		App.Tap("NextButtonID");
		App.WaitForElement("PopButtonId");
		App.Tap("PopButtonId");
		App.WaitForElement("CoffeeImageId");
		VerifyScreenshot("Issue1426Test_AfterTap");	
	}
}