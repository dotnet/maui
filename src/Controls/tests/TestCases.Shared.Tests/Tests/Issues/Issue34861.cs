#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_CATALYST // Test fails on iOS, Windows and Catalyst because of Header is not visible https://github.com/dotnet/maui/issues/34897
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34861 : _IssuesUITest
{
	public Issue34861(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Android] CollectionView EmptyView not displayed correctly with GridItemsLayout span > 1";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void EmptyViewShouldAppearBelowHeaderInGridLayout()
	{
		App.WaitForElement("TestCollectionView");

		var headerRect = App.WaitForElement("HeaderLabel").GetRect();
		var emptyViewRect = App.WaitForElement("EmptyViewLabel").GetRect();

		// EmptyView should be below the header (its Y should be >= header's bottom edge)
		Assert.That(emptyViewRect.Y, Is.GreaterThanOrEqualTo(headerRect.Y + headerRect.Height),
			"EmptyView should appear below the Header, not beside it");

		// EmptyView should start at (or near) the left edge, not pushed to the right
		Assert.That(emptyViewRect.X, Is.LessThanOrEqualTo(headerRect.X + 10),
			"EmptyView should be aligned to the left, not pushed to the right of the Header");
	}
}
#endif