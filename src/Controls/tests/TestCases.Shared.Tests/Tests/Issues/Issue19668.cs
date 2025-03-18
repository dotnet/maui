using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue19668:_IssuesUITest
{
	public Issue19668(TestDevice testDevice) : base(testDevice)
	{
	}
	public override string Issue => "BoxView Placement inside Border.";

	[Test]
	[Category(UITestCategories.Border)]
	public void BoxViewPlacementInsideBorder()
	{
		App.WaitForElement("Border");
		VerifyScreenshot();
	}
}
