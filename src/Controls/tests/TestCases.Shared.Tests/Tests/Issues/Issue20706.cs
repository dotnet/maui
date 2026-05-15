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
			// Workaround: On Mac Catalyst, Appium reports stepper buttons in reversed order.
			// See https://github.com/appium/appium/issues/22272
#if MACCATALYST
			App.DecreaseStepper("myStepper");
#else
			App.IncreaseStepper("myStepper");
#endif

			// Verify that the value has been increased.
			var step1Value = App.FindElement("entry").GetText();
			Assert.That("2", Is.EqualTo(step1Value));

			// Change the Stepper increment value.
			App.Click("incrementButton");

			// Increase the value.
			// Workaround: On Mac Catalyst, Appium reports stepper buttons in reversed order.
			// See https://github.com/appium/appium/issues/22272
#if MACCATALYST
			App.DecreaseStepper("myStepper");
#else
			App.IncreaseStepper("myStepper");
#endif
			var step2Value = App.FindElement("entry").GetText();
			Assert.That("12", Is.EqualTo(step2Value));

			// Decrease the value.
			// Workaround: On Mac Catalyst, Appium reports stepper buttons in reversed order.
			// See https://github.com/appium/appium/issues/22272
#if MACCATALYST
			App.IncreaseStepper("myStepper");
#else
			App.DecreaseStepper("myStepper");
#endif

			// Verify that the value has decreased.
			var step3Value = App.FindElement("entry").GetText();
			Assert.That("2", Is.EqualTo(step3Value));
		}
	}
}