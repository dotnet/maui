#if IOS
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
	public void Github1426Test()
	{
		App.Screenshot("You can see the coffe mug");
		App.WaitForElement("CoffeeImageId");
		App.WaitForElement("NextButtonID");
		App.Tap("NextButtonID");
		App.WaitForElement("PopButtonId");
		App.Tap("PopButtonId");
		App.WaitForElement("CoffeeImageId");
		App.Screenshot("Coffe mug Image is still there on the bottom");
	}
}
#endif