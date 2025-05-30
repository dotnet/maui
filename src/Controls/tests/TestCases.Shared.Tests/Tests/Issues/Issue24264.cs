using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue24264 : _IssuesUITest
{
	public override string Issue => "RadioButton should not leak memory";

	public Issue24264(TestDevice device)
	: base(device)
	{ }

	[Test]
	[Category(UITestCategories.RadioButton)]
	public void RadioButtonShouldNotLeakMemory()
	{
		App.WaitForElement("pushbutton");
		App.Tap("pushbutton");
		App.WaitForElement("pushbutton");
		App.Tap("pushbutton");
		App.WaitForElement("resultButton");
		App.Tap("resultButton");
		App.WaitForElement("successLabel");
	}
}

