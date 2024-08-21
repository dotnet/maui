using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue8262 : _IssuesUITest
	{
		public Issue8262(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Android] ImageRenderer still being accessed after control destroyed";

		[Test]
		[Category(UITestCategories.Image)]
		[Category(UITestCategories.LifeCycle)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnAllPlatforms]
		public void ScrollingQuicklyOnCollectionViewDoesntCrashOnDestroyedImage()
		{
			App.WaitForElement("ScrollMe");
			App.ScrollDown("ScrollMe", ScrollStrategy.Gesture, swipeSpeed: 20000);
			App.ScrollUp("ScrollMe", ScrollStrategy.Gesture, swipeSpeed: 20000);
			App.ScrollDown("ScrollMe", ScrollStrategy.Gesture, swipeSpeed: 20000);
			App.ScrollUp("ScrollMe", ScrollStrategy.Gesture, swipeSpeed: 20000);
			App.ScrollDown("ScrollMe", ScrollStrategy.Gesture, swipeSpeed: 20000);
			App.WaitForElement("ScrollMe");
		}
	}
}
