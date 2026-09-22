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
	[Retry(5)]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 2)]
	[FailsOnAndroidWhenRunningOnXamarinUITest("Flaky in CI (failed->passed on same SHA on net11.0; see ui-flake-quarantine-20260922.csv). Re-enable after flakiness investigation.")]
	public void CanReorderWithItemDataTemplateSelector()
	{
		App.WaitForElement("ReorderableCollectionView");
		App.DragAndDrop("David", "Charlie");
		var expectedText = App.WaitForElement("ReorderedLabel").GetText();
		Assert.That(expectedText, Is.EqualTo("Success"));
	}
}