#if !ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue3008 : _IssuesUITest
{

	public Issue3008(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Setting ListView.ItemSource to null doesn't cause it clear out its contents";

	// [Test]
	// [Category(UITestCategories.ListView)]
	// [FailsOnIOS]
	// public void EnsureListViewEmptiesOut()
	// {
	// 	App.Tap("Click Until Success");
	// 	App.WaitForElement("Not Grouped Item");
	// 	App.WaitForElement("Grouped Item");

	// 	App.Tap("Click Until Success");
	// 	App.WaitForElement("Not Grouped Item");
	// 	App.WaitForElement("Grouped Item");

	// 	App.Tap("Click Until Success");
	// 	App.WaitForNoElement("Not Grouped Item");
	// 	App.WaitForNoElement("Grouped Item");

	// 	App.Tap("Click Until Success");
	// 	App.WaitForElement("Not Grouped Item");
	// 	App.WaitForElement("Grouped Item");

	// 	App.Tap("Click Until Success");
	// 	App.WaitForNoElement("Not Grouped Item");
	// 	App.WaitForNoElement("Grouped Item");

	// 	App.Tap("Click Until Success");
	// 	App.WaitForElement("Not Grouped Item");
	// 	App.WaitForElement("Grouped Item");

	// 	App.Tap("Click Until Success");
	// 	App.WaitForNoElement("Not Grouped Item");
	// 	App.WaitForNoElement("Grouped Item");
	// }
}
#endif