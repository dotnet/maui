using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue39155 : _IssuesUITest
{
	public Issue39155(TestDevice device) : base(device) { }

	public override string Issue => "[iOS 27] NavigationPage.TitleView is 0pt tall (invisible) when pushed onto a NavigationPage whose bar was never laid out";


	[Test]
	[Category(UITestCategories.TitleView)]
	public void VerifyTitleViewRenderingOnNavigation()
	{
		App.WaitForElement("Replace Window.Page");
		App.Tap("Replace Window.Page");
		App.WaitForElement("PushTitleViewPage");
		App.Tap("PushTitleViewPage");
		App.WaitForElement("TitleView");

		VerifyScreenshot();
	}
}