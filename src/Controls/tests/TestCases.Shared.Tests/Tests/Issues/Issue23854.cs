using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue23854 : _IssuesUITest
{
	public Issue23854(TestDevice testDevice) : base(testDevice) { }

	public override string Issue => "ImageButton CornerRadius not being applied on Android";

	[Test]
	[Category(UITestCategories.ImageButton)]
	public void CornerRadiusShouldBeApplied()
	{
		App.WaitForElement("ImageButton");
		VerifyScreenshot();
	}
}
