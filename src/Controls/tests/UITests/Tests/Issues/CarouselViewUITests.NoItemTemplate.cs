using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
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
		public void Issue12777Test()
		{
			App.WaitForElement("TestCarouselView");
			App.Screenshot("Test passed");
		}
	}
}
