using Maui.Controls.Sample;
using Microsoft.Maui.Appium;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Graphics;
using NUnit.Framework;
using TestUtils.Appium.UITests;

namespace Microsoft.Maui.AppiumTests
{
	public class DragAndDropUITests : UITestBase
	{
		const string DragAndDropGallery = "* marked:'Drag and Drop Gallery'";
		public DragAndDropUITests(TestDevice device)
			: base(device)
		{
		}

		protected override void FixtureSetup()
		{
			base.FixtureSetup();
			App.NavigateToGallery(DragAndDropGallery);
		}

		protected override void FixtureTeardown()
		{
			base.FixtureTeardown();
			App.NavigateBack();
		}

		[Test]
		public void DragEvents()
		{
			App.WaitForElement("TargetView");
			App.EnterText("TargetView", "DragAndDropEvents");
			App.Tap("GoButton");

			App.WaitForElement("LabelDragElement");
			App.DragAndDrop("LabelDragElement", "DragTarget");
			App.WaitForElement("DragEventsLabel");

			var textAfterDrag = App.Query("DragEventsLabel").First().Text;
			Assert.True(textAfterDrag.Contains("DragStarting", StringComparison.OrdinalIgnoreCase));
			Assert.True(textAfterDrag.Contains("DragOver", StringComparison.OrdinalIgnoreCase));
			Assert.True(textAfterDrag.Contains("DropCompleted", StringComparison.OrdinalIgnoreCase));
		}

		[Test]
		public void DragAndDropBetweenLayouts()
		{
			App.WaitForElement("TargetView");
			App.EnterText("TargetView", "DragAndDropBetweenLayouts");
			App.Tap("GoButton");

			App.WaitForElement("Red");
			App.DragAndDrop("Red", "Green");
			App.WaitForElement("DragEventsLabel");

			var textAfterDrag = App.Query("DragEventsLabel").First().Text;
			Assert.True(textAfterDrag.Contains("DragStarting", StringComparison.OrdinalIgnoreCase));
			Assert.True(textAfterDrag.Contains("DragOver", StringComparison.OrdinalIgnoreCase));
			Assert.True(textAfterDrag.Contains("DropCompleted", StringComparison.OrdinalIgnoreCase));
			Assert.True(textAfterDrag.Contains("RainbowColorsAdd:Red", StringComparison.OrdinalIgnoreCase));
		}

		[Test]
		public void DragStartEventCoordinates()
		{
			App.WaitForElement("TargetView");
			App.EnterText("TargetView", "DragAndDropBetweenLayouts");
			App.Tap("GoButton");

			App.WaitForElement("Blue");
			App.DragAndDrop("Blue", "Green");

			var dragStartRelativeToSelf  = GetCoordinatesFromLabel(App.Query("DragStartRelativeSelf").First().Text);
			var dragStartRelativeToScreen = GetCoordinatesFromLabel(App.Query("DragStartRelativeScreen").First().Text);
			var dragStartRelativeToLabel = GetCoordinatesFromLabel(App.Query("DragStartRelativeLabel").First().Text);

			Assert.NotNull(dragStartRelativeToSelf);
			Assert.NotNull(dragStartRelativeToScreen);
			Assert.NotNull(dragStartRelativeToLabel);

			Assert.True(dragStartRelativeToSelf!.Value.X > 0 && dragStartRelativeToSelf!.Value.Y > 0);
			Assert.True(dragStartRelativeToScreen!.Value.X > 0 && dragStartRelativeToScreen!.Value.Y > 0);

			// The position of the drag relative to itself should be less than that relative to the screen
			// There are other elements in the screen, plus the ContentView of the test has some margin
			Assert.True(dragStartRelativeToSelf!.Value.X < dragStartRelativeToScreen!.Value.X);
			Assert.True(dragStartRelativeToSelf!.Value.Y < dragStartRelativeToScreen!.Value.Y);

			// Since the label is below the the box, the Y position of the drag relative to the label should be negative
			Assert.True(dragStartRelativeToLabel!.Value.Y < 0);
		}

		[Test]
		public void DragEventCoordinates()
		{
			App.WaitForElement("TargetView");
			App.EnterText("TargetView", "DragAndDropBetweenLayouts");
			App.Tap("GoButton");

			App.WaitForElement("Blue");
			App.DragAndDrop("Blue", "Green");

			var dragRelativeToDrop = GetCoordinatesFromLabel(App.Query("DragRelativeDrop").First().Text);
			var dragRelativeToScreen = GetCoordinatesFromLabel(App.Query("DragRelativeScreen").First().Text);
			var dragRelativeToLabel = GetCoordinatesFromLabel(App.Query("DragRelativeLabel").First().Text);
			var dragStartRelativeToScreen = GetCoordinatesFromLabel(App.Query("DragStartRelativeScreen").First().Text);

			Assert.NotNull(dragRelativeToDrop);
			Assert.NotNull(dragRelativeToScreen);
			Assert.NotNull(dragRelativeToLabel);
			Assert.NotNull(dragStartRelativeToScreen);


			Assert.True(dragRelativeToDrop!.Value.X > 0 && dragRelativeToDrop!.Value.Y > 0);
			Assert.True(dragRelativeToScreen!.Value.X > 0 && dragRelativeToScreen!.Value.Y > 0);

			// The position of the drag relative to the drop location should be less than that relative to the screen
			// There are other elements in the screen, plus the ContentView of the test has some margin
			Assert.True(dragRelativeToDrop!.Value.X < dragRelativeToScreen!.Value.X);
			Assert.True(dragRelativeToDrop!.Value.Y < dragRelativeToScreen!.Value.Y);

			// Since the label is below the the box, the Y position of the drag relative to the label should be negative
			Assert.True(dragRelativeToLabel!.Value.Y < 0);

			// The drag is executed left to right, so the X value should be higher than where it started
			Assert.True(dragRelativeToScreen!.Value.X > dragStartRelativeToScreen!.Value.X);
		}

		// Helper function to parse out the X and Y coordinates from text labels 'Drag position: (x),(y)'
		Point? GetCoordinatesFromLabel(string? labelText)
		{
			if (labelText is null) 
				return null;

			var i = labelText.IndexOf(':', StringComparison.Ordinal);

			if (i == -1)
				return null;

			var coordinates = labelText[(i + 1)..].Split(",");
			var x = int.Parse(coordinates[0]);
			var y = int.Parse(coordinates[1]);

			return new Point(x, y);
		}
	}
}