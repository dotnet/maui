using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class RadioButtonTemplateFromStyle : _IssuesUITest
{
	public RadioButtonTemplateFromStyle(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "RadioButton: Template From Style";

	[Test]
	[Category(UITestCategories.RadioButton)]
	public void ContentRenderers()
	{
		App.WaitForElement("A");
		App.WaitForElement("B");
		App.WaitForElement("C");
	}
}