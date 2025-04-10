#if TEST_FAILS_ON_MACCATALYST || TEST_FAILS_ON_IOS //Issue link - https://github.com/dotnet/maui/issues/19642
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
		Exception? fitException = null;
		Exception? bleedException = null;
		Exception? stretchException = null;

		TestResizeMode("FitButton", "ImagePaintWithResizeModeFit", ref fitException);
		TestResizeMode("BleedButton", "ImagePaintWithResizeModeBleed", ref bleedException);
		TestResizeMode("StretchButton", "ImagePaintWithResizeModeStretch", ref stretchException);

		if (fitException != null)
			throw fitException;
		if (bleedException != null)
			throw bleedException;
		if (stretchException != null)
			throw stretchException;
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