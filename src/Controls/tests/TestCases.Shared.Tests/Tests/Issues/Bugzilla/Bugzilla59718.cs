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
	// 	App.WaitForElement(q => q.Marked(Target1));
	// 	App.Tap(q => q.Marked(Target1));

	// 	App.WaitForElement(q => q.Marked(Target1b));

	// 	App.WaitForElement(q => q.Marked(Target2));
	// 	App.Tap(q => q.Marked(Target2));

	// 	App.WaitForElement(q => q.Marked(Target3));

	// 	App.WaitForElement(q => q.Marked(GoBackButtonId));
	// 	App.Tap(q => q.Marked(GoBackButtonId));

	// 	App.WaitForElement(q => q.Marked(Target1));
	// }
}