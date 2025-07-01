using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue28678 : _IssuesUITest
{
	public Issue28678(TestDevice device) : base(device) { }

	public override string Issue => "TargetInvocationException Occurs When Selecting Header/Footer After Changing ItemsLayout in CV2";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void HeaderFooterSelectionAfterItemsLayoutChangeShouldNotCrash()
	{
		App.WaitForElement("Button");
		App.Tap("Button");
		App.Tap("HeaderButton");
		App.Tap("FooterButton");
		App.WaitForElement("Button");
	}
}