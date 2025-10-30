using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32239 : _IssuesUITest
{
	public Issue32239(TestDevice testDevice) : base(testDevice)
	{
	}
	public override string Issue => "RemovePage fails to disconnect handlers when the page is not visible";

	[Test]
	[Category(UITestCategories.Navigation)]
	public void RemovePageDisconnectsHandlers()
	{
		App.WaitForElement("PushModalButton");
		App.Tap("PushModalButton");
		App.WaitForElement("PushPage2Button");
		App.Tap("PushPage2Button");
		App.WaitForElement("RemovePage1Button");
		App.Tap("RemovePage1Button");
		var label = App.WaitForElement("StatusLabel");
		Assert.That(label.GetText(), Is.EqualTo("Page 1 Handler Status: DISCONNECTED"));
	}
}