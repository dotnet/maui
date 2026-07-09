using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue35113 : _IssuesUITest
{
	public override string Issue => "CV2 header/footer view width is not expanded to its content width on iOS/macOS";

	public Issue35113(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void HorizontalGridHeaderExpandsToContentWidth()
	{
		// With the bug: the header's supplementary cell is constrained to ~30pt wide
		// (LayoutFactory2 estimated value) because ScrollDirection is never set on
		// TemplatedCell2. The header Label "This Is A Header" at FontSize=24 wraps
		// into a narrow column — GetRect().Width should be near 30pt.
		// With the fix: ScrollDirection is set correctly, the header self-sizes to
		// content width — GetRect().Width should be well over 100pt.
		App.WaitForElement("Issue35113Header");
		var rect = App.FindElement("Issue35113Header").GetRect();
		Assert.That(rect.Width, Is.GreaterThan(100),
			$"Header width {rect.Width} should be > 100 when fully expanded. " +
			"If ~30, ScrollDirection was not set on the supplementary TemplatedCell2.");
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void HorizontalGridFooterExpandsToContentWidth()
	{
		App.WaitForElement("Issue35113CollectionView");
		App.ScrollTo("Issue35113Footer");
		App.WaitForElement("Issue35113Footer");
		var rect = App.FindElement("Issue35113Footer").GetRect();
		Assert.That(rect.Width, Is.GreaterThan(100),
			$"Footer width {rect.Width} should be > 100 when fully expanded. " +
			"If ~30, ScrollDirection was not set on the supplementary TemplatedCell2.");
	}
}
