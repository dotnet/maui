#if TEST_FAILS_ON_WINDOWS //The BoxView's AutomationId doesn't work correctly on the Windows platform, 
// and inserting a Label inside the BoxView is not possible because we need to retrieve the BoxView's rect.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue8004 : _IssuesUITest
	{
		const string AnimateBoxViewButton = "AnimateBoxViewButton";
		const string BoxToScale = "BoxToScale";

		public Issue8004(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Add a ScaleXTo and ScaleYTo animation extension method";

		[Test]
        [Category(UITestCategories.Animation)]
        public void AnimateScaleOfBoxView()
        {
            App.WaitForElement("TestReady");
            var rect = App.WaitForElement(BoxToScale).GetRect();
 
            App.WaitForElement(AnimateBoxViewButton);
        
            // Tap the button.
            App.Tap(AnimateBoxViewButton);
 
            // Wait for animation to finish.
            Thread.Sleep(500);
 
            var scaledRect = App.WaitForElement(BoxToScale).GetRect();
 
            Assert.That(scaledRect.Width, Is.GreaterThan(rect.Width));
            Assert.That(scaledRect.Height, Is.GreaterThan(rect.Height));
        }
	}
}
#endif