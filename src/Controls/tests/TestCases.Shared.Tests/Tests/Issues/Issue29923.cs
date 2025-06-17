using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29923 : _IssuesUITest
{
	public Issue29923(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Removed page handlers not disconnected when using Navigation.RemovePage()";

	[Test]
	[Category(UITestCategories.Navigation)]
	public void RemovePageShouldDisconnectHandlers()
	{
		App.WaitForElement("NavigateToTestPageModalButton");
		App.Tap("NavigateToTestPageModalButton");
		App.WaitForElement("RemoveTestPage1Button");
		App.Tap("RemoveTestPage1Button");
		Assert.That(App.WaitForElement("HandlerStatusLabel").GetText(), Is.EqualTo("False"));
		App.Tap("PopModalButton");

		App.WaitForElement("NavigateToTestPageButton");
		App.Tap("NavigateToTestPageButton");
		App.WaitForElement("RemoveTestPage1Button");
		App.Tap("RemoveTestPage1Button");
		Assert.That(App.WaitForElement("HandlerStatusLabel").GetText(), Is.EqualTo("False"));
	}
}