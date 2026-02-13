using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue19676 : _IssuesUITest
{
	public override string Issue => "Android Switch Control Thumb Shadow missing when ThumbColor matches background";

	public Issue19676(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.Switch)]
	public void SwitchThumbShouldBeVisibleWithShadow()
	{
		App.WaitForElement("TestSwitch");
		VerifyScreenshot();
	}
}