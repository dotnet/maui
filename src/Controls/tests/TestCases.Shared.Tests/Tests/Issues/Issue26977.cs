using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue26977 : _IssuesUITest
{
	public Issue26977(TestDevice device) : base(device) { }

	public override string Issue => "Setter.TargetName + ControlTemplate crash";

	[Test]
	[Category(UITestCategories.Page)]
	public void SetterTargetNameWithControlTemplateShouldNotCrash()
	{
		App.WaitForElement("StateSwitch");
		App.Tap("StateSwitch");

		var normalText = App.FindElement("TargetLabel").GetText();
		Assert.That(normalText, Is.EqualTo("Value from Setter in State1"),
			"Label text should change to 'Value from Setter in State1' after the switch is toggled");
	}
}
