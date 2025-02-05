using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue7240 : _IssuesUITest
{
	const string ClickMe = "ClickMe";

	public Issue7240(TestDevice testDevice) : base(testDevice)
	{
	}


	public override string Issue => "[Android] Shell content layout hides navigated to page";

	[Test]
	[Category(UITestCategories.Shell)]
	public void ShellSecondPageHasSameLayoutAsPrimary()
	{
		App.WaitForElement(ClickMe);
		App.Tap(ClickMe);
		App.WaitForElement("Page Count:2");
		App.Tap(ClickMe);
		App.WaitForElement("Page Count:3");
	}
}