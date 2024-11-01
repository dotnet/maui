using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue11214 : _IssuesUITest
{
	public Issue11214(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "When adding FlyoutItems during Navigating only first one is shown";

	/*
	[Test]
	[Category(UITestCategories.Shell)]
	[FailsOnIOS]
	public void FlyoutItemChangesPropagateCorrectlyToPlatformForShellElementsNotCurrentlyActive()
	{
		App.WaitForElement("PageLoaded");
		TapInFlyout("ExpandMe", makeSureFlyoutStaysOpen: true);

		for (int i = 0; i < 2; i++)
			App.WaitForElement($"Some Item: {i}");

		TapInFlyout("ExpandMe", makeSureFlyoutStaysOpen: true);

		for (int i = 0; i < 2; i++)
			App.WaitForNoElement($"Some Item: {i}");
	}
	*/
}