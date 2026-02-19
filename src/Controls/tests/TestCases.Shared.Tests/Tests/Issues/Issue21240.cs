using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue21240 : _IssuesUITest
	{
		public override string Issue => "FlyoutPage IsGestureEnabled not working on Android";

		public Issue21240(TestDevice device) : base(device) { }

		[Test]
		[Category(UITestCategories.FlyoutPage)]
		public void FlyoutPageIsGestureEnabledShouldPreventSwipeGesture()
		{
			// Wait for the detail page to load
			App.WaitForElement("TitleLabel");
			
			// Try to open flyout with swipe gesture (this should NOT work when IsGestureEnabled=false)
			// On Android, swipe from left edge to right
			var titleRect = App.FindElement("TitleLabel").GetRect();
			App.DragCoordinates(10, titleRect.CenterY(), titleRect.Width - 10, titleRect.CenterY());

			// Verify flyout IS visible now
			App.WaitForNoElement("FlyoutLabel", timeout: TimeSpan.FromSeconds(5));
		}
	}
}
