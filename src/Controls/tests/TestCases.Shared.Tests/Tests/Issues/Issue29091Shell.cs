using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29091Shell : _IssuesUITest
{
	public Issue29091Shell(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Shell - Auto Resize chrome icons on iOS to make it more consistent with other platforms - TabBar";

	[Test]
	[Category(UITestCategories.Shell)]
	public void TabBarIconsShouldAutoscaleShell()
	{
		App.WaitForElement("Tab1");
		VerifyScreenshot();
	}
}