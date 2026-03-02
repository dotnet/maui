using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29740 : _IssuesUITest
{
	public Issue29740(TestDevice device) : base(device)
	{
	}

	public override string Issue => "[Windows] Stepper control fails to reach maximum value when increment exceeds remaining threshold";

	[Test]
	[Category(UITestCategories.Stepper)]
	public void GestureRecognizersOnLabelSpanShouldWork()
	{
		var initialvalue = App.WaitForElement("29740StepperValueLabel").GetText();
		Assert.That(initialvalue, Is.EqualTo("Stepper Value: 0"));
		for (int i = 0; i < 4; i++)
		{
			App.IncreaseStepper("29740Stepper");
		}
		var finalvalue = App.WaitForElement("29740StepperValueLabel").GetText();
		Assert.That(finalvalue, Is.EqualTo("Stepper Value: 10"));
	}
}