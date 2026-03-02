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
	[Category(UITestCategories.CollectionView)]
	public void CanReorderWithItemDataTemplateSelector()
	{
		App.WaitForElement("ReorderableCollectionView");
		App.DragAndDrop("David", "Charlie");
		var expectedText = App.WaitForElement("ReorderedLabel").GetText();
		Assert.That(expectedText, Is.EqualTo("Success"));
	}
}