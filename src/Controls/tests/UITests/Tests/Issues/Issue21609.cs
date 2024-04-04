using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue21609 : _IssuesUITest
	{
		const string TestCarouselView = "TestCarouselView";

		public Issue21609(TestDevice device) : base(device)
		{
		}

		public override string Issue => "After rotating the emulator from horizontal to vertical orientation, the card of page My Recipes is truncated";

		[Test]
		[Category(UITestCategories.CarouselView)]
		public async Task CorrectCarouselViewLayoutDeviceOrientation()
		{
			this.IgnoreIfPlatforms(new[]
			{
				TestDevice.iOS, TestDevice.Mac, TestDevice.Windows
			});

			try
			{
				App.WaitForElement(TestCarouselView);

				App.SetOrientationLandscape();
				await Task.Delay(1000); // Wait to complete the device rotation animation.
				VerifyScreenshot();
			}
			finally
			{
				App.SetOrientationPortrait();
			}
		}
	}
}
