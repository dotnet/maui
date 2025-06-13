using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29562 : _IssuesUITest
{
	public Issue29562(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Maui.Graphics GetStringSize Inverts Width and Height";

	[Fact]
	[Category(UITestCategories.GraphicsView)]
	public void GraphicsViewShouldNotWrapText()
	{
		App.WaitForElement("Label");
		VerifyScreenshot();
	}
}