using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue10947 : _IssuesUITest
{
	public Issue10947(TestDevice device)
		: base(device)
	{ }

	public override string Issue => "CollectionView Header and Footer Scrolling";
	string HeaderEntry => "HeaderEntry";
	string FooterEntry => "FooterEntry";

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 5)]
	public void CollectionViewHeaderShouldNotScroll()
	{
		var headerLocation = App.WaitForElementAndGetRect(HeaderEntry);
		var footerLocation = App.WaitForElementAndGetRect(FooterEntry);

		App.Tap(HeaderEntry);

		var newHeaderLocation = App.WaitForElementAndGetRect(HeaderEntry);
		ClassicAssert.AreEqual(headerLocation, newHeaderLocation);

		App.Tap(FooterEntry);

		var newFooterLocation = App.WaitForElementAndGetRect(FooterEntry);

		ClassicAssert.AreEqual(footerLocation, newFooterLocation);
	}
}
