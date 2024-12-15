#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_IOS // FindsElements method not works
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class StackLayoutIssue : _IssuesUITest
{
	public StackLayoutIssue(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "StackLayout issue";

	[Test]
	[Category(UITestCategories.Layout)]
	public void StackLayoutIssueTestsAllElementsPresent()
	{
		// TODO: Fix ME
		var images = App.FindElements("Image").Count;
		Assert.That (images, Is.EqualTo(2));

		App.WaitForElement ("Prize");
		App.WaitForElement ("FullName");
		App.WaitForElement ("Email");
		App.WaitForElement ("Company");
		App.WaitForElement ("Challenge");

		var switches = App.FindElements("Switch").Count;
		Assert.That (switches, Is.EqualTo(1));

		App.WaitForElement ("Spin");
		//Assert.Inconclusive("Fix Test");
	}
}
#endif