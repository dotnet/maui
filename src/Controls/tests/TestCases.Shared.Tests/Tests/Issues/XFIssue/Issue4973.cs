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
	//[FailsOnAndroidWhenRunningOnXamarinUITest]
	//public void Issue4973Test()
	//{
	//	App.Tap(q => q.Text("Tab5"));

	//	App.WaitForElement(q => q.Text("Test"));

	//	GarbageCollectionHelper.Collect();

	//	App.Tap(q => q.Text("Tab1"));

	//	App.Tap(q => q.Text("Tab2"));
	//}
}