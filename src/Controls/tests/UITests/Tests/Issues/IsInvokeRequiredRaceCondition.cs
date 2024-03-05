using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues;

public class IsInvokeRequiredRaceCondition : _IssuesUITest
{
	public IsInvokeRequiredRaceCondition(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Application.Current.Dispatcher.IsDispatchRequired race condition causes crash";

	[Test]
	public void ApplicationDispatcherIsInvokeRequiredRaceConditionCausesCrash()
	{
		App.WaitForElement("crashButton");
		App.Click("crashButton");
		App.WaitForElement("successLabel");
	}
}