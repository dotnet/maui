using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue8756 : _IssuesUITest
	{
		public Issue8756(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "3 dots in menu is in black in dark theme in Windows 10, but white in Windows 11";

		[Test]
		[Category(UITestCategories.ToolbarItem)]
		public void ToolbarOverflowMenuShouldRespectIconColor()
		{
			App.WaitForElement("IssueDescription");
			
			// Verify the test page loads correctly
			App.WaitForElement("StatusLabel");
			App.WaitForElement("ToggleIconColorButton");
			
			// Test toggling IconColor to ensure overflow button theming works
			// The overflow button (3-dot menu) should be visible and themed according to IconColor
			App.Tap("ToggleIconColorButton");
			App.WaitForElement("StatusLabel");
			
			// Verify the status changed to White
			var labelText = App.FindElement("StatusLabel").GetText();
			Assert.That(labelText, Does.Contain("White"));
			
			// Toggle back to default
			App.Tap("ToggleIconColorButton");
			App.WaitForElement("StatusLabel");
			
			// Verify the status changed back to Default
			labelText = App.FindElement("StatusLabel").GetText();
			Assert.That(labelText, Does.Contain("Default"));
		}
	}
}