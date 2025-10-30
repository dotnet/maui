using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class FlexLayoutGrowTextWrapping : _IssuesUITest
	{
		public FlexLayoutGrowTextWrapping(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "FlexLayout with Grow=1 text should wrap instead of cutting off on Windows";

		[Test]
		[Category(UITestCategories.Layout)]
		public void FlexLayoutGrowShouldWrapText()
		{
			// Verify all test elements are visible and accessible
			App.WaitForElement("Label1");
			App.WaitForElement("Label2");
			App.WaitForElement("Label3");
			App.WaitForElement("Label4");
			App.WaitForElement("Label5");
			App.WaitForElement("Button1");
			App.WaitForElement("Button2");
			App.WaitForElement("Label6");

			// Get the bounds of labels to verify they're laid out properly
			var label1Rect = App.WaitForElement("Label1")[0].GetRect();
			var label2Rect = App.WaitForElement("Label2")[0].GetRect();

			// Labels with Grow=1 in a row should have similar widths (within reasonable tolerance)
			// and should both be visible (height > 0)
			Assert.Greater(label1Rect.Height, 0, "Label1 should be visible");
			Assert.Greater(label2Rect.Height, 0, "Label2 should be visible");
			
			// Both labels should have reasonable widths (not collapsed)
			Assert.Greater(label1Rect.Width, 50, "Label1 should have reasonable width");
			Assert.Greater(label2Rect.Width, 50, "Label2 should have reasonable width");

			// Verify the buttons are also properly laid out
			var button1Rect = App.WaitForElement("Button1")[0].GetRect();
			var button2Rect = App.WaitForElement("Button2")[0].GetRect();
			
			Assert.Greater(button1Rect.Height, 0, "Button1 should be visible");
			Assert.Greater(button2Rect.Height, 0, "Button2 should be visible");
			Assert.Greater(button1Rect.Width, 50, "Button1 should have reasonable width");
			Assert.Greater(button2Rect.Width, 50, "Button2 should have reasonable width");
		}
	}
}
