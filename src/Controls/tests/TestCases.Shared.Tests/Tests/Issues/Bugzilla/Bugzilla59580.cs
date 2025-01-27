#if WINDOWs || ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla59580 : _IssuesUITest
{
	public Bugzilla59580(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Raising Command.CanExecutChanged causes crash on Android";

	[Test]
	[Category(UITestCategories.TableView)]
	public void RaisingCommandCanExecuteChangedCausesCrashOnAndroid()
	{
		App.WaitForElement("Cell");

		App.ContextActions("Cell");

		App.WaitForElement("Fire CanExecuteChanged");
		App.Tap("Fire CanExecuteChanged");
		App.WaitForElement("Cell");
	}
}
#endif