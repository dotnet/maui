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
		var headerARect = App.WaitForElement("GridHeaderTeamA").GetRect();
		var headerBRect = App.WaitForElement("GridHeaderTeamB").GetRect();

		// In a GridItemsLayout, group headers use GridViewHeaderItem and should span the full width.
		// (Group footers are regular grid items that only span one column, so we compare header to CollectionView.)
		// Tolerance of 20px accounts for the possible scrollbar width on Windows.
		Assert.That(headerARect.Width, Is.EqualTo(collectionViewRect.Width).Within(20),
			"Grid group header (TeamA) should stretch to full CollectionView width");

		// Second group must also stretch — protects against virtualization/container-recycling regressions.
		Assert.That(headerBRect.Width, Is.EqualTo(collectionViewRect.Width).Within(20),
			"Grid group header (TeamB) should stretch to full CollectionView width");

		// Vertical sanity: TeamA header sits within the CollectionView and above TeamB's header.
		Assert.That(headerARect.Y, Is.GreaterThanOrEqualTo(collectionViewRect.Y),
			"Grid group header should render at or below the CollectionView top edge");
		Assert.That(headerARect.Y, Is.LessThan(headerBRect.Y),
			"First group header should render above the second group header");
	}
}
