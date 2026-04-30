using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34842 : _IssuesUITest
{
	public Issue34842(TestDevice device) : base(device) { }

	public override string Issue => "[iOS] RadioButton BackgroundColor bleeds outside CornerRadius rounded corners";

	[Test]
	[Category(UITestCategories.RadioButton)]
	public void RadioButtonBackgroundColorClippedToCornerRadius()
	{
		App.WaitForElement("RadioButtonWithBackground");
		VerifyScreenshot();
	}
}
