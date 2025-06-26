using Microsoft.Maui.Graphics;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
	public class DragAndDropUITests : CoreGalleryBasePageTest
	{
		const string DragAndDropGallery = "Drag and Drop Gallery";
		public DragAndDropUITests(TestDevice device)
			: base(device)
		{
		}

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(DragAndDropGallery);
		}

		// https://github.com/dotnet/maui/issues/24914
#if !MACCATALYST
		[Test]
		[Category(UITestCategories.Gestures)]
		public void DragEvents()
		{
			App.WaitForElement("TargetView");
			App.EnterText("TargetView", "DragAndDropEvents");
			App.Tap("GoButton");

			App.WaitForElement("LabelDragElement");
			App.DragAndDrop("LabelDragElement", "DragTarget");

			App.WaitForElement("DragStartEventsLabel");
			var textAfterDragStart = App.FindElement("DragStartEventsLabel").GetText();

			if (string.IsNullOrEmpty(textAfterDragStart))
			{
				Assert.Fail("Text was expected: Drag start event");
			}
			else
			{
				Assert.That(textAfterDragStart, Is.EqualTo("DragStarting"));
			}

			App.WaitForElement("DragOverEventsLabel");
			var textAfterDragOver = App.FindElement("DragOverEventsLabel").GetText();
			if (string.IsNullOrEmpty(textAfterDragOver))
			{
				Assert.Fail("Text was expected: Drag over event");
			}
			else
			{
				Assert.That(textAfterDragOver, Is.EqualTo("DragOver"));
			}

			App.WaitForElement("DragCompletedEventsLabel");
			var textAfterDragComplete = App.FindElement("DragCompletedEventsLabel").GetText();
			if (string.IsNullOrEmpty(textAfterDragComplete))
			{
				Assert.Fail("Text was expected: Drag complete event");
			}
			else
			{
				Assert.That(textAfterDragComplete, Is.EqualTo("DropCompleted"));
			}

			App.WaitForElement("DropEventsLabel");
			var textAfterDrop = App.FindElement("DropEventsLabel").GetText();
			if (string.IsNullOrEmpty(textAfterDrop))
			{
				Assert.Fail("Text was expected: Drop event");
			}
			else
			{
				Assert.That(textAfterDrop, Is.EqualTo("Drop"));
			}
		}

		[Test]
		[Category(UITestCategories.Gestures)]
		public void DragAndDropBetweenLayouts()
		{
			App.WaitForElement("TargetView");
			App.EnterText("TargetView", "DragAndDropBetweenLayouts");
			App.Tap("GoButton");

			App.WaitForElement("ResetButton");
			App.Tap("ResetButton");

			App.WaitForElement("Red");
			App.WaitForElement("Green");
			App.DragAndDrop("Red", "Green");

			App.WaitForElement("DragStartEventsLabel");
			var textAfterDragStart = App.FindElement("DragStartEventsLabel").GetText();

			if (string.IsNullOrEmpty(textAfterDragStart))
			{
				Assert.Fail("Text was expected: Drag start event");
			}
			else
			{
				Assert.That(textAfterDragStart, Is.EqualTo("DragStarting"));
			}

			App.WaitForElement("DragOverEventsLabel");
			var textAfterDragOver = App.FindElement("DragOverEventsLabel").GetText();
			if (string.IsNullOrEmpty(textAfterDragOver))
			{
				Assert.Fail("Text was expected: Drag over event");
			}
			else
			{
				Assert.That(textAfterDragOver, Is.EqualTo("DragOver"));
			}

			App.WaitForElement("DragCompletedEventsLabel");
			var textAfterDragComplete = App.FindElement("DragCompletedEventsLabel").GetText();
			if (string.IsNullOrEmpty(textAfterDragComplete))
			{
				Assert.Fail("Text was expected: Drag complete event");
			}
			else
			{
				Assert.That(textAfterDragComplete, Is.EqualTo("DropCompleted"));
			}

			App.WaitForElement("RainBowColorsLabel");
			var rainbowColorText = App.FindElement("RainBowColorsLabel").GetText();
			if (string.IsNullOrEmpty(rainbowColorText))
			{
				Assert.Fail("Text was expected");
			}
			else
			{
				Assert.That(rainbowColorText, Is.EqualTo("RainbowColorsAdd:Red"));
			}

			App.WaitForElement("DropEventsLabel");
			var textAfterDrop = App.FindElement("DropEventsLabel").GetText();
			if (string.IsNullOrEmpty(textAfterDrop))
			{
				Assert.Fail("Text was expected: Drop event");
			}
			else
			{
				Assert.That(textAfterDrop, Is.EqualTo("Drop"));
			}
		}

		[Test]
		[Category(UITestCategories.Gestures)]
		public void PlatformDragEventArgs()
		{
			App.WaitForElement("TargetView");
			App.EnterText("TargetView", "DragAndDropEventArgs");
			App.Tap("GoButton");

			App.WaitForElement("LabelDragElement");
			App.DragAndDrop("LabelDragElement", "DragTarget");

			// Add an additional drag and drop to avoid the flakiness in CI.
			App.DragAndDrop("LabelDragElement", "DragTarget");

			App.WaitForElement("DragStartEventsLabel");
			var textAfterDragStart = App.FindElement("DragStartEventsLabel").GetText();

			if (string.IsNullOrEmpty(textAfterDragStart))
			{
				Assert.Fail("Text was expected: Drag start event");
			}
			else
			{
				if (Device == TestDevice.iOS || Device == TestDevice.Mac)
				{
					Assert.That(textAfterDragStart.Contains("DragStarting:Sender", StringComparison.OrdinalIgnoreCase));
					Assert.That(textAfterDragStart.Contains("DragStarting:DragInteraction", StringComparison.OrdinalIgnoreCase));
					Assert.That(textAfterDragStart.Contains("DragStarting:DragSession", StringComparison.OrdinalIgnoreCase));
				}
				else if (Device == TestDevice.Android)
				{
					Assert.That(textAfterDragStart.Contains("DragStarting:Sender", StringComparison.OrdinalIgnoreCase));
					Assert.That(textAfterDragStart.Contains("DragStarting:MotionEvent", StringComparison.OrdinalIgnoreCase));
				}
				else
				{
					Assert.That(textAfterDragStart.Contains("DragStarting:Sender", StringComparison.OrdinalIgnoreCase));
					Assert.That(textAfterDragStart.Contains("DragStarting:DragStartingEventArgs", StringComparison.OrdinalIgnoreCase));
					Assert.That(textAfterDragStart.Contains("DragStarting:Handled", StringComparison.OrdinalIgnoreCase));
				}
			}

			App.WaitForElement("DragOverEventsLabel");
			var textAfterDragOver = App.FindElement("DragOverEventsLabel").GetText();
			if (string.IsNullOrEmpty(textAfterDragOver))
			{
				Assert.Fail("Text was expected: Drag over event");
			}
			else
			{
				if (Device == TestDevice.iOS || Device == TestDevice.Mac)
				{
					Assert.That(textAfterDragOver.Contains("DragOver:Sender", StringComparison.OrdinalIgnoreCase));
					Assert.That(textAfterDragOver.Contains("DragOver:DropInteraction", StringComparison.OrdinalIgnoreCase));
					Assert.That(textAfterDragOver.Contains("DragOver:DropSession", StringComparison.OrdinalIgnoreCase));
				}
				else if (Device == TestDevice.Android)
				{
					Assert.That(textAfterDragOver.Contains("DragOver:Sender", StringComparison.OrdinalIgnoreCase));
					Assert.That(textAfterDragOver.Contains("DragOver:DragEvent", StringComparison.OrdinalIgnoreCase));
				}
				else
				{
					Assert.That(textAfterDragOver.Contains("DragOver:Sender", StringComparison.OrdinalIgnoreCase));
					Assert.That(textAfterDragOver.Contains("DragOver:DragEventArgs", StringComparison.OrdinalIgnoreCase));
				}
			}

			App.WaitForElement("DropCompletedEventsLabel");
			var textAfterDropComplete = App.FindElement("DropCompletedEventsLabel").GetText();
			if (string.IsNullOrEmpty(textAfterDropComplete))
			{
				Assert.Fail("Text was expected: Drop complete event");
			}
			else
			{
				if (Device == TestDevice.iOS || Device == TestDevice.Mac)
				{
					Assert.That(textAfterDropComplete.Contains("DropCompleted:Sender", StringComparison.OrdinalIgnoreCase));
					Assert.That(textAfterDropComplete.Contains("DropCompleted:DropInteraction", StringComparison.OrdinalIgnoreCase));
					Assert.That(textAfterDropComplete.Contains("DropCompleted:DropSession", StringComparison.OrdinalIgnoreCase));
				}
				else if (Device == TestDevice.Android)
				{
					Assert.That(textAfterDropComplete.Contains("DropCompleted:Sender", StringComparison.OrdinalIgnoreCase));
					Assert.That(textAfterDropComplete.Contains("DropCompleted:DragEvent", StringComparison.OrdinalIgnoreCase));
				}
				else
				{
					Assert.That(textAfterDropComplete.Contains("DropCompleted:Sender", StringComparison.OrdinalIgnoreCase));
					Assert.That(textAfterDropComplete.Contains("DropCompleted:DropCompletedEventArgs", StringComparison.OrdinalIgnoreCase));
				}
			}

			App.WaitForElement("DropEventsLabel");

			var textAfterDrop = App.FindElement("DropEventsLabel").GetText();

			if (string.IsNullOrEmpty(textAfterDrop))
			{
				Assert.Fail("Text was expected: drop event");
			}
			else
			{
				if (Device == TestDevice.iOS || Device == TestDevice.Mac)
				{
					Assert.That(textAfterDrop.Contains("Drop:Sender", StringComparison.OrdinalIgnoreCase));
					Assert.That(textAfterDrop.Contains("Drop:DropInteraction", StringComparison.OrdinalIgnoreCase));
					Assert.That(textAfterDrop.Contains("Drop:DropSession", StringComparison.OrdinalIgnoreCase));
				}
				else if (Device == TestDevice.Android)
				{
					Assert.That(textAfterDrop.Contains("Drop:Sender", StringComparison.OrdinalIgnoreCase));
					Assert.That(textAfterDrop.Contains("Drop:DragEvent", StringComparison.OrdinalIgnoreCase));

				}
				else
				{
					Assert.That(textAfterDrop.Contains("Drop:Sender", StringComparison.OrdinalIgnoreCase));
					Assert.That(textAfterDrop.Contains("Drop:DragEventArgs", StringComparison.OrdinalIgnoreCase));
				}
			}
		}

		[Test]
		[Category(UITestCategories.Gestures)]
		public void DragStartEventCoordinates()
		{
			App.WaitForElement("TargetView");
			App.EnterText("TargetView", "DragAndDropBetweenLayouts");
			App.Tap("GoButton");

			App.Tap("ResetButton");

			App.WaitForElement("Blue");
			App.WaitForElement("Green");
			App.DragAndDrop("Blue", "Green");

			var dragStartRelativeToSelf = GetCoordinatesFromLabel(App.FindElement("DragStartRelativeSelf").GetText());
			var dragStartRelativeToScreen = GetCoordinatesFromLabel(App.FindElement("DragStartRelativeScreen").GetText());
			var dragStartRelativeToLabel = GetCoordinatesFromLabel(App.FindElement("DragStartRelativeLabel").GetText());

			Assert.That(dragStartRelativeToSelf, Is.Not.Null);
			Assert.That(dragStartRelativeToScreen, Is.Not.Null);
			Assert.That(dragStartRelativeToLabel, Is.Not.Null);

			Assert.That(dragStartRelativeToSelf!.Value.X > 0 && dragStartRelativeToSelf!.Value.Y > 0);
			Assert.That(dragStartRelativeToScreen!.Value.X > 0 && dragStartRelativeToScreen!.Value.Y > 0);

			// The position of the drag relative to itself should be less than that relative to the screen
			// There are other elements in the screen, plus the ContentView of the test has some margin
			Assert.That(dragStartRelativeToSelf!.Value.X < dragStartRelativeToScreen!.Value.X);
			Assert.That(dragStartRelativeToSelf!.Value.Y < dragStartRelativeToScreen!.Value.Y);

			// Since the label is below the the box, the Y position of the drag relative to the label should be negative
			Assert.That(dragStartRelativeToLabel!.Value.Y < 0);
		}

		[Test]
		[Category(UITestCategories.Gestures)]
		public void DragEventCoordinates()
		{
			App.WaitForElement("TargetView");
			App.EnterText("TargetView", "DragAndDropBetweenLayouts");
			App.Tap("GoButton");

			App.Tap("ResetButton");

			App.WaitForElement("Blue");
			App.WaitForElement("Green");
			App.DragAndDrop("Blue", "Green");

			var dragRelativeToDrop = GetCoordinatesFromLabel(App.FindElement("DragRelativeDrop").GetText());
			var dragRelativeToScreen = GetCoordinatesFromLabel(App.FindElement("DragRelativeScreen").GetText());
			var dragRelativeToLabel = GetCoordinatesFromLabel(App.FindElement("DragRelativeLabel").GetText());
			var dragStartRelativeToScreen = GetCoordinatesFromLabel(App.FindElement("DragStartRelativeScreen").GetText());

			Assert.That(dragRelativeToDrop, Is.Not.Null);
			Assert.That(dragRelativeToScreen, Is.Not.Null);
			Assert.That(dragRelativeToLabel, Is.Not.Null);
			Assert.That(dragStartRelativeToScreen, Is.Not.Null);

			Assert.That(dragRelativeToDrop!.Value.X > 0 && dragRelativeToDrop!.Value.Y > 0);
			Assert.That(dragRelativeToScreen!.Value.X > 0 && dragRelativeToScreen!.Value.Y > 0);

			// The position of the drag relative to the drop location should be less than that relative to the screen
			// There are other elements in the screen, plus the ContentView of the test has some margin
			Assert.That(dragRelativeToDrop!.Value.X < dragRelativeToScreen!.Value.X);
			Assert.That(dragRelativeToDrop!.Value.Y < dragRelativeToScreen!.Value.Y);

			// Since the label is below the the box, the Y position of the drag relative to the label should be negative
			Assert.That(dragRelativeToLabel!.Value.Y < 0);

			// The drag is executed left to right, so the X value should be higher than where it started
			Assert.That(dragRelativeToScreen!.Value.X > dragStartRelativeToScreen!.Value.X);
		}

#if TEST_FAILS_ON_WINDOWS || TEST_FAILS_ON_MACCATALYST
		// TODO: Flaky test, disabling for Win and Mac.
		[Test]
		[Category(UITestCategories.Gestures)]
		public void DropEventCoordinates()
		{
			App.WaitForElement("TargetView");
			App.EnterText("TargetView", "DragAndDropBetweenLayouts");
			App.Tap("GoButton");

			App.Tap("ResetButton");

			App.WaitForElement("Blue");
			App.WaitForElement("Green");
			App.DragAndDrop("Blue", "Green");

			// Wait for all UI elements to confirm drag and drop operation completion
			App.WaitForElement("DropRelativeLayout");
			App.WaitForElement("DropRelativeScreen");
			App.WaitForElement("DropRelativeLabel");
			App.WaitForElement("DragStartRelativeScreen");

			var dropRelativeToLayout = GetCoordinatesFromLabel(App.FindElement("DropRelativeLayout").GetText());
			var dropRelativeToScreen = GetCoordinatesFromLabel(App.FindElement("DropRelativeScreen").GetText());
			var dropRelativeToLabel = GetCoordinatesFromLabel(App.FindElement("DropRelativeLabel").GetText());

			var dragRelativeToLabel = GetCoordinatesFromLabel(App.FindElement("DragRelativeLabel").GetText());
			var dragStartRelativeToScreen = GetCoordinatesFromLabel(App.FindElement("DragStartRelativeScreen").GetText());

			Assert.That(dropRelativeToLayout, Is.Not.Null);
			Assert.That(dropRelativeToScreen, Is.Not.Null);
			Assert.That(dropRelativeToLabel, Is.Not.Null);

			Assert.That(dragRelativeToLabel, Is.Not.Null);
			Assert.That(dragStartRelativeToScreen, Is.Not.Null);

			Assert.That(dropRelativeToLayout!.Value.X > 0 && dropRelativeToLayout!.Value.Y > 0);
			Assert.That(dropRelativeToScreen!.Value.X > 0 && dropRelativeToScreen!.Value.Y > 0);

			// The position of the drop relative the layout should be less than that relative to the screen
			// There are other elements in the screen, plus the ContentView of the test has some margin
			Assert.That(dropRelativeToLayout!.Value.X < dropRelativeToScreen!.Value.X);
			Assert.That(dropRelativeToLayout!.Value.Y < dropRelativeToScreen!.Value.Y);

			// Since the label is below the the box, the Y position of the drop relative to the label should be negative
			Assert.That(dropRelativeToLabel!.Value.Y < 0);

			// The drop is executed left to right, so the X value should be higher than where it started
			Assert.That(dropRelativeToScreen!.Value.X > dragStartRelativeToScreen!.Value.X);

			// The label receiving the coordinates of the drop is below that which receives the coordinates of the drag
			// Therefore, the label that receives the coordinates of the drop should have a smaller Y value (more negative)
			Assert.That(dropRelativeToLabel!.Value.Y < dragRelativeToLabel!.Value.Y);
		}
#endif
#endif

		// Helper function to parse out the X and Y coordinates from text labels 'Drag position: (x),(y)'
		Point? GetCoordinatesFromLabel(string? labelText)
		{
			if (labelText is null)
				return null;

			var i = labelText.IndexOf(':', StringComparison.Ordinal);

			if (i == -1)
				return null;

			var coordinates = labelText[(i + 1)..].Split(",");
			if (coordinates.Length < 2)
				return null;

			var x = int.Parse(coordinates[0]);
			var y = int.Parse(coordinates[1]);

			return new Point(x, y);
		}
	}
}