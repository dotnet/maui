using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue2499 : _IssuesUITest
{
	public Issue2499(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Binding Context set to Null in Picker";

	// [Test]
	// [Category(UITestCategories.Picker)]
	// [FailsOnIOS]
	// 	public void Issue2499Test()
	// 	{
	// 		RunningApp.WaitForElement("picker");
	// 		RunningApp.Tap("picker");
	// 		RunningApp.WaitForElement("cat");

	// 		AppResult[] items = RunningApp.Query("cat");
	// 		Assert.AreNotEqual(items.Length, 0);
	// 		RunningApp.WaitForElement(q => q.Marked("mouse"));
	// 		RunningApp.Tap("mouse");
	// #if __IOS__
	// 		System.Threading.Tasks.Task.Delay(500).Wait();
	// 		var cancelButtonText = "Done";
	// 		RunningApp.WaitForElement(q => q.Marked(cancelButtonText));
	// 		RunningApp.Tap(q => q.Marked(cancelButtonText));
	// 		System.Threading.Tasks.Task.Delay(1000).Wait();
	// #endif
	// 		items = RunningApp.Query("cat");
	// 		Assert.AreEqual(items.Length, 0);
	// 	}
}
