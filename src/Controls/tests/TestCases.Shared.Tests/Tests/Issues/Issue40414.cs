using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue40414 : _IssuesUITest
{
	public override string Issue => "CollectionViewHandler2 does not display grouped CollectionView items after ItemsSource is reset";

	public Issue40414(TestDevice device)
	: base(device)
	{ }

	[TestCase("OpenGridButton")]
	[TestCase("OpenListButton")]
	[ShardedTestCategory(UITestCategories.CollectionView)]
	public void GroupedItemsAreDisplayedAfterItemsSourceResetAndNavigation(string openButton)
	{
		App.WaitForElement(openButton);
		App.Tap(openButton);
		AssertGenerationVisible(1);

		App.Tap("ResetItemsSourceButton");
		AssertGenerationVisible(2);

		App.Tap("NavigateAwayButton");
		App.WaitForElement(openButton);
		App.Tap(openButton);
		AssertGenerationVisible(3);
	}

	void AssertGenerationVisible(int generation)
	{
		var collectionRect = App.WaitForElement("GroupedCollectionView").GetRect();
		foreach (var suffix in new[] { "Group1", "Item1_1", "Item1_4" })
		{
			var text = $"Generation{generation}{suffix}";
			var element = App.WaitForElement(text);
			Assert.That(element.GetText(), Is.EqualTo(text));
			var rect = element.GetRect();
			Assert.That(rect.Width, Is.GreaterThan(0));
			Assert.That(rect.Height, Is.GreaterThan(0));
			Assert.That(rect.X, Is.GreaterThanOrEqualTo(collectionRect.X));
			Assert.That(rect.Y, Is.GreaterThanOrEqualTo(collectionRect.Y));
			Assert.That(rect.X + rect.Width, Is.LessThanOrEqualTo(collectionRect.X + collectionRect.Width));
			Assert.That(rect.Y + rect.Height, Is.LessThanOrEqualTo(collectionRect.Y + collectionRect.Height));
		}
	}
}
