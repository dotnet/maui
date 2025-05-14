#if IOS || MACCATALYST // More tab is available in MAC and iOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue16175 : _IssuesUITest
{
	public Issue16175(TestDevice testDevice) : base(testDevice)
	{
	}
	public override string Issue => "OnNavigatedTo was not triggered when tabs in More section";

	[Test]
	[Category(UITestCategories.TabbedPage)]
	public void TabbedPageTabEventsTriggeredInMoreSection()
	{
		App.WaitForElement("More");
		App.Tap("More");
		App.WaitForElement("8");
		App.Tap("8");
		var navigatedToLabel = App.WaitForElement("navigatedToLabel");
		Assert.That(navigatedToLabel.GetText(), Is.EqualTo("NavigatedTo: Triggered"));
	}
}
#endif