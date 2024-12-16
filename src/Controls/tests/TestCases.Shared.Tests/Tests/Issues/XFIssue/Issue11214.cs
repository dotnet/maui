using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue11214 : _IssuesUITest
{
	const string flyoutString =
#if WINDOWS
	 "Click Me and You Should see 2 Items show up";
#else
		"ExpandMe";
#endif
	public Issue11214(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "When adding FlyoutItems during Navigating only first one is shown";

	[Test]
	[Category(UITestCategories.Shell)]

	public void FlyoutItemChangesPropagateCorrectlyToPlatformForShellElementsNotCurrentlyActive()
	{
		App.WaitForElement("PageLoaded");
		App.TapInShellFlyout(flyoutString);
		App.ShowFlyout();
		for (int i = 0; i < 2; i++)
			App.WaitForElement($"Some Item: {i}");
		App.Tap(flyoutString);
		App.ShowFlyout();
		for (int i = 0; i < 2; i++)
			App.WaitForNoElement($"Some Item: {i}");
	}

}