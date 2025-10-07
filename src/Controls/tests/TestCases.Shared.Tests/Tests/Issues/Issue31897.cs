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
		var label = App.WaitForElement("HeightLabel");
		Assert.Equal("250", label.Height);
	}
}