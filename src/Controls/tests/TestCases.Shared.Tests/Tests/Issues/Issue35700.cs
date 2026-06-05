using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue35700 : _IssuesUITest
{
	public Issue35700(TestDevice device) : base(device) { }

	public override string Issue => "Grouped CollectionView items not rendered properly on Android with GridItemsLayout";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void GroupedCollectionViewGridLayoutRendersCorrectly()
	{
		// Wait for the CollectionView to fully load and render
		App.WaitForElement("TestCollectionView");

		// Verify the layout renders correctly: the first group's first row
		// should have items uniformly sized and positioned across 5 columns.
		// On Android with Span=5 and VerticalItemSpacing=10, there was a regression
		// where the first row of the first group was incorrectly rendered.
		VerifyScreenshot();
	}
}
