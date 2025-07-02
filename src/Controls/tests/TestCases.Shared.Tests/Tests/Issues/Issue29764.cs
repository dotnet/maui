#if TEST_FAILS_ON_WINDOWS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29764 : _IssuesUITest
{
	public Issue29764(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Shadows disappearing permanently in Android and Windows after Label opacity is at any time set to 0";

	[Test]
	[Category(UITestCategories.Label)]
	public void LabelShadowRemainsAfterOpacityChange()
	{
		App.WaitForElement("MainButton");
		App.Click("MainButton");
		VerifyScreenshot();
	}
}
#endif
