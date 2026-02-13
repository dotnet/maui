#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_ANDROID // Clicking an already selected tab does not trigger navigation events on Windows or Android
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue25599_1 : _IssuesUITest
{
	public Issue25599_1(TestDevice testDevice) : base(testDevice)
	{
	}
	public override string Issue => "Shell Navigating event shows identical Current and Target on tab click";

	[Test]
	[Category(UITestCategories.Shell)]
	public void VerifyNavigatingEventOnTabReselection()
	{
		App.WaitForElement("PushButton");
		App.Tap("PushButton");
		App.WaitForElement("PopButton");
		App.TapTab("Home");
		var currentLabel = App.WaitForElement("MainPageNavigatingCurrentLabel");
		var targetLabel = App.WaitForElement("MainPageNavigatingTargetLabel");
		Assert.That(currentLabel.GetText(), Is.Not.EqualTo(targetLabel.GetText()));
	}
}
#endif