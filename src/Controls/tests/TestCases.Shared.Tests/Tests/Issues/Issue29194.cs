using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29194 : _IssuesUITest
{
	public Issue29194(TestDevice device) : base(device) { }

	public override string Issue => "[Android][Label] Setting a Label visible after having had focus on a InputView will increase the Label's height";

	[Test]
	[Category(UITestCategories.Label)]
	public void LabelShouldSizeProperly()
	{
		App.WaitForElement("Entry");
		App.Tap("Entry");
		App.Tap("Switch");
		App.WaitForElement("Label");
		App.DismissKeyboard();
		VerifyScreenshot();
	}
}