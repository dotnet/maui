#if TEST_FAILS_ON_WINDOWS // This test fails on Windows because we no longer works with CollectionView on Windows.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34897 : _IssuesUITest
{
	public Issue34897(TestDevice device) : base(device) { }

	public override string Issue => "CollectionView Header is not visible when ItemsSource is not set and EmptyView is set";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void CollectionViewHeaderVisibleWithEmptyViewAndNullItemsSource()
	{
		App.WaitForElement("Issue34897EmptyView");
		App.WaitForElement("Issue34897Header");
		App.WaitForElement("Issue34897Footer");
	}
}
#endif