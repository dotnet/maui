using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue28212 : _IssuesUITest
{
	public Issue28212(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Using CollectionView.EmptyView results in an Exception on Windows";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void Issue28212_CollectionView()
	{
		App.WaitForElement("Button");
		App.Click("Button");
		App.WaitForElement("Add");
		App.Click("Add");
		App.WaitForElement("Item 1");
		App.Click("BackButton");
		App.WaitForElement("Button");
		App.Click("Button");
		App.WaitForElement("Item 1");
	}
}