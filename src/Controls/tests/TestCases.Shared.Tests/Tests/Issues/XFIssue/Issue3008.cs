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
	// 	RunningApp.Tap("Click Until Success");
	// 	RunningApp.WaitForElement("Not Grouped Item");
	// 	RunningApp.WaitForElement("Grouped Item");

	// 	RunningApp.Tap("Click Until Success");
	// 	RunningApp.WaitForElement("Not Grouped Item");
	// 	RunningApp.WaitForElement("Grouped Item");

	// 	RunningApp.Tap("Click Until Success");
	// 	RunningApp.WaitForNoElement("Not Grouped Item");
	// 	RunningApp.WaitForNoElement("Grouped Item");

	// 	RunningApp.Tap("Click Until Success");
	// 	RunningApp.WaitForElement("Not Grouped Item");
	// 	RunningApp.WaitForElement("Grouped Item");

	// 	RunningApp.Tap("Click Until Success");
	// 	RunningApp.WaitForNoElement("Not Grouped Item");
	// 	RunningApp.WaitForNoElement("Grouped Item");

	// 	RunningApp.Tap("Click Until Success");
	// 	RunningApp.WaitForElement("Not Grouped Item");
	// 	RunningApp.WaitForElement("Grouped Item");

	// 	RunningApp.Tap("Click Until Success");
	// 	RunningApp.WaitForNoElement("Not Grouped Item");
	// 	RunningApp.WaitForNoElement("Grouped Item");
	// }
}
#endif