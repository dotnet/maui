#if ANDROID // This regression is Android-only: https://github.com/dotnet/maui/pull/29255
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue35806 : _IssuesUITest
{
	public Issue35806(TestDevice device) : base(device) { }

	public override string Issue => "Android CollectionView KeepScrollOffset stops working after replacing ItemsSource";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void KeepScrollOffsetWorksAfterReplacingItemsSource()
	{
		App.WaitForElement("CollectionView35806");

		// Verify initial source loaded
		App.WaitForElement("v1-Item 1");

		// Replace source, scroll to top, insert at top — KeepScrollOffset should keep position
		App.Click("ReplaceSourceButton");
		App.WaitForElement("v2-Item 1");
		App.Click("ScrollToTopButton");
		App.Click("InsertAtTopButton");

		// With KeepScrollOffset at position 0, the inserted item should be visible at the top.
		// Without the fix, "Inserted-31" is hidden above the viewport (broken KeepItemsInView behavior).
		App.WaitForElement("Inserted-31");

		// Replace source again to verify it still works on subsequent replacements
		App.Click("ReplaceSourceButton");
		App.WaitForElement("v3-Item 1");
		App.Click("ScrollToTopButton");
		App.Click("InsertAtTopButton");

		// After second replacement (30 items in new source), inserted item text is "Inserted-31"
		App.WaitForElement("Inserted-31");
	}
}
#endif
