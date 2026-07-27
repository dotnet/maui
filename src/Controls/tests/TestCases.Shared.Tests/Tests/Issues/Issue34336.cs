using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34336 : _IssuesUITest
{
	public override string Issue => "[iOS] CollectionView has excessive height if ObservableCollection source delayed in loading";

	public Issue34336(TestDevice device) : base(device) { }

	// Verifies that a horizontal CollectionView inside a Grid row set to Auto correctly
	// resizes to fit its content after a delayed ObservableCollection population.
	// Bug: On iOS the CollectionView retains excessive height (full-screen) instead of
	// shrinking to match its items once they arrive after a 3-second delay.
	[Test]
	[Category(UITestCategories.CollectionView)]
	public void CollectionViewHeightIsCorrectAfterDelayedLoad()
	{
		// Wait for page to load
		App.WaitForElement("BelowCollectionViewLabel");

		// Wait for items to appear after the delayed load.
		App.WaitForElement("ITEM 0", timeout: TimeSpan.FromSeconds(3));

		// Using screenshot instead of assert checks as a safer approach,
		// since pixel-level differences can occur across platforms.
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
}
