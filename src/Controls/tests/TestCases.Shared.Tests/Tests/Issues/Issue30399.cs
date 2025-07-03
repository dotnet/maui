#if MACCATALYST && WINDOWS  // Titlebar applicable only on Windows and MacCatalyst
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30399 : _IssuesUITest
{
	public Issue30399(TestDevice device) : base(device)
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