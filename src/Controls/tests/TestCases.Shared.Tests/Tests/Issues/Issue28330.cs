using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue28330 : _IssuesUITest
{
	public Issue28330(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Stepper allows to increment when value equals to maximum";

	[Fact]
	[Category(UITestCategories.Stepper)]
	public void Issue28330StepperIncrementShouldBeDisabled()
	{
		App.WaitForElement("Incrementlabel");
		App.IncreaseStepper("Incrementstepper");
		Assert.Equal("1", App.FindElement("Incrementlabel").GetText());
		App.DecreaseStepper("Decrementstepper");
		Assert.Equal("1", App.FindElement("Decrementlabel").GetText());
	}
}