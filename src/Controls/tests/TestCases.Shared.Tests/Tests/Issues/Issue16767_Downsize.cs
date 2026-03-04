#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_ANDROID //NullReferenceException throws on iOS and mac Issue link - https://github.com/dotnet/maui/issues/19642 and for Android: https://github.com/dotnet/maui/issues/30576
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
#endif