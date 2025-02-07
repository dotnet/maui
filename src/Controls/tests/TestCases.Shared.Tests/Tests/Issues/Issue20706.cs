#if TEST_FAILS_ON_CATALYST // Stepper interaction is not implemented for catalyst
using NUnit.Framework;
using NUnit.Framework.Legacy;
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

		[Test]
		[Category(UITestCategories.Stepper)]
		public void ChangeIncrementValue()
		{
			App.WaitForElement("entry");
			// check the current value.
			var initialValue = App.FindElement("entry").GetText();
			Assert.That("0", Is.EqualTo(initialValue));

			// Increase the value.
			App.IncreaseStepper("myStepper");

			// Verify that the value has been increased.
			var step1Value = App.FindElement("entry").GetText();
			Assert.That("2", Is.EqualTo(step1Value));

			// Change the Stepper increment value.
			App.Click("incrementButton");

			// Increase the value.
			App.IncreaseStepper("myStepper");
			var step2Value = App.FindElement("entry").GetText();
			Assert.That("12", Is.EqualTo(step2Value));

			// Decrease the value.
			App.DecreaseStepper("myStepper");

			// Verify that the value has decreased.
			var step3Value = App.FindElement("entry").GetText();
			Assert.That("2", Is.EqualTo(step3Value));
		}
	}
}
#endif