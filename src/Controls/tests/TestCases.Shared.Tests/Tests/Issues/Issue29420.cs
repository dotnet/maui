#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST // CarouselView Fails to Keep Last Item in View on iOS, android and macOS https://github.com/dotnet/maui/issues/18029, https://github.com/dotnet/maui/issues/29415
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue29420 : _IssuesUITest
{
	public override string Issue => "KeepLastInView Not Working as Expected in CarouselView";

	public Issue29420(TestDevice device) : base(device)
	{ }

	[Test, Order(1)]
	[Category(UITestCategories.CarouselView)]
	public async Task VerifyCarouselViewKeepLastInViewOnItemInsert()
	{
		App.WaitForElement("CarouselView");
		App.Tap("InsertButton");
		await Task.Delay(200); // Wait for the scrollbar to disappear.
		VerifyScreenshot("CarouselViewKeepLastInViewOnItemInsert");
	}

	[Test, Order(2)]
	[Category(UITestCategories.CarouselView)]
	public async Task VerifyCarouselViewKeepLastInViewOnItemAdd()
	{
		App.WaitForElement("CarouselView");
		App.Tap("AddButton");
		await Task.Delay(200); // Wait for the scrollbar to disappear.
		VerifyScreenshot("CarouselViewKeepLastInViewOnItemAdd");
	}
}
#endif