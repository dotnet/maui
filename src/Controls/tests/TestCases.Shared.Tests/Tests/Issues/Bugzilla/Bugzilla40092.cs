using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla40092 : _IssuesUITest
{
	public Bugzilla40092(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Ensure android devices with fractional scale factors (3.5) don't have a white line around the border";


	[Test]
	[Category(UITestCategories.BoxView)]
	public void AllScreenIsBlack()
	{
		App.WaitForElement("black");
		VerifyScreenshot();
	}
}