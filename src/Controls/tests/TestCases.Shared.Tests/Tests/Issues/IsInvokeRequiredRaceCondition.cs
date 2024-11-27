using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class IsInvokeRequiredRaceCondition : _IssuesUITest
{
	public IsInvokeRequiredRaceCondition(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Application.Current.Dispatcher.IsDispatchRequired race condition causes crash";

	[Test]
	[Category(UITestCategories.Dispatcher)]
	public void ApplicationDispatcherIsInvokeRequiredRaceConditionCausesCrash()
	{
		App.WaitForElement("crashButton");
		App.Tap("crashButton");
		App.WaitForElement("successLabel");
	}
}