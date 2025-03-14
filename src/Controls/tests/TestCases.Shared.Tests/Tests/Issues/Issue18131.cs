using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.Shape)]
    public class Issue18131 : _IssuesUITest
{
	public override string Issue => "Color changes are not reflected in the Rectangle shapes";

	public Issue18131(TestDevice testDevice) : base(testDevice) { }

	protected override bool ResetAfterEachTest => true;

	[Test]
	public void CheckBackgroundColorUpdatesShapeBackgroundColor()
	{
		App.WaitForElement("ToggleBackgroundColor");
		VerifyScreenshot("InitialShapesBackgroundandFillColors");

		App.Click("ToggleBackgroundColor");
		VerifyScreenshot();
	}

	[Test]
	public void UncheckBackgroundColorUpdatesShapeBackgroundColor()
	{
		App.WaitForElement("ToggleBackgroundColor");
		VerifyScreenshot("InitialShapesBackgroundandFillColors");

		App.Click("ToggleBackgroundColor");
		App.Click("ToggleBackgroundColor");
		VerifyScreenshot();
	}

	[Test]
	public void CheckBackgroundUpdatesShapeBackground()
	{
		App.WaitForElement("ToggleBackground");
		VerifyScreenshot("InitialShapesBackgroundandFillColors");

		App.Click("ToggleBackground");
		VerifyScreenshot();
	}

	[Test]
	public void UncheckBackgroundUpdatesShapeBackground()
	{
		App.WaitForElement("ToggleBackground");
		VerifyScreenshot("InitialShapesBackgroundandFillColors");

		App.Click("ToggleBackground");
		App.Click("ToggleBackground");
		VerifyScreenshot();
	}

	[Test]
	public void CheckFillUpdatesShapeFill()
	{
		App.WaitForElement("ToggleFill");
		VerifyScreenshot("InitialShapesBackgroundandFillColors");

		App.Click("ToggleFill");
		VerifyScreenshot();
	}

	[Test]
	public void UncheckFillUpdatesShapeFill()
	{
		App.WaitForElement("ToggleFill");
		VerifyScreenshot("InitialShapesBackgroundandFillColors");

		App.Click("ToggleFill");
		App.Click("ToggleFill");
		VerifyScreenshot();
	}
}
