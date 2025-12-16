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
		App.WaitForElement("Broccoli");
		App.WaitForElement("RemoveItems");
		App.Tap("RemoveItems");
		App.WaitForNoElement("Broccoli");
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
		App.WaitForElement("Banana");
		App.WaitForNoElement("Cat");
		App.WaitForNoElement("Dog");
		App.WaitForElement("ReplaceItems");
		App.Tap("ReplaceItems");
		App.WaitForElement("ReplaceItems");
		App.Tap("ReplaceItems");
		App.WaitForNoElement("Banana");
		App.WaitForElement("Cat");
		App.WaitForElement("Dog");
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

	[Test, Order(18)]
	public void VerifyBindableLayoutWithGetAndSetEmptyView()
    {
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ItemsSourceEmptyCollection");
		App.Tap("ItemsSourceEmptyCollection");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SetEmptyView");
		App.Tap("SetEmptyView");
		App.WaitForElement("(Set EmptyView)");
		App.WaitForElement("GetEmptyView");
		App.Tap("GetEmptyView");
		App.WaitForElement("DirectApiSummaryLabel");
		Assert.That(App.FindElement("DirectApiSummaryLabel").GetText(),Is.EqualTo("[Get EmptyView] EmptyView=True EmptyViewTemplate=False ItemsSourceCount=0 ItemTemplate=True ItemTemplateSelector=False HasEmptyView=True"));
    }

	[Test, Order(19)]
	public void VerifyBindableLayoutWithGetAndSetEmptyViewTemplate()
    {
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ItemsSourceEmptyCollection");
		App.Tap("ItemsSourceEmptyCollection");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SetEmptyViewTemplate");
		App.Tap("SetEmptyViewTemplate");
		App.WaitForElement("(Set EmptyViewTemplate)");
		App.WaitForElement("GetEmptyViewTemplate");
		App.Tap("GetEmptyViewTemplate");
		App.WaitForElement("DirectApiSummaryLabel");
		Assert.That(App.FindElement("DirectApiSummaryLabel").GetText(), Is.EqualTo("[Get EmptyViewTemplate] EmptyView=False EmptyViewTemplate=True ItemsSourceCount=0 ItemTemplate=True ItemTemplateSelector=False HasEmptyViewTemplate=True"));
    }

	[Test, Order(20)]
	public void VerifyBindableLayoutWithGetAndSetItemSource()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ItemsSourceObservableCollection");
		App.Tap("ItemsSourceObservableCollection");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SetItemsSource");
		App.Tap("SetItemsSource");
		App.WaitForElement("Apple");
		App.WaitForElement("GetItemsSource");
		App.Tap("GetItemsSource");
		App.WaitForElement("DirectApiSummaryLabel");
		Assert.That(App.FindElement("DirectApiSummaryLabel").GetText(), Is.EqualTo("[Get ItemsSource] EmptyView=False EmptyViewTemplate=False ItemsSourceCount=4 ItemTemplate=True ItemTemplateSelector=False Count=4"));
	}

	[Test, Order(21)]
	public void VerifyBindableLayoutWithGetAndSetItemTemplate()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ItemsSourceObservableCollection");
		App.Tap("ItemsSourceObservableCollection");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SetItemTemplate");
		App.Tap("SetItemTemplate");
		App.WaitForElement("Apple");
		App.WaitForElement("GetItemTemplate");
		App.Tap("GetItemTemplate");
		App.WaitForElement("DirectApiSummaryLabel");
		Assert.That(App.FindElement("DirectApiSummaryLabel").GetText(), Is.EqualTo("[Get ItemTemplate] EmptyView=False EmptyViewTemplate=False ItemsSourceCount=4 ItemTemplate=True ItemTemplateSelector=False HasItemTemplate=True"));
	}

	[Test, Order(22)]
	public void VerifyBindableLayoutWithGetAndSetItemTemplateSelector()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ItemsSourceObservableCollection");
		App.Tap("ItemsSourceObservableCollection");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SetItemTemplateSelector");
		App.Tap("SetItemTemplateSelector");
		App.WaitForElement("Apple");
		App.WaitForElement("GetItemTemplateSelector");
		App.Tap("GetItemTemplateSelector");
		App.WaitForElement("DirectApiSummaryLabel");
		Assert.That(App.FindElement("DirectApiSummaryLabel").GetText(), Is.EqualTo("[Get ItemTemplateSelector] EmptyView=False EmptyViewTemplate=False ItemsSourceCount=4 ItemTemplate=False ItemTemplateSelector=True HasSelector=True"));
	}
}