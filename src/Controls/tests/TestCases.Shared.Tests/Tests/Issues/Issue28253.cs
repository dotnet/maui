using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue28253 : _IssuesUITest
{
	public Issue28253(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Image is not loading in RadioButton (RadioButton.Content)";

	[Test]
	[Category(UITestCategories.RadioButton)]
	public void RadioButtonsShouldHaveImages()
	{
		App.WaitForElement("label");
		VerifyScreenshot();
	}
}
