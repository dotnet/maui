using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue20772 : _IssuesUITest
	{
		public Issue20772(TestDevice device) : base(device) { }

		public override string Issue => "Flickering occurs while updating the width of ContentView through PanGestureRecognizer";

		[Test]
		[Category(UITestCategories.Gestures)]
		public void PanGestureDoesNotFlickerWhenResizingView()
		{
			// Wait for the page to load
			App.WaitForElement("CustomContent");
			
			// Get initial width of the child element
			var childBefore = App.WaitForElement("CustomChild").GetRect();
			var initialWidth = childBefore.Width;

			// Get the element to pan on
			var element = App.WaitForElement("CustomContent").GetRect();
			var startX = element.X + (element.Width / 2);
			var startY = element.Y + (element.Height / 2);

			// Perform a pan gesture that drags to the right by 100 pixels
			// This will trigger the view resize during the pan
			var dragDistance = 100;
			App.DragCoordinates(startX, startY, startX + dragDistance, startY);

			// Wait for the status label to update with the result
			App.WaitForElement("StatusLabel");
			
			// Get final width of the child element
			var childAfter = App.WaitForElement("CustomChild").GetRect();
			var finalWidth = childAfter.Width;
			var actualWidthChange = finalWidth - initialWidth;

			// The test passes if the width change approximately matches the drag distance
			// With the bug (before fix): coordinates jump around, width change is erratic
			// With the fix: width change should be close to drag distance (within tolerance)
			// We allow 30 pixel tolerance for timing variations
			Assert.That(actualWidthChange, Is.EqualTo(dragDistance).Within(30), 
				$"Width change ({actualWidthChange}) should approximately match drag distance ({dragDistance}). " +
				$"Initial: {initialWidth}, Final: {finalWidth}");
		}
	}
}
