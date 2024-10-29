using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue9440 : _IssuesUITest
{
	public Issue9440(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Flyout closes with two or more taps";

	//[Test]
	//[Category(UITestCategories.Shell)]
	//public void GitHubIssue9440()
	//{
	//	DoubleTapInFlyout(Test1);
	//	App.WaitForElement(q => q.Marked(Test1));
	//	Assert.AreEqual(false, FlyoutIsPresented);
	//}
}