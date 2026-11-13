using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue16767_Resize : _IssuesUITest
{
	public Issue16767_Resize(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Resize function in W2DImage class";
	protected override bool ResetAfterEachTest => true;

	[Test]
	[Category(UITestCategories.GraphicsView)]
	public void ImagePaintWithResizeModeFit()
	{
		App.WaitForElement("ResizeModeFit");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.GraphicsView)]
	public void ImagePaintWithResizeModeBleed()
	{
		App.WaitForElement("ResizeModeBleed");
		App.Tap("ResizeModeBleed");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.GraphicsView)]
	public void ImagePaintWithResizeModeStretch()
	{
		App.WaitForElement("ResizeModeStretch");
		App.Tap("ResizeModeStretch");
		VerifyScreenshot();
	}
}