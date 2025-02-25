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
		var beforeRect = App.WaitForElement("CoffeeImageId").GetRect();
		App.WaitForElement("NextButtonID");
		App.Tap("NextButtonID");
		App.WaitForElement("PopButtonId");
		App.Tap("PopButtonId");
		var afterRect = App.WaitForElement("CoffeeImageId").GetRect();
		Assert.That(beforeRect.X, Is.EqualTo(afterRect.X));
		Assert.That(beforeRect.Y, Is.EqualTo(afterRect.Y));
	}
}