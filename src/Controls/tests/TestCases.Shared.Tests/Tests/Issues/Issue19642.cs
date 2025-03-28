using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue19642 : _IssuesUITest
{
	public Issue19642(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "NullReferenceException when using ImagePaint on Mac/iOS";

	[Test]
	[Category(UITestCategories.GraphicsView)]
	public void ImagePaintShouldBeDrawn()
	{
		App.WaitForElement("TestLabel");
		VerifyScreenshot();
	}
}
