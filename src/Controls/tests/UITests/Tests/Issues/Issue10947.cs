using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues;

public class Issue10947 : _IssuesUITest
{
	public Issue10947(TestDevice device)
		: base(device)
	{ }

	public override string Issue => "CollectionView Header and Footer Scrolling";
	string HeaderEntry => "HeaderEntry";
	string FooterEntry => "FooterEntry";

	[Test]
	public void CollectionViewHeaderShouldNotScroll()
	{
		var headerEntry = App.WaitForElement(HeaderEntry);
		var headerLocation = headerEntry.GetRect();
		var footerEntry = App.WaitForElement(FooterEntry);
		var footerLocation = headerEntry.GetRect();

		App.Click(HeaderEntry);

		var newHeaderEntry = App.WaitForElement(HeaderEntry);
		var newHeaderLocation = headerEntry.GetRect();
		Assert.AreEqual(headerLocation, newHeaderLocation);

		App.Click(FooterEntry);

		var newFooterEntry = App.WaitForElement(FooterEntry);
		var newFooterLocation = headerEntry.GetRect();

		Assert.AreEqual(footerLocation, newFooterLocation);
	}
}
