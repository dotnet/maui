using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue23038 : _IssuesUITest
{
	public override string Issue => "[Windows] GroupHeaderTemplate width smaller than ItemTemplate";

	public Issue23038(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.CollectionView)]
	[FailsOnAndroidWhenRunningOnXamarinUITest("Windows-only test: validates WinUI CollectionView group header styles")]
	[FailsOnIOSWhenRunningOnXamarinUITest("Windows-only test: validates WinUI CollectionView group header styles")]
	[FailsOnMacWhenRunningOnXamarinUITest("Windows-only test: validates WinUI CollectionView group header styles")]
	public void GroupHeaderShouldStretchToFullWidth()
	{
		var collectionViewRect = App.WaitForElement("GroupedCollectionView").GetRect();
		var headerARect = App.WaitForElement("HeaderTeamA").GetRect();
		var footerARect = App.WaitForElement("FooterTeamA").GetRect();
		var headerBRect = App.WaitForElement("HeaderTeamB").GetRect();
		var footerBRect = App.WaitForElement("FooterTeamB").GetRect();

		// Primary assertion: header must stretch to full CollectionView width.
		// Tolerance of 20px accounts for the possible scrollbar width on Windows.
		Assert.That(headerARect.Width, Is.EqualTo(collectionViewRect.Width).Within(20),
			"Group header (TeamA) should stretch to full CollectionView width");

		// Footer (regular list item) should also stretch to CollectionView width.
		Assert.That(footerARect.Width, Is.EqualTo(collectionViewRect.Width).Within(20),
			"Group footer (TeamA) should stretch to full CollectionView width");

		// Header must match footer width — both stretch to full width.
		// Tolerance of 5px accounts for device-pixel rounding under DPI scaling.
		Assert.That(headerARect.Width, Is.EqualTo(footerARect.Width).Within(5),
			"Group header (TeamA) width should match group footer width");

		// Second group must also stretch — protects against virtualization/container-recycling regressions.
		Assert.That(headerBRect.Width, Is.EqualTo(collectionViewRect.Width).Within(20),
			"Group header (TeamB) should stretch to full CollectionView width");
		Assert.That(headerBRect.Width, Is.EqualTo(footerBRect.Width).Within(5),
			"Group header (TeamB) width should match group footer width");

		// Vertical spacing sanity: header sits within the CollectionView and above its footer.
		Assert.That(headerARect.Y, Is.GreaterThanOrEqualTo(collectionViewRect.Y),
			"Group header should render at or below the CollectionView top edge");
		Assert.That(headerARect.Y, Is.LessThan(footerARect.Y),
			"Group header should render above its group footer");
	}
}
