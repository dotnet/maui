using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue11523 : _IssuesUITest
{
	public Issue11523(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Bug] FlyoutBehavior.Disabled removes back-button from navbar";

	// [Test]
	// [Category(UITestCategories.Shell)]
	// public void BackButtonStillVisibleWhenFlyoutBehaviorDisabled()
	// {
	// 	App.WaitForElement("PageLoaded");
	// 	App.WaitForElement(BackButtonAutomationId);
	// 	App.Tap(BackButtonAutomationId);
	// 	App.WaitForElement(FlyoutIconAutomationId);
	// }
}