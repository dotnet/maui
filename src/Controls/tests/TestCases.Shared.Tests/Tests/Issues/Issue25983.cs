using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue25983 : _IssuesUITest
	{
		public Issue25983(TestDevice device) : base(device) { }

		public override string Issue => "Issue25983 Grid not getting invalidated when changing the Height/Width of Row/ColumnDefinitions declared with the short syntax";

		[Test]
		[Category(UITestCategories.Layout)]
		public void GridRowDefinitionResizeWithShortSyntax()
		{
			// Wait for page to load and get initial blue label rect
			App.WaitForElement("resizeColumnResultLabel");
			var initialRect = App.WaitForElement("resizeColumnResultLabel").GetRect();
			var initialHeight = initialRect.Height;

			// Click resize row button - increases row height by 100
			App.Tap("resizeRowButton");

			// Wait for layout to update
			Task.Delay(500).Wait();

			// Get new rect - blue label should be taller if grid invalidated correctly
			var newRect = App.WaitForElement("resizeColumnResultLabel").GetRect();
			var newHeight = newRect.Height;

			// Verify the blue label got taller (row definition change took effect)
			Assert.That(newHeight, Is.GreaterThan(initialHeight), 
				$"Expected blue label height to increase after row resize. Initial: {initialHeight}, New: {newHeight}");
		}

		[Test]
		[Category(UITestCategories.Layout)]
		public void GridColumnDefinitionResizeWithShortSyntax()
		{
			// Wait for page to load and get initial blue label rect
			App.WaitForElement("resizeColumnResultLabel");
			var initialRect = App.WaitForElement("resizeColumnResultLabel").GetRect();
			var initialWidth = initialRect.Width;

			// Click resize column button - increases column width by 100
			App.Tap("resizeColumnButton");

			// Wait for layout to update
			Task.Delay(500).Wait();

			// Get new rect - blue label should be wider if grid invalidated correctly
			var newRect = App.WaitForElement("resizeColumnResultLabel").GetRect();
			var newWidth = newRect.Width;

			// Verify the blue label got wider (column definition change took effect)
			Assert.That(newWidth, Is.GreaterThan(initialWidth), 
				$"Expected blue label width to increase after column resize. Initial: {initialWidth}, New: {newWidth}");
		}
	}
}
