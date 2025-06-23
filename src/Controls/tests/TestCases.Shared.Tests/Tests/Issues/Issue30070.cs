using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

#if IOS || MACCATALYST
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
		App.SwipeLeftToRight("TestScrollView");
		VerifyScreenshot();
	}
}
#endif