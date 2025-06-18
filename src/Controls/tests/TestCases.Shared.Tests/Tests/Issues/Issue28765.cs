using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue28765 : _IssuesUITest
{
	public override string Issue => "[Android] Inconsistent footer scrolling in CollectionView when EmptyView as string";
	public Issue28765(TestDevice device) : base(device)
	{
	}

	[Test, Order(1)]
	[Category(UITestCategories.CollectionView)]
	public void EmptyViewStringWithHeaderAndFooterAsView()
	{
		App.WaitForElement("Footer View");
	}

	[Test, Order(2)]
	[Category(UITestCategories.CollectionView)]
	public void EmptyViewStringWithHeaderAndFooterString()
	{
		App.WaitForElement("Footer String");
	}
}