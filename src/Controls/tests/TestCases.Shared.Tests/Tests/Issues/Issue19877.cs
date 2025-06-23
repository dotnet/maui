using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue19877 : _IssuesUITest
{
	public Issue19877(TestDevice device) : base(device) { }

	public override string Issue => "RoundRectangle Border is messed up when contains an Image with AspectFill";

	[Test]
	[Category(UITestCategories.Border)]
	public void BorderRoundRectangleWithImage()
	{
		App.WaitForElement("OasisImage");

		VerifyScreenshot();
	}
}
