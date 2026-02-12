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
#if MACCATALYST
		// On macOS, the XCUITest driver enumerates stepper buttons in reverse order,
		// so IncreaseStepper actually decreases and vice versa. Use DecreaseStepper as a workaround.
		App.DecreaseStepper("Issue33769_Stepper");
#else
		App.IncreaseStepper("Issue33769_Stepper");
#endif
		var result = App.WaitForElement("Issue33769_StepperStatusLabel").GetText();
		Assert.That(result, Is.EqualTo("Success"));
	}
}