using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue2976 : _IssuesUITest
{
	public Issue2976(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Sample 'WorkingWithListviewNative' throw Exception on Xam.Android project.";

	// [Test]
	// [Category(UITestCategories.ListView)]
	// [FailsOnAndroid]
	// [FailsOnIOS]
	// public void Issue1Test()
	// {
	// 	App.Screenshot("I am at Issue 2976");
	// 	App.Tap(q => q.Marked("DEMOA"));
	// 	App.Tap(q => q.Marked("DEMOB"));
	// 	App.Tap(q => q.Marked("DEMOC"));
	// 	App.Tap(q => q.Marked("DEMOD"));
	// }
}