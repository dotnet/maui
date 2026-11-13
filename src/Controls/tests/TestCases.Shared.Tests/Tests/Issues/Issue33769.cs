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
		App.IncreaseStepper("Issue33769_Stepper");
		var result = App.WaitForElement("Issue33769_StepperStatusLabel").GetText();
		Assert.That(result, Is.EqualTo("Success"));

		App.DecreaseStepper("Issue33769_Stepper");
		result = App.WaitForElement("Issue33769_StepperStatusLabel").GetText();
		Assert.That(result, Is.EqualTo("Success"));
	}
}
