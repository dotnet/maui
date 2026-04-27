using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32316 : _IssuesUITest
{

	public Issue32316(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "RTL mode: Padding for the label is not mirroring properly";

	[Test]
	[Category(UITestCategories.Label)]
	public void RTLModePaddingShouldWork()
	{
		App.WaitForElement("ToggleFlowDirectionButton");
		App.Tap("ToggleFlowDirectionButton");
		VerifyScreenshot();
	}
}