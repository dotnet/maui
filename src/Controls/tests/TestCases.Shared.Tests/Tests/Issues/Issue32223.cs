using NUnit.Framework;
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
		App.DragAndDrop("David", "Charlie");
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