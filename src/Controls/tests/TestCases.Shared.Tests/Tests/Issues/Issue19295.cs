#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue19295 : _IssuesUITest
	{
		const string PickerAutomationId = "picker";
		const string ShortItem = "Short";
		const string LongItem = "Japanese Macaque";

		public Issue19295(TestDevice device) : base(device) { }

		public override string Issue => "Picker does Not Resize Automatically After Selection";

		[Test]
		[Category(UITestCategories.Picker)]
		public void PickerShouldResizeAfterChangingSelection()
		{
			// Arrange: Wait for picker and get initial width with short item selected
			App.WaitForElement(PickerAutomationId);
			var initialWidth = App.FindElement(PickerAutomationId).GetRect().Width;

			// Act: Open picker and select the longer item
			App.Tap(PickerAutomationId);
			App.SelectPickerWheelValue(LongItem);
			App.WaitForElement("Done");
			App.Tap("Done");

			// Assert: Picker width should increase to accommodate longer text
			var finalWidth = App.FindElement(PickerAutomationId).GetRect().Width;
			Assert.That(finalWidth, Is.GreaterThan(initialWidth), 
				$"Picker width should increase when selecting longer item. Initial: {initialWidth}, Final: {finalWidth}");
		}
	}
}
#endif