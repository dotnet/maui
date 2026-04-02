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
		var collectionViewRect = App.WaitForElement("GroupedCollectionView").GetRect();
		var headerRect = App.WaitForElement("HeaderTeamA").GetRect();
		var footerRect = App.WaitForElement("FooterTeamA").GetRect();

		// Footer (regular list item) should stretch to CollectionView width; tolerance accounts for possible scrollbar
		Assert.That(footerRect.Width, Is.EqualTo(collectionViewRect.Width).Within(20),
			"Group footer should stretch to full CollectionView width");

		// Header must match footer width — both should stretch to full width
		Assert.That(headerRect.Width, Is.EqualTo(footerRect.Width).Within(2),
			"Group header width should match group footer width");
	}
}
