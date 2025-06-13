using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue1769 : _IssuesUITest
{
	const string GoToPageTwoButtonText = "Go To Page 2";
	const string SwitchAutomatedId = nameof(SwitchAutomatedId);
	const string SwitchIsNowLabelTextFormat = "Switch is now {0}";

	public Issue1769(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "PushAsync with Switch produces NRE";

	[Fact]
	[Category(UITestCategories.Switch)]
	[Category(UITestCategories.Compatibility)]
	public void Issue1769Test()
	{
		App.WaitForElement(GoToPageTwoButtonText);
		App.Tap(GoToPageTwoButtonText);

		App.WaitForElement(SwitchAutomatedId);
		Assert.Equal(string.Format(SwitchIsNowLabelTextFormat, false, App.FindElement("SwitchLabel").GetText()));
		App.Tap(SwitchAutomatedId);
		Assert.Equal(string.Format(SwitchIsNowLabelTextFormat, true, App.FindElement("SwitchLabel").GetText()));
	}
}