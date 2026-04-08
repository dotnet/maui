using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue23038_GridLayout : _IssuesUITest
{
	public override string Issue => "[Windows] GroupHeaderTemplate width smaller than ItemTemplate (GridLayout)";

	public Issue23038_GridLayout(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.CollectionView)]
	[FailsOnAndroidWhenRunningOnXamarinUITest("Windows-only test: validates WinUI CollectionView group header styles")]
	[FailsOnIOSWhenRunningOnXamarinUITest("Windows-only test: validates WinUI CollectionView group header styles")]
	[FailsOnMacWhenRunningOnXamarinUITest("Windows-only test: validates WinUI CollectionView group header styles")]
	public void GridGroupHeaderShouldStretchToFullWidth()
	{
		var collectionViewRect = App.WaitForElement("GroupedGridCollectionView").GetRect();
		var headerRect = App.WaitForElement("GridHeaderTeamA").GetRect();

		// In a GridItemsLayout, group headers use GridViewHeaderItem and should span the full width.
		// Group footers are regular grid items that only span one column, so we compare header to CollectionView.
		Assert.That(headerRect.Width, Is.EqualTo(collectionViewRect.Width).Within(20),
			"Grid group header should stretch to full CollectionView width");
	}
}
