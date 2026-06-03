using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33769 : _IssuesUITest
{
	public Issue33769(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Stepper control fails to reach maximum value when increment exceeds remaining threshold";

	[Test]
	[Category(UITestCategories.Stepper)]
	public void ValidateStepperReachesMinMax()
	{
		App.WaitForElement("Issue33769_StepperStatusLabel");

		// Increase to max and verify - retry tap if it didn't register.
		// Workaround: On Mac Catalyst, Appium reports stepper buttons in reversed order.
		// See https://github.com/appium/appium/issues/22272
		App.RetryAssert(() =>
		{
			var currentValue = App.WaitForElement("Issue33769_StepperStatusLabel").GetText();
			if (currentValue != "Success")
			{
#if MACCATALYST
				App.DecreaseStepper("Issue33769_Stepper");
#else
				App.IncreaseStepper("Issue33769_Stepper");
#endif
				currentValue = App.WaitForElement("Issue33769_StepperStatusLabel").GetText();
			}
			Assert.That(currentValue, Is.EqualTo("Success"));
		});

		// Decrease to min and verify - retry tap if it didn't register.
		// Workaround: On Mac Catalyst, Appium reports stepper buttons in reversed order.
		// See https://github.com/appium/appium/issues/22272
		App.RetryAssert(() =>
		{
			var currentValue = App.WaitForElement("Issue33769_StepperStatusLabel").GetText();
			if (currentValue != "Success")
			{
#if MACCATALYST
				App.IncreaseStepper("Issue33769_Stepper");
#else
				App.DecreaseStepper("Issue33769_Stepper");
#endif
				currentValue = App.WaitForElement("Issue33769_StepperStatusLabel").GetText();
			}
			Assert.That(currentValue, Is.EqualTo("Success"));
		});
	}
}
