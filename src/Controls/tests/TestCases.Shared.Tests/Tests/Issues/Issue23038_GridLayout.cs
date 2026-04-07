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
	public void GridGroupHeaderShouldStretchToFullWidth()
	{
		var collectionViewRect = App.WaitForElement("GroupedGridCollectionView").GetRect();
		var headerRect = App.WaitForElement("GridHeaderTeamA").GetRect();
		var footerRect = App.WaitForElement("GridFooterTeamA").GetRect();

		// Footer (regular grid item container) should stretch to CollectionView width
		Assert.That(footerRect.Width, Is.EqualTo(collectionViewRect.Width).Within(20),
			"Grid group footer should stretch to full CollectionView width");

		// Header must match footer width — both should stretch to full width
		Assert.That(headerRect.Width, Is.EqualTo(footerRect.Width).Within(2),
			"Grid group header width should match grid group footer width");
	}
}
