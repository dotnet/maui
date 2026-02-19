using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue25747 : _IssuesUITest
{
	public Issue25747(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Updating a Border.TranslationY does not work";

	[Test]
	[Category(UITestCategories.Border)]
	[Category(UITestCategories.InputTransparent)]
	public void TranslationYShouldWork()
	{
		App.WaitForElement("Button");
		App.Click("Button");
		App.Click("Button");

		VerifyScreenshot();
	}
}