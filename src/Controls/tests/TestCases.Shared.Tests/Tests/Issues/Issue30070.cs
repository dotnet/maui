#if IOS || MACCATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30070 : _IssuesUITest
{
	public Issue30070(TestDevice device) : base(device)
	{
	}

	public override string Issue => "ScrollView Orientation set to Horizontal allows both horizontal and vertical scrolling";

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void ScrollViewOrientationTest()
	{
		App.WaitForElement("TestScrollView");
		App.ScrollDown("TestScrollView");
		VerifyScreenshot();
	}
}
#endif