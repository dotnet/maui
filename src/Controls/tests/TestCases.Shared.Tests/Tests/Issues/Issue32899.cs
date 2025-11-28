using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32899 : _IssuesUITest
{
	public Issue32899(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Dotnet bot image is not showing up when using iOS 26 and macOS 26.1";

	[Test]
	[Category(UITestCategories.Shell)]
	public void TitleViewImageShouldBeVisible()
	{
		// Wait for the page to load
		App.WaitForElement("InstructionLabel");

		// Verify the ImageButton in TitleView is present
		// The image itself should be rendered and visible
		App.WaitForElement("TitleImageButton");

		// Take a screenshot to verify the image is displayed
		VerifyScreenshot();
	}
}
