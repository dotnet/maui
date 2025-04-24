using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue16767_Downsize : _IssuesUITest
{
	public Issue16767_Downsize(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Downsize function in W2DImage class";

	[Test]
	[Category(UITestCategories.GraphicsView)]
	public void ImagePaintWithDownsize()
	{
		App.WaitForElement("descriptionLabel");
		VerifyScreenshot();
	}
}