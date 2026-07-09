#if ANDROID
// This fix is Android-specific. The same bug on iOS, macOS, and Windows is tracked
// separately in https://github.com/dotnet/maui/issues/36230.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue35643 : _IssuesUITest
{
	public Issue35643(TestDevice device) : base(device) { }

	public override string Issue => "CurrentItem is updated incorrectly on Android when the CarouselView is bound to an ObservableCollection with Loop = false";

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void CurrentItemShouldUpdateWhenCurrentItemIsReplaced()
	{
		App.WaitForElement("CurrentItemLabel");
		Assert.That(App.FindElement("CurrentItemLabel").GetText(), Is.EqualTo("2"), "Initial CurrentItem should be '2'");
		Assert.That(App.FindElement("PositionLabel").GetText(), Is.EqualTo("2"), "Initial Position should be 2");

		App.Tap("UpdateButton");

		Assert.That(App.FindElement("CurrentItemLabel").GetText(), Is.EqualTo("2b"),
			"CurrentItem should be '2b' after replacing the item");
		Assert.That(App.FindElement("PositionLabel").GetText(), Is.EqualTo("2"),
			"Position should stay at 2 after replacing the item");
	}

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void CurrentItemShouldUpdateWhenCurrentItemIsReplacedInLoopMode()
	{
		App.WaitForElement("LoopCurrentItemLabel");
		Assert.That(App.FindElement("LoopCurrentItemLabel").GetText(), Is.EqualTo("C"), "Initial CurrentItem should be 'C'");
		Assert.That(App.FindElement("LoopPositionLabel").GetText(), Is.EqualTo("2"), "Initial Position should be 2");

		App.ScrollTo("LoopReplaceButton");
		App.Tap("LoopReplaceButton");

		App.WaitForElement("LoopCurrentItemLabel");
		Assert.That(App.FindElement("LoopCurrentItemLabel").GetText(), Is.EqualTo("C2"),
			"CurrentItem should be 'C2' after replacing the item in loop mode");
		Assert.That(App.FindElement("LoopPositionLabel").GetText(), Is.EqualTo("2"),
			"Position should stay at 2 after replacing the item in loop mode");

		// The VM-bound labels above are updated directly by the button handler and would pass
		// even if the on-screen carousel cell itself was never rebound. Confirm the visible
		// loop carousel cell (whose AutomationId is bound to the item value) was actually
		// rebound to the replacement value by RebindVisibleLoopItem.
		App.WaitForElement("C2");
	}
}
#endif
