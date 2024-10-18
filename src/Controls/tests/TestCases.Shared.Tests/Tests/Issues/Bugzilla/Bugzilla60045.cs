using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Bugzilla60045 : _IssuesUITest
{
	public Bugzilla60045(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "ListView with RecycleElement strategy doesn't handle CanExecute of TextCell Command properly";

	// [Test]
	// [FailsOnIOS]
	// public void CommandDoesNotFire()
	// {
	// 	App.WaitForElement(ClickThis);
	// 	App.Tap(ClickThis);
	// 	App.WaitForNoElement(Fail);
	// }
}
