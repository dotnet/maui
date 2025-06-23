using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla25979 : _IssuesUITest
{
	public Bugzilla25979(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "https://bugzilla.xamarin.com/show_bug.cgi?id=25979";

	[Test]
	[Category(UITestCategories.Navigation)]
	public void Bugzilla25979Test()
	{
		App.WaitForElement("PageOneButtonId");
		App.Tap("PageOneButtonId");
		App.WaitForElementTillPageNavigationSettled("PageTwoButtonId");
		App.Tap("PageTwoButtonId");
		App.WaitForElementTillPageNavigationSettled("PopButton");
		App.Tap("PopButton");
		App.WaitForElement("PopAttempted");
	}
}
