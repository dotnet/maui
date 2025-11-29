using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue18933 : _IssuesUITest
{
	public Issue18933(TestDevice device) : base(device)
	{
	}

	public override string Issue => "ContentView Background Color Not Cleared When Set to Null";

	[Test, Order(1)]
	[Category(UITestCategories.ViewBaseTests)]
	public void VerifyBackgroundColorCleared()
	{
		App.WaitForElement("clearBgBtn");
		App.Tap("clearBgBtn");
		VerifyScreenshot();
	}

	[Test, Order(2)]
	[Category(UITestCategories.ViewBaseTests)]
	public void VerifyBackgroundColorSet()
	{
		App.WaitForElement("setBgBtn");
		App.Tap("setBgBtn");
		VerifyScreenshot();
	}
}