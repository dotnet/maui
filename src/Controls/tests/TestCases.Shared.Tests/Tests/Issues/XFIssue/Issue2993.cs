using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue2993 : _IssuesUITest
{
	public Issue2993(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Android] Bottom Tab Bar with a navigation page is hiding content";

	[Test]
	[Category(UITestCategories.Layout)]
	public void BottomContentVisibleWithBottomBarAndNavigationPage()
	{
		App.WaitForElement("BottomText");
	}
}