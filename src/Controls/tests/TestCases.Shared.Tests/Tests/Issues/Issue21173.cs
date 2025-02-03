using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue21173 : _IssuesUITest
{
	public override string Issue => "[Android] Border with RoundRectangle Stroke does not correctly round corners of things within it";

	public Issue21173(TestDevice device)
		: base(device)
	{ }

	[Test]
	[Category(UITestCategories.Border)]
	public void BorderWithRoundRectangleShouldRoundCornersOfContentWithinIt()
	{
		_ = App.WaitForElement("image");

		// The test passes if corners of borders' contents' have proper corner radiuses
		VerifyScreenshot();
	}
}
