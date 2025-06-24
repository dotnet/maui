using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue28902 : _IssuesUITest
	{
		public Issue28902(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Label disappears when android:hardwareAccelerated=\"false\"";

		[Test]
		[Category(UITestCategories.Label)]
		[Category(UITestCategories.Compatibility)]
		public void LabelShouldBeVisibleWhenHardwareAccelerationDisabled()
		{
			App.WaitForElement("TestLabel");
			
			// Verify the label is visible and has the expected text
			var label = App.FindElement("TestLabel");
			Assert.That(label, Is.Not.Null, "Label should be found by automation ID");
			Assert.That(label.GetText(), Is.EqualTo("Test"), "Label should display the correct text");
		}
	}
}