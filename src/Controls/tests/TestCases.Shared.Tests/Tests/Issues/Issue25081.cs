#if WINDOWS || MACCATALYST // TitleBar is only supported on Windows and MacCatalyst
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue25081 : _IssuesUITest
{
	public override string Issue => "[Windows] The flyout icon and background appear awkward when enabled alongside a TitleBar";

	public Issue25081(TestDevice device)
	: base(device)
	{ }

	[Test, Order(1)]
	[Category(UITestCategories.Shell)]
	public async Task VerifyFlyoutIconBackgroundColor()
	{
		App.WaitForElement("ColorChangeButton");
		VerifyScreenshot(includeTitleBar: true);
	}

	[Test, Order(2)]
	[Category(UITestCategories.Shell)]
	public async Task VerifyDynamicFlyoutIconBackgroundColor()
	{

		App.WaitForElement("ColorChangeButton");
		App.Tap("ColorChangeButton");
		VerifyScreenshot(includeTitleBar: true);
	}
}
#endif