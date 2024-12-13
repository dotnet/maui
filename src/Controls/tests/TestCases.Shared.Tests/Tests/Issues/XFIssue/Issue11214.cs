﻿#if TEST_FAILS_ON_WINDOWS // FlyoutItems added dynamically during navigation are not displayed on Windows. More information: https://github.com/dotnet/maui/issues/26391.
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

	[Test]
	[Category(UITestCategories.Shell)]

	public void FlyoutItemChangesPropagateCorrectlyToPlatformForShellElementsNotCurrentlyActive()
	{
		App.WaitForElement("PageLoaded");
		App.TapInShellFlyout("ExpandMe");
		App.ShowFlyout();
		for (int i = 0; i < 2; i++)
			App.WaitForElement($"Some Item: {i}");
		App.Tap("ExpandMe");
		App.ShowFlyout();
		for (int i = 0; i < 2; i++)
			App.WaitForNoElement($"Some Item: {i}");
	}

}
#endif 