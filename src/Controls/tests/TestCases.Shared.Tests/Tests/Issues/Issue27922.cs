using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue27922 : _IssuesUITest
{
	public override string Issue => "[WinUI]CollectionView with GroupHeader enabled should scroll properly with groupheader";

	public Issue27922(TestDevice device) : base(device)
	{
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void ItemShouldbeScrolledbasedOnGroupHeader()
	{
		App.WaitForElement("27922GroupedCollection");
		App.WaitForElement("27922ScrollToButton");
		App.Tap("27922ScrollToButton");
		App.WaitForElement("Coffee");
		VerifyScreenshot();
	}
}

