using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla45926 : _IssuesUITest
{
	public Bugzilla45926(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Effect not attaching to ScrollView";

	// TODO Xamarin.UITest Migration
	// [Test]
	// [FailsOnAndroid]
	// public void Issue45926Test()
	// {
	// 	App.WaitForElement(q => q.Marked("New Page"));

	// 	App.Tap(q => q.Marked("New Page"));
	// 	App.WaitForElement(q => q.Marked("Second Page #1"));
	// 	App.Back();
	// 	App.WaitForElement(q => q.Marked("Intermediate Page"));
	// 	App.Back();
	// 	App.Tap(q => q.Marked("Do GC"));
	// 	App.Tap(q => q.Marked("Do GC"));
	// 	App.Tap(q => q.Marked("Send Message"));
	// 	App.Tap(q => q.Marked("Do GC"));

	// 	App.WaitForElement(q => q.Marked("Instances: 0"));
	// 	App.WaitForElement(q => q.Marked("Messages: 0"));
	// }
}