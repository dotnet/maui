using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue14181 : _IssuesUITest
{
	public Issue14181(TestDevice device) : base(device) { }

	public override string Issue => "[iOS] RadioButton BackgroundColor bleeds outside CornerRadius rounded corners";

	[Test]
	[Category(UITestCategories.RadioButton)]
	public void RadioButtonBackgroundColorClippedToCornerRadius()
	{
		App.WaitForElement("RadioButtonWithBackground");
		VerifyScreenshot();
	}
}
