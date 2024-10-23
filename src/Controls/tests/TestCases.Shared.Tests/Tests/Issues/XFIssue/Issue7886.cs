using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue7886 : _IssuesUITest
{
	public Issue7886(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "PushModalAsync modal page with Entry crashes on close for MacOS (NRE)";

	//[Test]
	//[Category(UITestCategories.Navigation)]
	//public void NoNREOnPushModalAsyncAndBack()
	//{
	//	RunningApp.WaitForElement(TriggerModalAutomationId);
	//	RunningApp.Tap(TriggerModalAutomationId);
	//	RunningApp.WaitForElement(PopModalAutomationId);
	//	RunningApp.Tap(PopModalAutomationId);
	//}
}