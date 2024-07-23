using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class CarouselViewSetOrientation : _IssuesUITest
	{
		public const string HTML = "HTML";

		public CarouselViewSetOrientation(TestDevice device)
			: base(device)
		{
		}

		public override string Issue => "[Bug] CarouselView content disappears after 2 rotations if TextType=Html is used";

		// Issue12193 (src\ControlGallery\src\Issues.Shared\Issue12193.cs
		[Test]
		[Category(UITestCategories.CarouselView)]
		[FailsOnMac("Set Orientation methods not implemented")]
		[FailsOnWindows("Set Orientation methods not implemented")]
		public async Task RotatingCarouselViewHTMLShouldNotDisappear()
		{
			int delay = 3000;

			App.SetOrientationPortrait();
			await Task.Delay(delay);

			App.SetOrientationLandscape();
			await Task.Delay(delay);

			App.SetOrientationPortrait();
			await Task.Delay(delay);

			App.SetOrientationLandscape();
			await Task.Delay(delay);

			App.SetOrientationPortrait();
			await Task.Delay(delay);

			App.WaitForElement(HTML);
		}
	}
}