using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Bugzilla57758 : IssuesUITest
	{
		const string ImageId = "TestImageId";

		public Bugzilla57758(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "ObjectDisposedException for Microsoft.Maui.Controls.Platform.Android.FastRenderers.ImageRenderer";

		[Test]
		[Category(UITestCategories.Image)]
		[FailsOnIOS]
		public void RemovingImageWithGestureFromLayoutWithinGestureHandlerDoesNotCrash()
		{
			RunningApp.WaitForElement(ImageId);
			RunningApp.Tap(ImageId);
		}
	}
}