using NUnit.Framework;
using OpenQA.Selenium.Appium;
using UITest.Appium;
using UITest.Core;

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
			// WinAppDriver's W3C actions do not support mouse input.
			windowsApp.Driver.ExecuteScript("windows: clickAndDrag", new Dictionary<string, object>
			{
				["startElementId"] = source.Id,
				["endElementId"] = target.Id,
				// The cell center is an insertion boundary; drop in its leading half.
				["endX"] = target.Size.Width / 4,
				["endY"] = target.Size.Height / 2,
				["durationMs"] = 1000
			});
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