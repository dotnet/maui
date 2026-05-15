using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue2597 : _IssuesUITest
{
	public Issue2597(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Stepper control .IsEnabled doesn't work";

	[Test]
	[Category(UITestCategories.Stepper)]
	public void Issue2597Test()
	{
		App.WaitForElement("Stepper");

		// Workaround: On Mac Catalyst, Appium reports stepper buttons in reversed order.
		// See https://github.com/appium/appium/issues/22272
#if MACCATALYST
		App.DecreaseStepper("Stepper");
#else
		App.IncreaseStepper("Stepper");
#endif

		Assert.That(App.FindElement("StepperValue").GetText(), Is.EqualTo("Stepper value is 0"));

		// Workaround: On Mac Catalyst, Appium reports stepper buttons in reversed order.
		// See https://github.com/appium/appium/issues/22272
#if MACCATALYST
		App.IncreaseStepper("Stepper");
#else
		App.DecreaseStepper("Stepper");
#endif

		Assert.That(App.FindElement("StepperValue").GetText(), Is.EqualTo("Stepper value is 0"));
	}
}