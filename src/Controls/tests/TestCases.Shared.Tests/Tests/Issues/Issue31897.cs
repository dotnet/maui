using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue31897 : _IssuesUITest
{
	public Issue31897(TestDevice testDevice) : base(testDevice)
	{
	}
	public override string Issue => "CollectionView card height appears larger in Developer Balance sample";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void EnsureCollectionViewLayoutOnItemsSourceChange()
	{
		App.WaitForElement("GetHeightButton");
		App.Tap("GetHeightButton");
		var heightLabel = App.WaitForElement("HeightLabel");
		Assert.That(heightLabel.GetText(), Is.EqualTo("250"));
		App.Tap("RemoveItemsButton");
		App.Tap("GetHeightButton");
		Assert.That(heightLabel.GetText(), Is.EqualTo("250"));
		App.Tap("RemoveItemsButton");
		App.Tap("GetHeightButton");
		Assert.That(heightLabel.GetText(), Is.EqualTo("250"));
		App.Tap("AddItemsButton");
		App.Tap("GetHeightButton");
		Assert.That(heightLabel.GetText(), Is.EqualTo("250"));
		App.Tap("UpdateNewItemsButton");
		App.Tap("GetHeightButton");
		Assert.That(heightLabel.GetText(), Is.EqualTo("250"));
	}
}