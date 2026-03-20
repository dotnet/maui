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
			// Use retryTimeout to wait for scrollbar to disappear on Windows
			VerifyScreenshot(retryTimeout: TimeSpan.FromSeconds(2));
		}
	}
}