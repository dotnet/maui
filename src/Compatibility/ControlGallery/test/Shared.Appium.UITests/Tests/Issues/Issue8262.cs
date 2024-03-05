using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue8262 : IssuesUITest
	{
		public Issue8262(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Android] ImageRenderer still being accessed after control destroyed";

		[Test]
		[Category(UITestCategories.Image)]
		public void ScrollingQuicklyOnCollectionViewDoesntCrashOnDestroyedImage()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForElement("ScrollMe");
			App.ScrollDown("ScrollMe", ScrollStrategy.Gesture, swipeSpeed: 20000);
			App.ScrollUp("ScrollMe", ScrollStrategy.Gesture, swipeSpeed: 20000);
			App.ScrollDown("ScrollMe", ScrollStrategy.Gesture, swipeSpeed: 20000);
			App.ScrollUp("ScrollMe", ScrollStrategy.Gesture, swipeSpeed: 20000);
			App.ScrollDown("ScrollMe", ScrollStrategy.Gesture, swipeSpeed: 20000);
			RunningApp.WaitForElement("ScrollMe");
		}
	}
}
