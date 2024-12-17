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
	const string EntryCommandParameter = "EntryCommandParameter";
	const string ToggleBehaviorId = "ToggleBehaviorId";
	const string ToggleCommandId = "ToggleCommandId";
	const string ToggleCommandCanExecuteId = "ToggleCommandCanExecuteId";
	const string ToggleIconId = "ToggleIconId";
	const string ToggleIsEnabledId = "ToggleIsEnabledId";
	const string ToggleTextId = "ToggleTextId";
	const string CommandResultId = "CommandResult";
	const string PushPageId = "PushPageId";
	const string FlyoutOpen = "Flyout Open";

	public override string Issue => "Shell Back Button Behavior Test";

#if !ANDROID && !MACCATALYST && !IOS && !WINDOWS // It returns null value
    [Test]
    public void CommandTest()
    {
        App.WaitForElement(ToggleCommandId);
        App.Tap(ToggleCommandId);
        App.EnterText(EntryCommandParameter, "parameter");
        var commandResult = App.QueryUntilPresent(() =>
        {
             
            var element = App.WaitForElement(CommandResultId);
            if (element != null && element.ReadText() == "parameter")
                return element;
 
            return null;
        });
 
        Assert.That(commandResult?.ReadText(), Is.EqualTo("parameter"));
        App.EnterText(EntryCommandParameter, "canexecutetest");
        App.Tap(ToggleCommandCanExecuteId);
 
        commandResult = App.QueryUntilPresent(() =>
        {
            var element = App.WaitForElement(CommandResultId);
            if (element != null && element.ReadText() == "parameter")
                return element;
 
            return null;
        });
 
        Assert.That(commandResult?.ReadText(), Is.EqualTo("parameter"));
    }
#endif

	[Test]
	public void CommandWorksWhenItsTheOnlyThingSet()
	{
		App.WaitForElement(PushPageId);
		App.Tap(PushPageId);
		App.WaitForElement(ToggleCommandId);
		App.Tap(ToggleCommandId);
		App.EnterText(EntryCommandParameter, "parameter");
		var commandResult = App.QueryUntilPresent(() =>
		{
			TapBackArrow();
			var element = App.WaitForElement(CommandResultId);
			if (element != null && element.ReadText() == "parameter")
				return element;

			return null;
		});

		Assert.That(commandResult?.ReadText(), Is.EqualTo("parameter"));
	}

#if IOS || MACCATALYST // Test fails on Android and  Windows(text not updated in windows ) More Information:https://github.com/dotnet/maui/issues/1625
	[Test]
	public void BackButtonSetToTextStillNavigatesBack()
	{
		App.WaitForElement(PushPageId);
		App.Tap(PushPageId);
		App.WaitForElement(ToggleTextId);
		App.Tap(ToggleTextId);
		App.WaitForElement("T3xt");
		App.Tap("T3xt");
		App.WaitForNoElement(FlyoutOpen);
		App.WaitForElement("Page 0");
	}
#endif


#if !ANDROID && !IOS && !MACCATALYST && !WINDOWS // Text not upated in mac, windows and ios .In Android it show waitforelement for "T3xt"
    [Test]
    public void BackButtonSetToTextStillOpensFlyout()
    {
        App.WaitForElement(ToggleTextId);
        App.Tap(ToggleTextId);
        App.WaitForElement("T3xt");
        App.Tap("T3xt");
        App.WaitForElement(FlyoutOpen);
    }
    [Test]  //this test case only for Android but waitForElement for "T3xt"
    public void FlyoutDisabledDoesntOpenFlyoutWhenSetToText()
    {
        App.WaitForElement("ToggleFlyoutBehavior");
        App.Tap("ToggleFlyoutBehavior");
        App.Tap("ToggleFlyoutBehavior");
        App.WaitForElement("Flyout Behavior: Disabled");
        App.Tap(ToggleTextId);
        App.WaitForElement("T3xt");
        App.Tap("T3xt");
        App.WaitForNoElement(FlyoutOpen);
    }
    [Test] // Text not updated in MacCatalyst, IOS , Windows
     
    public void FlyoutDisabledDoesntOpenFlyoutWhenSetToText()
    {
        App.WaitForElement("ToggleFlyoutBehavior");
        App.Tap(ToggleTextId);
        App.WaitForElement("T3xt");
        App.Tap("ToggleFlyoutBehavior");
        App.WaitForElement("T3xt");
        App.Tap("ToggleFlyoutBehavior");
        App.WaitForElement("Flyout Behavior: Disabled");
        App.Tap("T3xt");
        App.WaitForNoElement(FlyoutOpen);
        App.WaitForElement("ToggleFlyoutBehaviour");
        App.Tap("ToggleFlyoutBehaviour");
    }
#endif

#if IOS || MACCATALYST  //   Icon not updated in Windows
	[Test]
	public void AutomationIdOnIconOverride()
	{
		App.WaitForElement("ToggleFlyoutBehavior");
		App.Tap(ToggleIconId);
		App.WaitForElement("CoffeeAutomation");
		App.Tap("ToggleFlyoutBehavior");
		App.WaitForElement("CoffeeAutomation");
		App.Tap("ToggleFlyoutBehavior");
		App.WaitForElement("Flyout Behavior: Disabled");
		App.Tap("CoffeeAutomation");
		App.WaitForNoElement(FlyoutOpen);
	}
#endif
	void TapBackArrow()
	{
#if WINDOWS || ANDROID
        App.TapBackArrow();
#else
		App.TapBackArrow("Page 0");
#endif
	}
    
}

 
 
 
 
 