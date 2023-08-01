using Maui.Controls.Sample;
using Microsoft.Maui.Appium;
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
		public void DragAndDropCoordinates()
		{
			App.WaitForElement("TargetView");
			App.EnterText("TargetView", "DragAndDropBetweenLayouts");
			App.Tap("GoButton");

			App.WaitForElement("Blue");
			App.DragAndDrop("Blue", "Green");
			App.WaitForElement("DragEventsLabel");

			var textStartDrag = App.Query("StartDragEventLabel").First().Text;
			var textEndDrag = App.Query("EndDragEventLabel").First().Text;

			Point startPoint = new Point(float.Parse(textStartDrag.Split(",")[0]), float.Parse(textStartDrag.Split(",")[1]));
			Point endPoint = new Point(float.Parse(textEndDrag.Split(",")[0]), float.Parse(textEndDrag.Split(",")[1]));

			Assert.True(startPoint.X != 0 && startPoint.Y != 0);
			Assert.True(endPoint.X != 0 && endPoint.Y != 0);
			Assert.True(endPoint.Y < startPoint.Y); // Blue is below Green as of writing this test
		}
	}
}