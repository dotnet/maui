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
	// 	RunningApp.WaitForElement(q => q.Marked("New Page"));

	// 	RunningApp.Tap(q => q.Marked("New Page"));
	// 	RunningApp.WaitForElement(q => q.Marked("Second Page #1"));
	// 	RunningApp.Back();
	// 	RunningApp.WaitForElement(q => q.Marked("Intermediate Page"));
	// 	RunningApp.Back();
	// 	RunningApp.Tap(q => q.Marked("Do GC"));
	// 	RunningApp.Tap(q => q.Marked("Do GC"));
	// 	RunningApp.Tap(q => q.Marked("Send Message"));
	// 	RunningApp.Tap(q => q.Marked("Do GC"));

	// 	RunningApp.WaitForElement(q => q.Marked("Instances: 0"));
	// 	RunningApp.WaitForElement(q => q.Marked("Messages: 0"));
	// }
}