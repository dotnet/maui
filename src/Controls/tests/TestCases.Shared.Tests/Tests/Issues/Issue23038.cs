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
	public void GroupHeaderShouldStretchToFullWidth()
	{
		App.WaitForElement("GroupedCollectionView");

		// Get the CollectionView bounds
		var collectionViewRect = App.WaitForElement("GroupedCollectionView").GetRect();

		// Get the group header bounds
		var headerRect = App.WaitForElement("Header_Team A").GetRect();

		// Get the group footer bounds (footers are regular list items, they already stretch)
		var footerRect = App.WaitForElement("Footer_Team A").GetRect();

		// Verify the footer stretches to CollectionView width (validates our baseline)
		Assert.That(footerRect.Width, Is.EqualTo(collectionViewRect.Width).Within(20),
			"Group footer should stretch to full CollectionView width");

		// The header width should match the footer width (both should stretch to full width)
		Assert.That(headerRect.Width, Is.EqualTo(footerRect.Width).Within(2),
			"Group header width should match group footer width");
	}
}
