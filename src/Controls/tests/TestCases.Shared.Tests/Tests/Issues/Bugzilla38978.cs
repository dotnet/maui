using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla38978 : _IssuesUITest
{
	public Bugzilla38978(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Cell.ForceUpdateSize issues with row selection/deselection (ViewCell)";

	[Test]
	[FailsOnIOS]
	[Category(UITestCategories.ManualReview)]
	public void Bugzilla38978Test ()
	{
		App.WaitForElement("2");
		App.Tap("2");
		App.Screenshot("If the tapped image is not larger, this test has failed.");
	}
}
