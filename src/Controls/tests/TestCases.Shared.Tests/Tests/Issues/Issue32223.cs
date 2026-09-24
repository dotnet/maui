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
		if (App is AppiumWindowsApp windowsApp)
		{
			var source = windowsApp.Driver.FindElement(MobileBy.AccessibilityId("David"));
			var target = windowsApp.Driver.FindElement(MobileBy.AccessibilityId("Charlie"));
			var mouse = new PointerInputDevice(PointerKind.Mouse);
			var drag = new ActionSequence(mouse, 0);
			drag.AddAction(mouse.CreatePointerMove(source, 0, 0, TimeSpan.Zero));
			drag.AddAction(mouse.CreatePointerDown(PointerButton.LeftMouse));
			// The cell center is an insertion boundary; drop in its leading half.
			drag.AddAction(mouse.CreatePointerMove(target, -target.Size.Width / 4, 0, TimeSpan.FromSeconds(1)));
			drag.AddAction(mouse.CreatePointerUp(PointerButton.LeftMouse));
			windowsApp.Driver.PerformActions([drag]);
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