using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30483 : _IssuesUITest
{
	public Issue30483(TestDevice device) : base(device) { }

	public override string Issue => "Flyout Menu CollectionView First Item Misaligned";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void FlyoutMenuCollectionViewFirstItemMisaligned()
	{
		App.WaitForElement("CollectionView");
		App.ScrollDown("CollectionView");
		VerifyScreenshot();
	}
}