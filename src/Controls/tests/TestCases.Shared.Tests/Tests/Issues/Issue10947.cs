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
	[Category(UITestCategories.CollectionView)]
	public void CollectionViewHeaderShouldNotScroll()
	{
		var headerEntry = App.WaitForElement(HeaderEntry);
		var headerLocation = headerEntry.GetRect();
		var footerEntry = App.WaitForElement(FooterEntry);
		var footerLocation = headerEntry.GetRect();

		App.Tap(HeaderEntry);

		var newHeaderEntry = App.WaitForElement(HeaderEntry);
		var newHeaderLocation = headerEntry.GetRect();
		ClassicAssert.AreEqual(headerLocation, newHeaderLocation);

		App.Tap(FooterEntry);

		var newFooterEntry = App.WaitForElement(FooterEntry);
		var newFooterLocation = headerEntry.GetRect();

		ClassicAssert.AreEqual(footerLocation, newFooterLocation);
	}
}
