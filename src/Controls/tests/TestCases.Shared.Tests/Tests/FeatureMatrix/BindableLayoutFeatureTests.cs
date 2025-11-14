using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

[Category(UITestCategories.Layout)]
public class BindableLayoutFeatureTests : UITest
{
	public const string BindableLayoutFeatureMatrix = "BindableLayout Feature Matrix";

	public BindableLayoutFeatureTests(TestDevice device)
		: base(device)
	{
	}

	protected override void FixtureSetup()
	{
		base.FixtureSetup();
		App.NavigateToGallery(BindableLayoutFeatureMatrix);
	}

	[Test, Order(1)]
	public void VerifyBindableLayoutWithItemsSourceNone()
	{
		App.WaitForElement("StackLayoutWithBindableLayout");
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ItemsSourceNone");
		App.Tap("ItemsSourceNone");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("StackLayoutWithBindableLayout");
		VerifyScreenshot();
	}

	[Test, Order(2)]
	public void VerifyBindableLayoutWithItemsSourceObservableCollection()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ItemsSourceObservableCollection");
		App.Tap("ItemsSourceObservableCollection");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("StackLayoutWithBindableLayout");
		VerifyScreenshot();
	}

	[Test, Order(3)]
	public void VerifyBindableLayoutWithItemsSourceEmptyCollection()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ItemsSourceEmptyCollection");
		App.Tap("ItemsSourceEmptyCollection");
		App.WaitForElement("EmptyViewString");
		App.Tap("EmptyViewString");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("StackLayoutWithBindableLayout");
		App.WaitForElement("No Items Available(String)");
	}

	[Test, Order(4)]
	public void VerifyBindableLayoutWithEmptyViewString()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ItemsSourceEmptyCollection");
		App.Tap("ItemsSourceEmptyCollection");
		App.WaitForElement("EmptyViewString");
		App.Tap("EmptyViewString");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("StackLayoutWithBindableLayout");
		App.WaitForElement("No Items Available(String)");
	}

	[Test, Order(5)]
	public void VerifyBindableLayoutWithEmptyViewView()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ItemsSourceEmptyCollection");
		App.Tap("ItemsSourceEmptyCollection");
		App.WaitForElement("EmptyViewGrid");
		App.Tap("EmptyViewGrid");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("StackLayoutWithBindableLayout");
		App.WaitForElement("No Items Available(Grid View)");
	}

	[Test, Order(6)]
	public void VerifyBindableLayoutWithEmptyViewTemplate()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("EmptyViewTemplateGrid");
		App.Tap("EmptyViewTemplateGrid");
		App.WaitForElement("ItemsSourceEmptyCollection");
		App.Tap("ItemsSourceEmptyCollection");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("StackLayoutWithBindableLayout");
		App.WaitForElement("No Template Items Available(Grid View)");
	}

	[Test, Order(7)]
	public void VerifyBindableLayoutWithBasicItemTemplate()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ItemsSourceObservableCollection");
		App.Tap("ItemsSourceObservableCollection");
		App.WaitForElement("ItemTemplateBasic");
		App.Tap("ItemTemplateBasic");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("StackLayoutWithBindableLayout");
		VerifyScreenshot();
	}

	[Test, Order(8)]
	public void VerifyBindableLayoutWithGridItemTemplate()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ItemsSourceObservableCollection");
		App.Tap("ItemsSourceObservableCollection");
		App.WaitForElement("ItemTemplateGrid");
		App.Tap("ItemTemplateGrid");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("StackLayoutWithBindableLayout");
		VerifyScreenshot();
	}

	[Test, Order(9)]
	public void VerifyBindableLayoutWithItemTemplateSelector()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ItemsSourceObservableCollection");
		App.Tap("ItemsSourceObservableCollection");
		App.WaitForElement("ItemTemplateSelectorAlternate");
		App.Tap("ItemTemplateSelectorAlternate");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("StackLayoutWithBindableLayout");
		VerifyScreenshot();
	}

	[Test, Order(10)]
	public void VerifyBindableLayoutWithEmptyViewStringAndEmptyViewTemplate()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ItemsSourceEmptyCollection");
		App.Tap("ItemsSourceEmptyCollection");
		App.WaitForElement("EmptyViewString");
		App.Tap("EmptyViewString");
		App.WaitForElement("EmptyViewTemplateGrid");
		App.Tap("EmptyViewTemplateGrid");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("StackLayoutWithBindableLayout");
		App.WaitForElement("No Template Items Available(Grid View)");
	}

	[Test, Order(11)]
	public void VerifyBindableLayoutWithEmptyViewViewAndEmptyViewTemplate()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ItemsSourceEmptyCollection");
		App.Tap("ItemsSourceEmptyCollection");
		App.WaitForElement("EmptyViewGrid");
		App.Tap("EmptyViewGrid");
		App.WaitForElement("EmptyViewTemplateGrid");
		App.Tap("EmptyViewTemplateGrid");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("StackLayoutWithBindableLayout");
		App.WaitForElement("No Template Items Available(Grid View)");
	}

	[Test, Order(12)]
	public void VerifyBindableLayoutWithAddItems()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ItemsSourceObservableCollection");
		App.Tap("ItemsSourceObservableCollection");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForNoElement("Dragonfruit");
		App.WaitForElement("AddItems");
		App.Tap("AddItems");
		App.WaitForElement("Dragonfruit");
	}

	[Test, Order(13)]
	public void VerifyBindableLayoutWithAddItemsAtIndex()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ItemsSourceObservableCollection");
		App.Tap("ItemsSourceObservableCollection");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForNoElement("Chikoo");
		App.WaitForElement("IndexEntry");
		App.ClearText("IndexEntry");
		App.EnterText("IndexEntry", "2");
		App.WaitForElement("AddItems");
		App.Tap("AddItems");
		App.WaitForElement("Chikoo");
	}

	[Test, Order(14)]
	public void VerifyBindableLayoutWithRemoveItems()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ItemsSourceObservableCollection");
		App.Tap("ItemsSourceObservableCollection");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Spinach");
		App.WaitForElement("RemoveItems");
		App.Tap("RemoveItems");
		App.WaitForNoElement("Spinach");
	}

	[Test, Order(15)]
	public void VerifyBindableLayoutWithRemoveItemsAtIndex()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ItemsSourceObservableCollection");
		App.Tap("ItemsSourceObservableCollection");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Broccoli");
		App.WaitForElement("IndexEntry");
		App.ClearText("IndexEntry");
		App.EnterText("IndexEntry", "3");
		App.WaitForElement("RemoveItems");
		App.Tap("RemoveItems");
		App.WaitForNoElement("Broccoli");
	}

	[Test, Order(16)]
	public void VerifyBindableLayoutWithReplaceItems()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ItemsSourceObservableCollection");
		App.Tap("ItemsSourceObservableCollection");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Apple");
		App.WaitForNoElement("Cat");
		App.WaitForElement("ReplaceItems");
		App.Tap("ReplaceItems");
		App.WaitForNoElement("Apple");
		App.WaitForElement("Cat");
	}

	[Test, Order(17)]
	public void VerifyBindableLayoutWithReplaceItemsAtIndex()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ItemsSourceObservableCollection");
		App.Tap("ItemsSourceObservableCollection");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("Carrot");
		App.WaitForNoElement("Monkey");
		App.WaitForElement("IndexEntry");
		App.ClearText("IndexEntry");
		App.EnterText("IndexEntry", "2");
		App.WaitForElement("ReplaceItems");
		App.Tap("ReplaceItems");
		App.WaitForNoElement("Carrot");
		App.WaitForElement("Monkey");
	}
}