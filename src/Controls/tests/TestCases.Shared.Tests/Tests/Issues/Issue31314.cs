using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue31314 : _IssuesUITest
{
	public Issue31314(TestDevice testDevice) : base(testDevice) { }

	public override string Issue => "LinearGradientBrush can not work";

	[Test]
	[Category(UITestCategories.Border)]
	public void LinearGradientBrushShouldWork()
	{
		App.WaitForElement("Label");
		VerifyScreenshot();
	}
}