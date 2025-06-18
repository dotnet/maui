using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	internal class Issue25792 : _IssuesUITest
	{
		public Issue25792(TestDevice device) : base(device) { }

		public override string Issue => "Picker ItemsSource Change Triggers Exception: 'Value Does Not Fall Within the Expected Range";

		[Test]
		[Category(UITestCategories.Picker)]
		public void PickerShouldNotCrashWhenSelectedIndexExceedsItemsSourceCount()
		{
			App.WaitForElement("Picker");
			App.Tap("ChangeDataButton");
			App.WaitForElement("Picker");
			var selectedItemText = App.WaitForElement("Picker").GetText();
			Assert.That(selectedItemText, Is.EqualTo("New Item 2"));
		}
	}
}
