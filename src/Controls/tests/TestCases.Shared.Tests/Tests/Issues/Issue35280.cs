using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue35280 : _IssuesUITest
{
	public Issue35280(TestDevice device) : base(device) { }

	public override string Issue => "LinearGradientBrush with transparent stops renders as opaque black box on Android";

	[Test]
	[Category(UITestCategories.Brush)]
	public void LinearGradientBrushTransparentStopsShouldNotBeOpaque()
	{
		// Wait for the gradient container to be visible
		App.WaitForElement("DescriptionLabel");

		// The gradient has transparent/semi-transparent stops.
		// With the bug (GetGradientData(1.0f)), all stops are forced fully opaque → solid black box.
		// With the fix (GetGradientData(null)), per-stop alpha is preserved → the dotnet_bot.png image shows through.
		VerifyScreenshot(retryTimeout: TimeSpan.FromSeconds(2));
	}
}
