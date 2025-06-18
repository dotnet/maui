using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class LabelTextType : _IssuesUITest
{
	public LabelTextType(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Implementation of Label TextType";

	[Test]
	[Category(UITestCategories.Label)]
	public void LabelToggleHtmlAndPlainTextTest()
	{
		App.WaitForElement("TextTypeLabel");
		App.Screenshot("I see plain text");

		Assert.That(App.WaitForElement("TextTypeLabel").GetText(), Is.EqualTo("<h1>Hello World!</h1>"));

		App.Tap("ToggleTextTypeButton");

		Assert.That(App.WaitForElement("TextTypeLabel").GetText()?.Contains("<h1>", StringComparison.OrdinalIgnoreCase), Is.False);
	}
}