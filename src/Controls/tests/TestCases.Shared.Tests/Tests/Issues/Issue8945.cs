using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue8945 : _IssuesUITest
	{
		public Issue8945(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Add Open/Close API to picker controls";

		[Test]
		[Category(UITestCategories.Picker)]
		public void OpenClosePicker()
		{
			App.WaitForElement("WaitForStubControl");
			App.Tap("OpenPickerButton");
			Assert.That(App.WaitForElement("PickerStatusLabel")?.GetText(), Is.EqualTo("Picker Opened"));
			App.Tap("ClosePickerButton");
			Assert.That(App.WaitForElement("PickerStatusLabel")?.GetText(), Is.EqualTo("Picker Closed"));
		}
	}
}