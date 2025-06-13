using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue2597 : _IssuesUITest
{
	public Issue2597(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Stepper control .IsEnabled doesn't work";

	[Fact]
	[Category(UITestCategories.Stepper)]
	public void Issue2597Test()
	{
		App.WaitForElement("Stepper");

		App.IncreaseStepper("Stepper");

		Assert.Equal("Stepper value is 0", App.FindElement("StepperValue").GetText());

		App.DecreaseStepper("Stepper");

		Assert.Equal("Stepper value is 0", App.FindElement("StepperValue").GetText());
	}
}