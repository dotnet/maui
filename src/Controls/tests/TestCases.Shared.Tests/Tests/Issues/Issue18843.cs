using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue18843 : _IssuesUITest
{
	public override string Issue => "[Android] Wrong left margin in the navigation bar";

	public Issue18843(TestDevice device)
		: base(device)
	{ }

	[Category(UITestCategories.TitleView)]
	public void NoLeftMarginShouldBeShown()
	{
		_ = App.WaitForElement("WaitForStubControl");

		//Test passes if no the whole navigation bar is red
		VerifyScreenshot();
	}
}