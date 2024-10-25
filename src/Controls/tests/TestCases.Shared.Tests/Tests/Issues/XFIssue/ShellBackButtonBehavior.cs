using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.Shell)]
public class ShellBackButtonBehavior : _IssuesUITest
{
	public ShellBackButtonBehavior(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Shell Back Button Behavior Test";

	// TODO: HostApp UI pushes some ControlGallery specific page? Commented out now, fix that first!
	//[Test]
//	[FailsOnAndroid]
//	public void CommandTest()
//	{
//		RunningApp.Tap(ToggleCommandId);
//		RunningApp.EnterText(EntryCommandParameter, "parameter");
//		ShowFlyout();

//		// API 19 workaround
//		var commandResult = RunningApp.QueryUntilPresent(() =>
//		{
//			ShowFlyout();
//			if (RunningApp.WaitForElement(CommandResultId)[0].ReadText() == "parameter")
//				return RunningApp.WaitForElement(CommandResultId);

//			return null;
//		})?.FirstOrDefault()?.ReadText();

//		Assert.AreEqual("parameter", commandResult);
//		RunningApp.EnterText(EntryCommandParameter, "canexecutetest");
//		RunningApp.Tap(ToggleCommandCanExecuteId);

//		commandResult = RunningApp.QueryUntilPresent(() =>
//		{
//			if (RunningApp.WaitForElement(CommandResultId)[0].ReadText() == "parameter")
//				return RunningApp.WaitForElement(CommandResultId);

//			return null;
//		})?.FirstOrDefault()?.ReadText();

//		Assert.AreEqual("parameter", commandResult);
//	}

//	[Test]
//	public void CommandWorksWhenItsTheOnlyThingSet()
//	{
//		RunningApp.Tap(PushPageId);
//		RunningApp.Tap(ToggleCommandId);
//		RunningApp.EnterText(EntryCommandParameter, "parameter");

//		// API 19 workaround
//		var commandResult = RunningApp.QueryUntilPresent(() =>
//		{

//#if __ANDROID__
//			TapBackArrow();
//#else
//			RunningApp.Tap("Page 0");
//#endif

//			if (RunningApp.WaitForElement(CommandResultId)[0].ReadText() == "parameter")
//				return RunningApp.WaitForElement(CommandResultId);

//			return null;
//		})?.FirstOrDefault()?.ReadText();

//		Assert.AreEqual(commandResult, "parameter");
//	}

//	[Test]
//	[FailsOnIOS]
//	public void BackButtonSetToTextStillNavigatesBack()
//	{
//		RunningApp.Tap(PushPageId);
//		RunningApp.Tap(ToggleTextId);
//		RunningApp.Tap("T3xt");
//		RunningApp.WaitForNoElement(FlyoutOpen);
//		RunningApp.WaitForElement("Page 0");
//	}

//	[Test]
//	[FailsOnIOS]
//	public void BackButtonSetToTextStillOpensFlyout()
//	{
//		RunningApp.Tap(ToggleTextId);

//		RunningApp.Tap("T3xt");
//		RunningApp.WaitForElement(FlyoutOpen);
//	}

//#if __ANDROID__
//	[Test]
//	public void FlyoutDisabledDoesntOpenFlyoutWhenSetToText()
//	{
//		RunningApp.WaitForElement("ToggleFlyoutBehavior");
//		RunningApp.Tap("ToggleFlyoutBehavior");
//		RunningApp.Tap("ToggleFlyoutBehavior");
//		RunningApp.WaitForElement("Flyout Behavior: Disabled");
//		RunningApp.Tap(ToggleTextId);
//		RunningApp.Tap("T3xt");
//		RunningApp.WaitForNoElement(FlyoutOpen);
//	}
//#else
//	[Test]
//	[FailsOnIOS]
//	public void FlyoutDisabledDoesntOpenFlyoutWhenSetToText()
//	{
//		RunningApp.WaitForElement("ToggleFlyoutBehavior");
//		RunningApp.Tap(ToggleTextId);
//		RunningApp.WaitForElement("T3xt");
//		RunningApp.Tap("ToggleFlyoutBehavior");
//		RunningApp.WaitForElement("T3xt");
//		RunningApp.Tap("ToggleFlyoutBehavior");
//		RunningApp.WaitForElement("Flyout Behavior: Disabled");
//		RunningApp.Tap("T3xt");
//		RunningApp.WaitForNoElement(FlyoutOpen);
//	}
//#endif
//	[Test]
//	public void AutomationIdOnIconOverride()
//	{
//		RunningApp.WaitForElement("ToggleFlyoutBehavior");
//		RunningApp.Tap(ToggleIconId);
//		RunningApp.WaitForElement("CoffeeAutomation");
//		RunningApp.Tap("ToggleFlyoutBehavior");
//		RunningApp.WaitForElement("CoffeeAutomation");
//		RunningApp.Tap("ToggleFlyoutBehavior");
//		RunningApp.WaitForElement("Flyout Behavior: Disabled");
//		RunningApp.Tap("CoffeeAutomation");
//		RunningApp.WaitForNoElement(FlyoutOpen);
//	}
}