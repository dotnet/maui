using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue23832 : _IssuesUITest
{
	public Issue23832(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Some HEIC photos is upside down after using PlatformImage.Resize";

	[Test]
	[Category(UITestCategories.GraphicsView)]
	public void HEICImageShouldNotRenderUpsideDown()
	{
		App.WaitForElement("DrawableLabel");
		VerifyScreenshot();
	}
}