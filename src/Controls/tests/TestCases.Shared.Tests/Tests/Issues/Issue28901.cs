using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue28901 : _IssuesUITest
{
	public override string Issue => "[Windows] Switch control is not sizing properly";

	public Issue28901(TestDevice device)
	: base(device)
	{ }

	[Test]
	[Category(UITestCategories.Switch)]
	public void VerifySwitchControlSize()
	{
		App.WaitForElement("SwitchControl");
		VerifyScreenshot();
	}
}