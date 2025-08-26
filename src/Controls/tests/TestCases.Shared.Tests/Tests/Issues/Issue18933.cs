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

	[Test]
	[Category(UITestCategories.ViewBaseTests)]
	public void VerifyBackgroundColorCleared()
	{
		App.WaitForElement("Issue18933Btn");
		App.Tap("Issue18933Btn");
		VerifyScreenshot();
	}
}