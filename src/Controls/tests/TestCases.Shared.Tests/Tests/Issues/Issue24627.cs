#if WINDOWS || MACCATALYST // TitleBar is only supported on Windows and MacCatalyst
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue24627 : _IssuesUITest
{
	public override string Issue => "[Windows] TitleBar Title FontAttributes";

	public Issue24627(TestDevice device)
	: base(device)
	{ }

	[Test, Order(1)]
	[Category(UITestCategories.TitleView)]
	public void VerifyTitleBarTitleFontAttributesBold()
	{
		App.WaitForElement("ChangeFAButton");
		VerifyScreenshot(includeTitleBar: true);
	}

	[Test, Order(2)]
	[Category(UITestCategories.TitleView)]
	public void ChangeTitleBarTitleFontAttributesToNone()
	{
		App.WaitForElement("ChangeFAButton");
		App.Tap("ChangeFAButton");
		VerifyScreenshot(includeTitleBar: true);
	}
}
#endif