using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue1342 : _IssuesUITest
{

	public Issue1342(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[iOS] ListView throws Exception on ObservableCollection.Add/Remove for non visible list view";

	[Test]
	[Category(UITestCategories.ListView)]
	public void AddingItemsToNonVisibleListViewDoesntCrash()
	{
		App.Tap("add2");
		App.Tap("add3");
		App.WaitForElement("add1");
	}
}
