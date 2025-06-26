﻿using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Trait("Category", UITestCategories.Shell)]
public class ShellBackButtonBehavior : _IssuesUITest
{
	public ShellBackButtonBehavior(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Shell Back Button Behavior Test";

	// TODO: HostApp UI pushes some ControlGallery specific page? Commented out now, fix that first!
	//[Fact]
	//	[FailsOnAndroid]
	//	public void CommandTest()
	//	{
	//		App.Tap(ToggleCommandId);
	//		App.EnterText(EntryCommandParameter, "parameter");
	//		ShowFlyout();

	//		// API 19 workaround
	//		var commandResult = App.QueryUntilPresent(() =>
	//		{
	//			ShowFlyout();
	//			if (App.WaitForElement(CommandResultId)[0].ReadText() == "parameter")
	//				return App.WaitForElement(CommandResultId);

	//			return null;
	//		})?.FirstOrDefault()?.ReadText();

	//		Assert.Equal("parameter", commandResult);
	//		App.EnterText(EntryCommandParameter, "canexecutetest");
	//		App.Tap(ToggleCommandCanExecuteId);

	//		commandResult = App.QueryUntilPresent(() =>
	//		{
	//			if (App.WaitForElement(CommandResultId)[0].ReadText() == "parameter")
	//				return App.WaitForElement(CommandResultId);

	//			return null;
	//		})?.FirstOrDefault()?.ReadText();

	//		Assert.Equal("parameter", commandResult);
	//	}

	//	[Fact]
	//	public void CommandWorksWhenItsTheOnlyThingSet()
	//	{
	//		App.Tap(PushPageId);
	//		App.Tap(ToggleCommandId);
	//		App.EnterText(EntryCommandParameter, "parameter");

	//		// API 19 workaround
	//		var commandResult = App.QueryUntilPresent(() =>
	//		{

	//#if __ANDROID__
	//			TapBackArrow();
	//#else
	//			App.Tap("Page 0");
	//#endif

	//			if (App.WaitForElement(CommandResultId)[0].ReadText() == "parameter")
	//				return App.WaitForElement(CommandResultId);

	//			return null;
	//		})?.FirstOrDefault()?.ReadText();

	//		Assert.Equal(commandResult, "parameter");
	//	}

	//	[Fact]
	//	[FailsOnIOSWhenRunningOnXamarinUITest]
	//	public void BackButtonSetToTextStillNavigatesBack()
	//	{
	//		App.Tap(PushPageId);
	//		App.Tap(ToggleTextId);
	//		App.Tap("T3xt");
	//		App.WaitForNoElement(FlyoutOpen);
	//		App.WaitForElement("Page 0");
	//	}

	//	[Fact]
	//	[FailsOnIOSWhenRunningOnXamarinUITest]
	//	public void BackButtonSetToTextStillOpensFlyout()
	//	{
	//		App.Tap(ToggleTextId);

	//		App.Tap("T3xt");
	//		App.WaitForElement(FlyoutOpen);
	//	}

	//#if __ANDROID__
	//	[Fact]
	//	public void FlyoutDisabledDoesntOpenFlyoutWhenSetToText()
	//	{
	//		App.WaitForElement("ToggleFlyoutBehavior");
	//		App.Tap("ToggleFlyoutBehavior");
	//		App.Tap("ToggleFlyoutBehavior");
	//		App.WaitForElement("Flyout Behavior: Disabled");
	//		App.Tap(ToggleTextId);
	//		App.Tap("T3xt");
	//		App.WaitForNoElement(FlyoutOpen);
	//	}
	//#else
	//	[Fact]
	//	[FailsOnIOSWhenRunningOnXamarinUITest]
	//	public void FlyoutDisabledDoesntOpenFlyoutWhenSetToText()
	//	{
	//		App.WaitForElement("ToggleFlyoutBehavior");
	//		App.Tap(ToggleTextId);
	//		App.WaitForElement("T3xt");
	//		App.Tap("ToggleFlyoutBehavior");
	//		App.WaitForElement("T3xt");
	//		App.Tap("ToggleFlyoutBehavior");
	//		App.WaitForElement("Flyout Behavior: Disabled");
	//		App.Tap("T3xt");
	//		App.WaitForNoElement(FlyoutOpen);
	//	}
	//#endif
	//	[Fact]
	//	public void AutomationIdOnIconOverride()
	//	{
	//		App.WaitForElement("ToggleFlyoutBehavior");
	//		App.Tap(ToggleIconId);
	//		App.WaitForElement("CoffeeAutomation");
	//		App.Tap("ToggleFlyoutBehavior");
	//		App.WaitForElement("CoffeeAutomation");
	//		App.Tap("ToggleFlyoutBehavior");
	//		App.WaitForElement("Flyout Behavior: Disabled");
	//		App.Tap("CoffeeAutomation");
	//		App.WaitForNoElement(FlyoutOpen);
	//	}
}