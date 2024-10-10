using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue2740 : _IssuesUITest
{
	public Issue2740(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "System.NotSupportedException: Unable to activate instance of type Microsoft.Maui.Controls.Platform.Android.PageContainer from native handle";

	// [Test]
	// [Category(UITestCategories.LifeCycle)]
	// public void Issue2740Test()
	// {
	// 	RunningApp.WaitForElement(q => q.Marked("1"));
	// 	RunningApp.Tap(q => q.Marked("1"));
	// 	RunningApp.WaitForElement(q => q.Marked("Switch"));
	// 	RunningApp.Tap(q => q.Marked("Switch"));
	// 	RunningApp.WaitForElement(q => q.Marked("1"));
	// }
}