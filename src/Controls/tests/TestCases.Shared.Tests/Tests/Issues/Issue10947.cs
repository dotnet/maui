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
		var headerLocation = App.WaitForElement(HeaderEntry).GetRect();
		var footerLocation = App.WaitForElement(FooterEntry).GetRect();

		App.Tap(HeaderEntry);

		var newHeaderLocation = App.WaitForElement(HeaderEntry).GetRect();
		ClassicAssert.AreEqual(headerLocation, newHeaderLocation);

		App.Tap(FooterEntry);

		var newFooterLocation = App.WaitForElement(FooterEntry).GetRect();

		ClassicAssert.AreEqual(footerLocation, newFooterLocation);
	}
}
