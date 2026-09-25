using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

[Category(UITestCategories.Layout)]
public class BindableLayoutFeatureTests : _GalleryUITest
{
	public const string BindableLayoutFeatureMatrix = "BindableLayout Feature Matrix";
	public override string GalleryPageName => BindableLayoutFeatureMatrix;

	public BindableLayoutFeatureTests(TestDevice device)
		: base(device)
	{
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

}