using Xunit;
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue20706 : _IssuesUITest
	{
		public override string Issue => "Stepper doesn't change increment value when being bound to a double in MVVM context (Windows)";
		public Issue20706(TestDevice device) : base(device)
		{
		}

		[Fact]
		[Category(UITestCategories.Stepper)]
		public void ChangeIncrementValue()
		{
			App.WaitForElement("entry");
			// check the current value.
			var initialValue = App.FindElement("entry").GetText();
			Assert.Equal(initialValue, "0");

			// Increase the value.
			App.IncreaseStepper("myStepper");

			// Verify that the value has been increased.
			var step1Value = App.FindElement("entry").GetText();
			Assert.Equal(step1Value, "2");

			// Change the Stepper increment value.
			App.Click("incrementButton");

			// Increase the value.
			App.IncreaseStepper("myStepper");
			var step2Value = App.FindElement("entry").GetText();
			Assert.Equal(step2Value, "12");

			// Decrease the value.
			App.DecreaseStepper("myStepper");

			// Verify that the value has decreased.
			var step3Value = App.FindElement("entry").GetText();
			Assert.Equal(step3Value, "2");
		}
	}
}