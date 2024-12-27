using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla37462 : _IssuesUITest
{
	public Bugzilla37462(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Using App Compat/App Compat theme breaks Navigation.RemovePage on Android ";

	[Test]
	[Category(UITestCategories.Navigation)]
	public void CanRemoveIntermediatePagesAndPopToFirstPage()
	{
		App.WaitForElement("Go To 2");
		App.WaitForElement("This is a label on page 1");
		App.Tap("Go To 2");

		App.WaitForElement("Go To 3");
		App.Tap("Go To 3");

		App.WaitForElement("Go To 4");
		App.Tap("Go To 4");

		App.WaitForElement("Back to 1");
		App.Tap("Back to 1");

		App.WaitForElement("Go To 2");
		App.WaitForElement("This is a label on page 1");
	}
}
