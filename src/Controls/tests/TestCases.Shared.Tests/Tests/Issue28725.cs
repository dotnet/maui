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
	protected override bool ResetAfterEachTest =>  true;

	const string Label = "Label";
	
	[Test, Order(1)]
	[Category(UITestCategories.GraphicsView)]
	public void ImagePaintResizeFit()
	{
		NavigateAndVerify("FitButton");
	}

	[Test, Order(2)]
	[Category(UITestCategories.GraphicsView)]
	public void ImagePaintResizeBleed()
	{
		NavigateAndVerify("BleedButton");
	}

	[Test, Order(3)]
	[Category(UITestCategories.GraphicsView)]
	public void ImagePaintResizeStretch()
	{
		NavigateAndVerify("StretchButton");
	}

	void NavigateAndVerify(string buttonId)
	{
		App.WaitForElement(buttonId);
		App.Click(buttonId);
		App.WaitForElement("Label");
		VerifyScreenshot();
	}
}
#endif