#if ANDROID || IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue28523 : _IssuesUITest
{
	public Issue28523(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Different behavior on iOS and Android when Loop = False";

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void CarouselViewItemShouldScaleProperly()
	{
		App.WaitForElement("Baboon");
		App.SetOrientationLandscape();
		App.WaitForElement("Baboon");
#if ANDROID
		VerifyScreenshot(cropLeft: 125);
#else
		VerifyScreenshot();
#endif
	}

	[TearDown]
	public void TearDown()
	{
		App.SetOrientationPortrait();
	}
}
#endif