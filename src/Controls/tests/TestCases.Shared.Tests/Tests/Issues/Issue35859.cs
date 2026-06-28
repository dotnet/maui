#if TEST_FAILS_ON_WINDOWS || TEST_FAILS_ON_ANDROID // This test is specific to iOS/macOS CollectionView handler behavior.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue35859 : _IssuesUITest
{
	public Issue35859(TestDevice device)
		: base(device)
	{
	}

	public override string Issue => "CollectionView2 on iOS measures non-first cells despite ItemSizingStrategy.MeasureFirstItem";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void CollectionView2ShouldNotMeasureNonFirstItemsWithCachedFirstItemHeight()
	{
		App.WaitForElement("35859ResetButton");
		App.Tap("35859ResetButton");

		App.WaitForElement("35859ScrollTo40Button");
		App.Tap("35859ScrollTo40Button");

		var summary = App.WaitForElement("35859Summary").GetText();
		Assert.That(summary, Does.Contain("Items2 CV2: 0 cached-height non-first"));

	}
}
#endif
