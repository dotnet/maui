using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34459 : _IssuesUITest
{
	public override string Issue => "Android Label word wrapping clips text depending on alignment and layout options";

	public Issue34459(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.Label)]
	public void LabelWordWrapNotClippedWithRtlFlowDirection()
	{
		App.WaitForElement("RtlContainer");
		VerifyScreenshot();
	}
}
