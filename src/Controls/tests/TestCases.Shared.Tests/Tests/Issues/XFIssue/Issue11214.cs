using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue11214 : _IssuesUITest
{

#if WINDOWS // AutomationId for flyout items is not worked on Windows.
	const string FlyoutString = "Click Me and You Should see 2 Items show up";
#else
	const string FlyoutString = "ExpandMe";
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
		App.TapInShellFlyout(FlyoutString);
		App.ShowFlyout();
		for (int i = 0; i < 2; i++)
			App.WaitForElement($"Some Item: {i}");
		App.Tap(FlyoutString);
		App.ShowFlyout();
		for (int i = 0; i < 2; i++)
			App.WaitForNoElement($"Some Item: {i}");
	}
}