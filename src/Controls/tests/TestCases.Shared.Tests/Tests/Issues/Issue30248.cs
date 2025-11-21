#if MACCATALYST    //This is the Mac Specific issue, so restricting other platforms
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30248 : _IssuesUITest
{
	public override string Issue => "TitleBar, MacCatalyst - content is not aligned to left on fullscreen";

	public Issue30248(TestDevice device)
	: base(device)
	{ }

	[Test]
	[Category(UITestCategories.Window)]
	public void VerifyTitleBarContentinFullScreenmode()
	{
		App.WaitForElement("TitleBarAlignmentLabel");
		App.EnterFullScreen();
		VerifyScreenshot(includeTitleBar: true);
	}
}
#endif