using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32200 : _IssuesUITest
{
	public override string Issue => "NavigationPage TitleView iOS 26";
	public Issue32200(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.Navigation)]
	public void NavagionPageTitleViewShouldRespectMargins()
	{
		App.WaitForElement("Label");
		VerifyScreenshot();
	}
}
