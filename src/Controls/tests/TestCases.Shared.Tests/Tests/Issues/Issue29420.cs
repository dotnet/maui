#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST // CarouselView Fails to Keep Last Item in View on iOS, android and macOS https://github.com/dotnet/maui/issues/18029, https://github.com/dotnet/maui/issues/29415
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue29420 : _IssuesUITest
{
	public override string Issue => "KeepLastInView Not Working as Expected in CarouselView";

	public Issue29420(TestDevice device) : base(device)
	{ }

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewKeepLastInViewOnItemInsert()
	{
		App.WaitForElement("CarouselView");
		App.Tap("InsertButton");
		VerifyScreenshot("CarouselViewKeepLastInViewOnItemInsert");
	}

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewKeepLastInViewOnItemAdd()
	{
		App.WaitForElement("CarouselView");
		App.Tap("AddButton");
		VerifyScreenshot("CarouselViewKeepLastInViewOnItemAdd");
	}
}
#endif