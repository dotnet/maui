using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue28208 : _IssuesUITest
{
	public Issue28208(TestDevice device) : base(device) { }

	public override string Issue => "[Windows] The Slider and Stepper control does not work in One-Way binding mode with a MultiBinding Converter";

	[Test]
	[Category(UITestCategories.Slider)]
	[Category(UITestCategories.Stepper)]
	public void OneWayBindingWithMultiBindingConverterShouldReflecttInView()
	{
		App.WaitForElement("Button");
		App.Tap("Button");
		App.Tap("Button");
		var sliderLabel = App.FindElement("Sliderlabel").GetText();
		Assert.That(sliderLabel, Is.EqualTo("4"));
		var stepperLabel = App.FindElement("StepperLabel").GetText();
		Assert.That(stepperLabel, Is.EqualTo("4"));
	}
}