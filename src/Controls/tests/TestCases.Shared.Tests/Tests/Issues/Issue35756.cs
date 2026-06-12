using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue35756 : _IssuesUITest
{
	public Issue35756(TestDevice device) : base(device) { }

	public override string Issue => "OnNavigatedTo does not fire after PopModalAsync when tab was changed from inside the modal";

	[Test]
	[Category(UITestCategories.TabbedPage)]
	public void OnNavigatedToFiresAfterPopModalWhenTabChangedFromInsideModal()
	{
		App.WaitForElement("Tab1Content");
		Assert.That(App.WaitForElement("Tab1NavigatedToCount").GetText(), Is.EqualTo("NavigatedTo count: 1"));

		App.TapTab("Tab 2");
		App.WaitForElement("Tab2Content");
		App.Tap("PushModalButton");
		App.WaitForElement("ModalContent");

		App.Tap("SwitchToTab1Button");

		App.Tap("CloseModalButton");
		App.WaitForElement("Tab1Content");

		Assert.That(App.WaitForElement("Tab1NavigatedToCount").GetText(), Is.EqualTo("NavigatedTo count: 3"));
	}
}
