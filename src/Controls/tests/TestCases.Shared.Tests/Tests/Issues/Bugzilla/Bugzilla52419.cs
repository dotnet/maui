#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla52419 : _IssuesUITest
{
	public Bugzilla52419(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[A] OnAppearing called for previous pages in a tab's navigation when switching active tabs";

	// [Test]
	// [Category(UITestCategories.TabbedPage)]
	// [FailsOnAndroidWhenRunningOnXamarinUITest]
	// public void Bugzilla52419Test()
	// {
	// 	App.WaitForElement(q => q.Marked("Push new page"));
	// 	App.Tap(q => q.Marked("Push new page"));
	// 	App.WaitForElement(q => q.Marked("Push new page"));
	// 	App.Tap(q => q.Marked("Push new page"));
	// 	App.WaitForElement(q => q.Marked("Push new page"));
	// 	App.Tap(q => q.Marked("Push new page"));
	// 	App.Tap(q => q.Marked("Tab Page 2"));
	// 	App.Tap(q => q.Marked("Tab Page 1"));
	// 	App.Tap(q => q.Marked("Tab Page 2"));
	// 	App.Tap(q => q.Marked("Tab Page 1"));
	// 	App.Back();
	// 	App.WaitForElement(q => q.Marked("AppearanceLabel"));
	// 	var label = App.Query(q => q.Marked("AppearanceLabel"))[0];
	// 	Assert.AreEqual("Times Appeared: 2", label.Text);
	// }
}
#endif