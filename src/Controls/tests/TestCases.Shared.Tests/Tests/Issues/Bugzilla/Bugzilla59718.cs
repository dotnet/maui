using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla59718 : _IssuesUITest
{
	public Bugzilla59718(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Multiple issues with listview and navigation in UWP";

	// [Test]
	// [Category(UITestCategories.ListView)]
	// [FailsOnIOS]
	// public void Bugzilla59718Test()
	// {
	// 	RunningApp.WaitForElement(q => q.Marked(Target1));
	// 	RunningApp.Tap(q => q.Marked(Target1));

	// 	RunningApp.WaitForElement(q => q.Marked(Target1b));

	// 	RunningApp.WaitForElement(q => q.Marked(Target2));
	// 	RunningApp.Tap(q => q.Marked(Target2));

	// 	RunningApp.WaitForElement(q => q.Marked(Target3));

	// 	RunningApp.WaitForElement(q => q.Marked(GoBackButtonId));
	// 	RunningApp.Tap(q => q.Marked(GoBackButtonId));

	// 	RunningApp.WaitForElement(q => q.Marked(Target1));
	// }
}