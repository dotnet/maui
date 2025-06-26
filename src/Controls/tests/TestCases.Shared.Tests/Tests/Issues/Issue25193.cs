using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue25193 : _IssuesUITest
{
	public Issue25193(TestDevice device) : base(device) { }

	public override string Issue => "Background gradients don't work for some views";

	[Fact]
	[Trait("Category", UITestCategories.Brush)]
	public void BackgroundGradientsShouldRenderCorrectly()
	{
		App.WaitForElement("label");
		VerifyScreenshot();
	}
}