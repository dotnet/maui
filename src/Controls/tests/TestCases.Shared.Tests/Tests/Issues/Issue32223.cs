using NUnit.Framework;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Interactions;
using OpenQA.Selenium.Interactions;
using UITest.Appium;
using UITest.Core;
using PointerInputDevice = OpenQA.Selenium.Appium.Interactions.PointerInputDevice;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32223 : _IssuesUITest
{
	public Issue32223(TestDevice testDevice) : base(testDevice)
	{
	}
	public override string Issue => "[Android] CollectionView items do not reorder correctly when using an item DataTemplateSelector";

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 2)]
	public void CanReorderWithItemDataTemplateSelector()
	{
		App.WaitForElement("ReorderableCollectionView");
		App.WaitForElement("Charlie");
		App.WaitForElement("David");
		if (App is AppiumApp appiumApp && App is (AppiumWindowsApp or AppiumAndroidApp))
		{
			var source = appiumApp.Driver.FindElement(App is AppiumWindowsApp ? MobileBy.AccessibilityId("David") : MobileBy.Id("David"));
			var target = appiumApp.Driver.FindElement(App is AppiumWindowsApp ? MobileBy.AccessibilityId("Charlie") : MobileBy.Id("Charlie"));
			var touch = new PointerInputDevice(PointerKind.Touch);
			var drag = new ActionSequence(touch, 0);
			drag.AddAction(touch.CreatePointerMove(source, 0, 0, TimeSpan.FromMilliseconds(5)));
			drag.AddAction(touch.CreatePointerDown(PointerButton.TouchContact));
			drag.AddAction(touch.CreatePause(TimeSpan.FromSeconds(1)));
			// Cross the insertion boundary; center-to-center stops exactly at Android's swap threshold.
			drag.AddAction(touch.CreatePointerMove(target, -target.Size.Width / 4, 0, TimeSpan.FromSeconds(1)));
			drag.AddAction(touch.CreatePointerUp(PointerButton.TouchContact));
			appiumApp.Driver.PerformActions([drag]);
		}
		else
		{
			App.DragAndDrop("David", "Charlie");
		}

		App.RetryAssert(() =>
		{
			Assert.That(App.WaitForElement("ReorderedLabel").GetText(), Is.EqualTo("Success"));
			var david = App.WaitForElementAndGetRect("David");
			var charlie = App.WaitForElementAndGetRect("Charlie");
			Assert.That(david.X, Is.LessThan(charlie.X), "David must move before Charlie in the two-column grid.");
			Assert.That(david.Y, Is.EqualTo(charlie.Y).Within(5));
		});
	}
}