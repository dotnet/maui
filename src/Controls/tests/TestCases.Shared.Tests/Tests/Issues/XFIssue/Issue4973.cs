using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue4973 : _IssuesUITest
{
	public Issue4973(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "TabbedPage nav tests";

	//[Test]
	//[Category(UITestCategories.Navigation)]
	//[FailsOnAndroid]
	//public void Issue4973Test()
	//{
	//	RunningApp.Tap(q => q.Text("Tab5"));

	//	RunningApp.WaitForElement(q => q.Text("Test"));

	//	GarbageCollectionHelper.Collect();

	//	RunningApp.Tap(q => q.Text("Tab1"));

	//	RunningApp.Tap(q => q.Text("Tab2"));
	//}
}