using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue20156 : _IssuesUITest
{
	public override string Issue => "Border has a 1 pixel thickness even when it's thickness property is set to 0";

	public Issue20156(TestDevice device)
		: base(device)
	{ }

	[Test]
	[Category(UITestCategories.Border)]
	[FailsOnMacWhenRunningOnXamarinUITest("VerifyScreenshot method not implemented on macOS")]
	public void BorderShouldHaveNoThickness()
	{
		_ = App.WaitForElement("WaitForStubControl");

		// The test passes if there's no gap between the borders
		VerifyScreenshot();
	}
}
