#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_ANDROID // More tab is available in Mac and iOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue16175 : _IssuesUITest
{
	public Issue16175(TestDevice device) : base(device)
	{
	}
	public override string Issue => "OnNavigatedTo event triggered in More tabs";

	[Test]
	[Category(UITestCategories.TabbedPage)]
	public void TabEventsTriggeredInMoreTab()
	{
		App.TapTab("More");
		App.TapTab("Tab8");
		var navigatedToLabel = App.WaitForElement("navigatedToLabel");
		Assert.That(navigatedToLabel.GetText(), Is.EqualTo("NavigatedTo: Triggered"));
	}
}
#endif