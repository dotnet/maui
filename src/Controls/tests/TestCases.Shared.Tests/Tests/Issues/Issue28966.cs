using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue28966 : _IssuesUITest
{
	public Issue28966(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "ImageButton's border not rendering correctly on Android";

	[Test]
	[Category(UITestCategories.ImageButton)]
	public void ImageButtonBorderShouldRenderCorrectly()
	{
		App.WaitForElement("ImageButton");
		VerifyScreenshot();
	}
}