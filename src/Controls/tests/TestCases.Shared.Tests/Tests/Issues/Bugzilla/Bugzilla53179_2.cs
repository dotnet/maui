using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla53179_2 : _IssuesUITest
{
	const string Success = "Success";

	public Bugzilla53179_2(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Removing page during OnAppearing throws exception";

	[Test]
	[Category(UITestCategories.Navigation)]
	public void RemovePageOnAppearingDoesNotCrash()
	{
		App.WaitForElement(Success);
	}
}