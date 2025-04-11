using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue21472 : _IssuesUITest
{
	public Issue21472(TestDevice testDevice) : base(testDevice)
	{
	}
	public override string Issue => "Shell FlyoutBackgroundImage doesn't shown";

	[Test]
	[Category(UITestCategories.Shell)]
	public void ShellFlyoutBackgroundImageVerify()
	{
		App.WaitForElement("label");
		VerifyScreenshot();
	}
}