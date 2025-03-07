#if IOS || ANDROID //The test fails on Windows and MacCatalyst because the SetOrientation method, which is intended to change the device orientation, is only supported on mobile platforms iOS and Android.
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
		[FailsOnMacWhenRunningOnXamarinUITest("Set Orientation methods not implemented")]
		[FailsOnWindowsWhenRunningOnXamarinUITest("Set Orientation methods not implemented")]
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
#endif