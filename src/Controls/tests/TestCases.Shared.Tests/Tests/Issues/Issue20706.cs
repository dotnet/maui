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

			// Increase the value and verify - retry tap if it didn't register.
			// Workaround: On Mac Catalyst, Appium reports stepper buttons in reversed order.
			// See https://github.com/appium/appium/issues/22272
			App.RetryAssert(() =>
			{
				var currentValue = App.FindElement("entry").GetText();
				if (currentValue != "2")
				{
#if MACCATALYST
					App.DecreaseStepper("myStepper");
#else
					App.IncreaseStepper("myStepper");
#endif
					currentValue = App.FindElement("entry").GetText();
				}
				Assert.That("2", Is.EqualTo(currentValue));
			});

			// Change the Stepper increment value.
			App.Click("incrementButton");

			// Increase the value and verify - retry tap if it didn't register.
			// Workaround: On Mac Catalyst, Appium reports stepper buttons in reversed order.
			// See https://github.com/appium/appium/issues/22272
			App.RetryAssert(() =>
			{
				var currentValue = App.FindElement("entry").GetText();
				if (currentValue != "12")
				{
#if MACCATALYST
					App.DecreaseStepper("myStepper");
#else
					App.IncreaseStepper("myStepper");
#endif
					currentValue = App.FindElement("entry").GetText();
				}
				Assert.That("12", Is.EqualTo(currentValue));
			});

			// Decrease the value and verify - retry tap if it didn't register.
			// Workaround: On Mac Catalyst, Appium reports stepper buttons in reversed order.
			// See https://github.com/appium/appium/issues/22272
			App.RetryAssert(() =>
			{
				var currentValue = App.FindElement("entry").GetText();
				if (currentValue != "2")
				{
#if MACCATALYST
					App.IncreaseStepper("myStepper");
#else
					App.DecreaseStepper("myStepper");
#endif
					currentValue = App.FindElement("entry").GetText();
				}
				Assert.That("2", Is.EqualTo(currentValue));
			});
		}
	}
}