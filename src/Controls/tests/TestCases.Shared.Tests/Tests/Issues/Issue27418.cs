#if TEST_FAILS_ON_CATALYST
// When maximizing the window to verify the screenshot on Mac, resizing did not occur properly on cv1. Therefore, I have restricted Catalyst for now.
// for more information , see https://github.com/dotnet/maui/issues/26969
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue27418 : _IssuesUITest
	{
		public Issue27418(TestDevice device) : base(device) { }

		public override string Issue => "CarouselView Rendering Issue with PeekAreaInsets on Android Starting from .NET MAUI 9.0.21";

		[Test]
		[Category(UITestCategories.CarouselView)]
		public void CarouselItemsShouldRenderProperly()
		{
			App.WaitForElement("CarouselView");
			VerifyScreenshot();
		}
	}
}
#endif