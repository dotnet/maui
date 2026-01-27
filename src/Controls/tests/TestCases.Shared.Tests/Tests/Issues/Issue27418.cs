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
#if MACCATALYST
			// Wait for scene-based window layout to complete
			// Multi-window apps need extra time for the layout to stabilize
			// The CarouselView layout requires multiple passes to settle
			Thread.Sleep(3000);
#endif
#if WINDOWS
            Thread.Sleep(2000); // Wait for scrollbar to disappear to avoid flaky test failures in CI
#endif
			VerifyScreenshot();
		}
	}
}