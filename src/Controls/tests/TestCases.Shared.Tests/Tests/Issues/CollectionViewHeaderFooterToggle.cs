using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class CollectionViewHeaderFooterToggle : _IssuesUITest
	{
		const string HeaderButtonId = "ToggleHeaderButton";
		const string FooterButtonId = "ToggleFooterButton";
		const string HeaderLabelId = "HeaderLabel";
		const string FooterLabelId = "FooterLabel";

		public CollectionViewHeaderFooterToggle(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "CollectionView Header/Footer Toggle Issue";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void HeaderCanBeRemovedAndReAdded()
		{
			// Verify header is visible initially
			App.WaitForElement(HeaderLabelId);

			// Remove header
			App.Tap(HeaderButtonId);
			App.WaitForNoElement(HeaderLabelId, timeout: TimeSpan.FromSeconds(2));

			// Re-add header
			App.Tap(HeaderButtonId);
			App.WaitForElement(HeaderLabelId);
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void FooterCanBeRemovedAndReAdded()
		{
			// Verify footer is visible initially
			App.WaitForElement(FooterLabelId);

			// Remove footer
			App.Tap(FooterButtonId);
			App.WaitForNoElement(FooterLabelId, timeout: TimeSpan.FromSeconds(2));

			// Re-add footer
			App.Tap(FooterButtonId);
			App.WaitForElement(FooterLabelId);
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void HeaderAndFooterCanBeToggledMultipleTimes()
		{
			// First cycle - Header
			App.WaitForElement(HeaderLabelId);
			App.Tap(HeaderButtonId);
			App.WaitForNoElement(HeaderLabelId, timeout: TimeSpan.FromSeconds(2));
			App.Tap(HeaderButtonId);
			App.WaitForElement(HeaderLabelId);

			// Second cycle - Header
			App.Tap(HeaderButtonId);
			App.WaitForNoElement(HeaderLabelId, timeout: TimeSpan.FromSeconds(2));
			App.Tap(HeaderButtonId);
			App.WaitForElement(HeaderLabelId);

			// First cycle - Footer
			App.WaitForElement(FooterLabelId);
			App.Tap(FooterButtonId);
			App.WaitForNoElement(FooterLabelId, timeout: TimeSpan.FromSeconds(2));
			App.Tap(FooterButtonId);
			App.WaitForElement(FooterLabelId);

			// Second cycle - Footer
			App.Tap(FooterButtonId);
			App.WaitForNoElement(FooterLabelId, timeout: TimeSpan.FromSeconds(2));
			App.Tap(FooterButtonId);
			App.WaitForElement(FooterLabelId);
		}
	}
}
