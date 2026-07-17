using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33274 : _IssuesUITest
{
	public Issue33274(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Windows Maui Stepper is not clamped to minimum or maximum internally";

	[Test]
	[Category(UITestCategories.Stepper)]
	public void Issue33274CheckInAndDeCrementation()
	{
		App.WaitForElement("Maximumlabel");

		// We are already at maximum and the increment schould not increase the internal value
		App.IncreaseStepper("Maximumstepper");
		App.DecreaseStepper("Maximumstepper");
		Assert.That(App.FindElement("Maximumlabel").GetText(), Is.EqualTo("0"));

		// We are already at minimum and the decrement schould not decrease the internal value
		App.DecreaseStepper("Minimumstepper");
		App.IncreaseStepper("Minimumstepper");
		Assert.That(App.FindElement("Minimumlabel").GetText(), Is.EqualTo("1"));
	}
}