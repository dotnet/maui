using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30052 : _IssuesUITest
{
	public Issue30052(TestDevice testDevice) : base(testDevice) { }

	public override string Issue => "Right-To-Left (RTL) alignment is not applied to Editor placeholder";

	[Test]
	[Category(UITestCategories.Editor)]
	public void RTLShouldBeAppliedToPlaceholers()
	{
		App.WaitForElement("Editor");
		VerifyScreenshot();
	}
}