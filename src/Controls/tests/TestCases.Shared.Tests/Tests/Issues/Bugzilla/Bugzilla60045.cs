using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla60045 : _IssuesUITest
{
	public const string ClickThis = "Click This";
	public const string Fail = "Fail";

	public Bugzilla60045(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "ListView with RecycleElement strategy doesn't handle CanExecute of TextCell Command properly";

	[Test]
	[Category(UITestCategories.ListView)]
	public void CommandDoesNotFire()
	{
		App.WaitForElement(ClickThis);
		App.Tap(ClickThis);
		App.WaitForNoElement(Fail);
	}
}