using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class CarouselViewNoItemTemplate : _IssuesUITest
	{
		public CarouselViewNoItemTemplate(TestDevice device)
			: base(device)
		{
		}

		public override string Issue => "[Bug] CarouselView NRE if item template is not specified";

		// Issue12777 (src\ControlGallery\src\Issues.Shared\Issue12777.cs
		[Test]
		[Category(UITestCategories.CarouselView)]
		[FailsOnAllPlatformsWhenRunningOnXamarinUITest("This test is failing, likely due to product issue")]
		public void Issue12777Test()
		{
			App.WaitForElement("TestCarouselView");
			App.Screenshot("Test passed");
		}
	}
}
