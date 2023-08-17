using Maui.Controls.Sample;
using Microsoft.Maui.Appium;
using NUnit.Framework;
using TestUtils.Appium.UITests;
using Xamarin.UITest;

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
		public void PlatformDragEventArgs()
		{
			App.WaitForElement("TargetView");
			App.EnterText("TargetView", "DragAndDropEventArgs");
			App.Tap("GoButton");

			App.WaitForElement("LabelDragElement");
			App.DragAndDrop("LabelDragElement", "DragTarget");
			App.WaitForElement("DragEventsLabel");

			var textAfterDrag = App.Query("DragEventsLabel").First().Text;

#if IOSUITEST || MACUITEST
			Assert.True(textAfterDrag.Contains("DragStarting:Sender", StringComparison.OrdinalIgnoreCase));
			Assert.True(textAfterDrag.Contains("DragStarting:DragInteraction", StringComparison.OrdinalIgnoreCase));
			Assert.True(textAfterDrag.Contains("DragStarting:DragSession", StringComparison.OrdinalIgnoreCase));

			Assert.True(textAfterDrag.Contains("DropCompleted:Sender", StringComparison.OrdinalIgnoreCase));
			Assert.True(textAfterDrag.Contains("DropCompleted:DropInteraction", StringComparison.OrdinalIgnoreCase));
			Assert.True(textAfterDrag.Contains("DropCompleted:DropSession", StringComparison.OrdinalIgnoreCase));

			Assert.True(textAfterDrag.Contains("DragLeave:Sender", StringComparison.OrdinalIgnoreCase));
			Assert.True(textAfterDrag.Contains("DragLeave:DropInteraction", StringComparison.OrdinalIgnoreCase));
			Assert.True(textAfterDrag.Contains("DragLeave:DropSession", StringComparison.OrdinalIgnoreCase));

			Assert.True(textAfterDrag.Contains("DragOver:Sender", StringComparison.OrdinalIgnoreCase));
			Assert.True(textAfterDrag.Contains("DragOver:DropInteraction", StringComparison.OrdinalIgnoreCase));
			Assert.True(textAfterDrag.Contains("DragOver:DropSession", StringComparison.OrdinalIgnoreCase));

			Assert.True(textAfterDrag.Contains("Drop:Sender", StringComparison.OrdinalIgnoreCase));
			Assert.True(textAfterDrag.Contains("Drop:DropInteraction", StringComparison.OrdinalIgnoreCase));
			Assert.True(textAfterDrag.Contains("Drop:DropSession", StringComparison.OrdinalIgnoreCase));
#elif ANDROID
			Assert.True(textAfterDrag.Contains("DragStarting:Sender", StringComparison.OrdinalIgnoreCase));
			Assert.True(textAfterDrag.Contains("DragStarting:MotionEvent", StringComparison.OrdinalIgnoreCase));

			Assert.True(textAfterDrag.Contains("DropCompleted:Sender", StringComparison.OrdinalIgnoreCase));
			Assert.True(textAfterDrag.Contains("DropCompleted:MotionEvent", StringComparison.OrdinalIgnoreCase));

			Assert.True(textAfterDrag.Contains("DragLeave:Sender", StringComparison.OrdinalIgnoreCase));
			Assert.True(textAfterDrag.Contains("DragLeave:MotionEvent", StringComparison.OrdinalIgnoreCase));

			Assert.True(textAfterDrag.Contains("DragOver:Sender", StringComparison.OrdinalIgnoreCase));
			Assert.True(textAfterDrag.Contains("DragOver:MotionEvent", StringComparison.OrdinalIgnoreCase));

			Assert.True(textAfterDrag.Contains("Drop:Sender", StringComparison.OrdinalIgnoreCase));
			Assert.True(textAfterDrag.Contains("Drop:MotionEvent", StringComparison.OrdinalIgnoreCase));
#elif WINTEST
			Assert.True(textAfterDrag.Contains("DragStarting:Sender", StringComparison.OrdinalIgnoreCase));
			Assert.True(textAfterDrag.Contains("DragStarting:DragStartingEventArgs", StringComparison.OrdinalIgnoreCase));

			Assert.True(textAfterDrag.Contains("DropCompleted:Sender", StringComparison.OrdinalIgnoreCase));
			Assert.True(textAfterDrag.Contains("DropCompleted:DropCompletedEventArgs", StringComparison.OrdinalIgnoreCase));

			Assert.True(textAfterDrag.Contains("DragLeave:Sender", StringComparison.OrdinalIgnoreCase));
			Assert.True(textAfterDrag.Contains("DragLeave:DragEventArgs", StringComparison.OrdinalIgnoreCase));

			Assert.True(textAfterDrag.Contains("DragOver:Sender", StringComparison.OrdinalIgnoreCase));
			Assert.True(textAfterDrag.Contains("DragOver:DragEventArgs", StringComparison.OrdinalIgnoreCase));

			Assert.True(textAfterDrag.Contains("Drop:Sender", StringComparison.OrdinalIgnoreCase));
			Assert.True(textAfterDrag.Contains("Drop:DragEventArgs", StringComparison.OrdinalIgnoreCase));
#endif
		}
	}
}