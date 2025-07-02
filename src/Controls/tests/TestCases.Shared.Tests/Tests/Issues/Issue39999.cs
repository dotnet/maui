# if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue39999 : _IssuesUITest
{
	public Issue39999(TestDevice device) : base(device)
	{
	}

	public override string Issue => "RTL FlowDirection causes overlap with window control buttons";

	[Test]
	[Category(UITestCategories.Window)]
	public void VerifyTitleBarRTL()
	{
		App.WaitForElement("ToggleFlowDirectionButton");
		App.Tap("ToggleFlowDirectionButton");
		App.Tap("ToggleFlowDirectionButton");
		VerifyScreenshot(includeTitleBar: true);
	}
}
#endif