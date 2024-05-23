using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue12193 : _IssuesUITest
	{
		const string HTML = "HTML";

		public Issue12193(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] CarouselView content disappears after 2 rotations if TextType=Html is used";

		[Test]
		[Category(UITestCategories.CarouselView)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnIOS]
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