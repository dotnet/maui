using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue7886 : _IssuesUITest
{

	const string TriggerModalAutomationId = "TriggerModal";
	const string PopModalAutomationId = "Done";

	public Issue7886(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "PushModalAsync modal page with Entry crashes on close for MacOS (NRE)";

	[Test]
	[Category(UITestCategories.Navigation)]
	public void NoNREOnPushModalAsyncAndBack()
	{
		App.WaitForElement(TriggerModalAutomationId);
		App.Tap(TriggerModalAutomationId);
		App.WaitForElement(PopModalAutomationId);
		App.Tap(PopModalAutomationId);
		App.WaitForElement(TriggerModalAutomationId);
	}
}