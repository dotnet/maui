using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue26908 : _IssuesUITest
	{
		public Issue26908(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Time Picker Focus Unfocus is not working";

		[Test]
		[Category(UITestCategories.TimePicker)]
		public void FocusAndUnfocusEventsShouldWork()
		{
			App.WaitForElement("FocusButton");
			App.Click("FocusButton");

			string text = App.FindElement("StatusLabel").GetText()!;
			Assert.That(text, Is.EqualTo("FocusedUnfocused"));
		}
	}
}