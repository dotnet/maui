using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue31465 : _IssuesUITest
{
	public Issue31465(TestDevice device) : base(device) { }

	public override string Issue => "The page can be dragged down, and it would cause an extra space between Header and EmptyView text.";
	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyCollectionViewEmptyView()
	{
		App.WaitForElement("HeaderLabel");
		App.Click("CollectionViewHeader");
		VerifyScreenshot();
	}
}