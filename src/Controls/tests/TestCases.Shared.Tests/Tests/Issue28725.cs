#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS //NullReferenceException throws on iOS and mac Issue link - https://github.com/dotnet/maui/issues/19642
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue28725 : _IssuesUITest
{
	public Issue28725(TestDevice testDevice) : base(testDevice) 
	{
	}

	public override string Issue => "ImagePaint is not rendering in View";

	[Test]
	[Category(UITestCategories.GraphicsView)]
	public void ImagePaintShouldRenderProperly()
	{
		Exception? exception = null;
	
		TestResizeMode("FitButton", "ImagePaintWithResizeModeFit", ref exception);
		TestResizeMode("BleedButton", "ImagePaintWithResizeModeBleed", ref exception);
		TestResizeMode("StretchButton", "ImagePaintWithResizeModeStretch", ref exception);

		if(exception != null)
		{
			throw exception;
		}

	}

	void TestResizeMode(string buttonId, string screenshotName, ref Exception? exception)
	{
		App.WaitForElement(buttonId);
		App.Click(buttonId);
		App.WaitForElement("Label");
		VerifyScreenshotOrSetException(ref exception, screenshotName);
		App.Click("Back");
	}
}
#endif