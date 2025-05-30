#if TEST_FAILS_ON_WINDOWS // Related issue for windows: https://github.com/dotnet/maui/issues/29445
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue29372 : _IssuesUITest
{
	public override string Issue => "CarouselView ItemsLayout Not Updating at Runtime";

	public Issue29372(TestDevice device)
	: base(device)
	{ }

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselLayoutOrientationChange()
	{
		App.WaitForElement("carouselview");
		App.Tap("ChangeItemsLayoutButton");
		VerifyScreenshot();
	}
}
#endif