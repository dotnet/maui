using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue31182 : _IssuesUITest
{
	public Issue31182(TestDevice testDevice) : base(testDevice) { }

	public override string Issue => "GraphicsView draws at half size after canvas.ResetState()";

	[Test]
	[Category(UITestCategories.GraphicsView)]
	public void GraphicsViewShouldDrawAtFullSize()
	{
		App.WaitForElement("label");
		VerifyScreenshot();
	}
}