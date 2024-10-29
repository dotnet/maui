using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla47923 : _IssuesUITest
{
	public Bugzilla47923(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "TapGestureRecognizer blocks List View Context Actions1";

	// [FailsOnAndroidWhenRunningOnXamarinUITest]
	// [FailsOnIOSWhenRunningOnXamarinUITest]
	// [Test]
	// public void Bugzilla47923Test()
	// {
	// 	foreach (var testString in new[] { "AspectFit", "AspectFill", "Fill", "Test cell views" })
	// 	{
	// 		App.WaitForElement(q => q.Marked(testString));
	// 		App.Tap(q => q.Marked(testString));
	// 		App.Back();
	// 	}
	// }
}