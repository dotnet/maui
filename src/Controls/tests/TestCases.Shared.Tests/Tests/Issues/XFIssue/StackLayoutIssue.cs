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

	//	[Test]
	//[Category(UITestCategories.Layout)]
	//public void StackLayoutIssueTestsAllElementsPresent()
	//{
	//	// TODO: Fix ME

	//	//var images = App.Query (PlatformQueries.Images);
	//	//Assert.AreEqual (2, images.Length);

	//	//App.WaitForElement (q => q.Marked ("Win a Xamarin Prize"));
	//	//App.WaitForElement (PlatformQueries.EntryWithPlaceholder ("Full Name"));
	//	//App.WaitForElement (PlatformQueries.EntryWithPlaceholder ("Email"));
	//	//App.WaitForElement (PlatformQueries.EntryWithPlaceholder ("Company"));
	//	//App.WaitForElement (q => q.Marked ("Completed Azure Mobile Services Challenge?"));

	//	//var switches = App.Query (q => q.Raw ("Switch"));
	//	//Assert.AreEqual (1, switches.Length);

	//	//App.WaitForElement (q => q.Button ("Spin"));
	//	//App.Screenshot ("All elements present");

	//	Assert.Inconclusive("Fix Test");
	//}
}