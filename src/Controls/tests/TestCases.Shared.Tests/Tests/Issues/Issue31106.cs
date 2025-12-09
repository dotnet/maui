using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue31106 : _IssuesUITest
	{
		public override string Issue => "[MacCatalyst] Picker dialog closes automatically with VoiceOver/Keyboard";

		public Issue31106(TestDevice device) : base(device) { }

		[Test]
		[Category(UITestCategories.Picker)]
		public void PickerShouldStayOpen()
		{
			App.WaitForElement("PickerStateLabel");
			var initialState = App.FindElement("PickerStateLabel").GetText();
			Assert.That(initialState, Does.Contain("Closed"));
			App.Tap("TestPicker");
			var openState = App.FindElement("PickerStateLabel").GetText();
			Assert.That(openState, Does.Contain("Open"));
		}
	}
}
