using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue28098 : _IssuesUITest
{
	public override string Issue => "Returning back from navigation to MainPage would result in a blank screen";
	public Issue28098(TestDevice device) : base(device)
	{
	}

	[Test]
	[Category(UITestCategories.Picker)]
	public void BlankScreenOnNavigationBack()
	{
		App.WaitForElement("Button");
		App.Tap("Button");
		App.WaitForElement("BackButton");
		App.Tap("BackButton");
		VerifyScreenshot();
	}
}