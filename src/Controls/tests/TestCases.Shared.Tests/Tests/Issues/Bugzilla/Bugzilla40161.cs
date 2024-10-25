using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla40161 : _IssuesUITest
{
	public Bugzilla40161(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Issue Bugzilla40161";

	// [Test]
	// [Category(UITestCategories.Layout)]
	// [FailsOnIOSWhenRunningOnXamarinUITest]
	// public void Issue1Test()
	// {
	// 	App.Screenshot("I am at Issue 40161");
	// 	App.WaitForElement("REFRESH");
	// 	App.Screenshot("I see the first image");

	// 	App.Tap("SWAP");
	// 	App.Tap("REFRESH");

	// 	App.WaitForTextToBePresentInElement("counter", "step=0");

	// 	App.Screenshot("I swap the image");

	// 	App.WaitForTextToBePresentInElement("width", "w=50");
	// }
}