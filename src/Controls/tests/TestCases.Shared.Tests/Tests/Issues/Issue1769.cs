using NUnit.Framework;
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

	[Test]
	[Category(UITestCategories.Switch)]
	[Category(UITestCategories.Compatibility)]
	public void Issue1769Test()
	{
		App.WaitForElement(GoToPageTwoButtonText);
		App.Tap(GoToPageTwoButtonText);

		App.WaitForElement(SwitchAutomatedId);
		Assert.That(App.FindElement("SwitchLabel").GetText(), Is.EqualTo(string.Format(SwitchIsNowLabelTextFormat, false)));
		App.Tap(SwitchAutomatedId);
		Assert.That(App.FindElement("SwitchLabel").GetText(), Is.EqualTo(string.Format(SwitchIsNowLabelTextFormat, true)));
	}
}