using Maui.Controls.Sample;
using Microsoft.Maui.Appium;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Graphics;
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
			// This issue is not working on net7 for the following platforms 
			// This is not a regression it's just the test being backported from net8
			UITestContext.IgnoreIfPlatforms(new[]
			{
				TestDevice.Mac, TestDevice.Windows
			}, BackportedTestMessage);

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
	}
}