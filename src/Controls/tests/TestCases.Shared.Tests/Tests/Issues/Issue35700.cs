using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue35700 : _IssuesUITest
{
	public Issue35700(TestDevice device) : base(device) { }

	public override string Issue => "Grouped CollectionView items not rendered properly on Android with GridItemsLayout";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void GroupedCollectionViewGridLayoutRendersCorrectly()
	{
		App.WaitForElement("TestCollectionView");
		VerifyScreenshot();
	}
}
