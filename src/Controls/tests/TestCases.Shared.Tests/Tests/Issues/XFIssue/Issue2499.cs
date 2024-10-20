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
	// 		App.WaitForElement("picker");
	// 		App.Tap("picker");
	// 		App.WaitForElement("cat");

	// 		AppResult[] items = App.Query("cat");
	// 		Assert.AreNotEqual(items.Length, 0);
	// 		App.WaitForElement(q => q.Marked("mouse"));
	// 		App.Tap("mouse");
	// #if __IOS__
	// 		System.Threading.Tasks.Task.Delay(500).Wait();
	// 		var cancelButtonText = "Done";
	// 		App.WaitForElement(q => q.Marked(cancelButtonText));
	// 		App.Tap(q => q.Marked(cancelButtonText));
	// 		System.Threading.Tasks.Task.Delay(1000).Wait();
	// #endif
	// 		items = App.Query("cat");
	// 		Assert.AreEqual(items.Length, 0);
	// 	}
}
