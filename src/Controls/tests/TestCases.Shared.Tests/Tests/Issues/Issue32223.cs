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

	[Test, Order(1)]
	[Category(UITestCategories.CollectionView)]
	public void CanReorderWithItemDataTemplateSelector_1()
	{
		App.WaitForElement("ReorderableCollectionView");
		App.DragAndDrop("David", "Charlie");
		VerifyScreenshot();
	}

	[Test, Order(2)]
	[Category(UITestCategories.CollectionView)]
	public void CanReorderWithItemDataTemplateSelector_2()
	{
		App.WaitForElement("ReorderableCollectionView");
		App.DragAndDrop("David", "Alice");
		VerifyScreenshot();
	}

	[Test, Order(3)]
	[Category(UITestCategories.CollectionView)]
	public void CanReorderWithItemDataTemplateSelector_3()
	{
		App.WaitForElement("ReorderableCollectionView");
		App.DragAndDrop("Charlie", "Alice");
		VerifyScreenshot();
	}
}