using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.Focus)]
public class Issue19752(TestDevice device) : _IssuesUITest(device)
{
	public override string Issue => "Button does not behave properly when pointer hovers over the button because it's in focused state.";

	protected override bool ResetAfterEachTest => true;

	[Test]
	public void InitialStateAreAllCorrect()
	{
		Assert.That(App.FindElement("button1").GetText(), Is.EqualTo("Normal"));
		Assert.That(App.FindElement("button2").GetText(), Is.EqualTo("Disabled"));
		Assert.That(App.FindElement("button3").GetText(), Is.EqualTo("Normal"));
	}

	[Test]
	public void HoveringOverButtonMovesToPointerOverState()
	{
		App.MoveCursor("button1");

		// when the mouse moves over a button, it gets a state
		Assert.That(App.FindElement("button1").GetText(), Is.EqualTo("PointerOver"));
	}

	// TODO: find a way to send actions to appium and then read values simultaneously
	//	[Test]
	//	public void PressingButtonMovesToPressedState()
	//	{
	//		Task.Run(() =>
	//		{
	//#if MACCATALYST
	//			App.LongPress("button1");
	//#else
	//			App.TouchAndHold("button1");
	//#endif
	//		});

	//		// pressing and holding the mouse is pressed
	//		App.WaitForTextToBePresentInElement("button1", "Pressed");
	//		Assert.That(App.FindElement("button1").GetText(), Is.EqualTo("Pressed"));
	//	}

	[Test]
	public void PressingAndReleasingButtonMovesToPointerOverState()
	{
		var rectBefore = App.FindElement("button1").GetRect();

		App.Tap("button1");

		// pressing a button sets it to be focused, but the pointer over state is appplied after
		Assert.That(App.FindElement("button1").GetText(), Is.EqualTo("PointerOver"));

		// we are shrinking the focused button a bit
		var rectAfter = App.FindElement("button1").GetRect();
		Assert.That(rectBefore, Is.Not.EqualTo(rectAfter));
	}

	[Test]
	public void HoveringOverButtonAndThenMovingOffMovesToNormalState()
	{
		var rectBefore = App.FindElement("button1").GetRect();

		App.MoveCursor("button1");
		App.MoveCursor("button2");

		// hovering over a button and then moving off goes back to the normal state
		// and does not affect focus
		Assert.That(App.FindElement("button1").GetText(), Is.EqualTo("Normal"));

		// we are shrinking the focused button a bit, but the button is still not focused
		var rectAfter = App.FindElement("button1").GetRect();
		Assert.That(rectBefore, Is.EqualTo(rectAfter));
	}

	[Test]
	public void EnablingButtonMovesToNormalState()
	{
		App.Tap("button1");

		// enabling a button just switches to the normal state
		Assert.That(App.FindElement("button2").GetText(), Is.EqualTo("Normal"));
	}

	[Test]
	public void DisablingUnfocusedButtonMovesToDisabledState()
	{
		var rectBefore = App.FindElement("button2").GetRect();

		App.Tap("button1"); // focus button 1
		App.Tap("button2"); // move the focus to button 2, but then disable it

		// the button is disabled without a focus chnage as it never had focus
		Assert.That(App.FindElement("button2").GetText(), Is.EqualTo("Disabled"));

		// we are shrinking the focused button a bit, but the button never had focus
		var rectAfter = App.FindElement("button2").GetRect();
		Assert.That(rectBefore, Is.EqualTo(rectAfter));

		// this forces focus to button 3 which is set on top of the normal state
		Assert.That(App.FindElement("button3").GetText(), Is.EqualTo("Focused"));
	}

	[Test]
	public void DisablingFocusedButtonMovesToDisabledState()
	{
		var rectBefore = App.FindElement("button3").GetRect();

		App.Tap("button1"); // focus button 1
		App.Tap("button2"); // move the focus to button 2, but then disable it forcing focus to button 3
		App.Tap("button3"); // disable the focused button

		// this disables the button, but the unfocus change is applied before all states
		Assert.That(App.FindElement("button3").GetText(), Is.EqualTo("Disabled"));

		// we are shrinking the focused button a bit, so it should have been unfocused after disabling
		var rectAfter = App.FindElement("button3").GetRect();
		Assert.That(rectBefore, Is.EqualTo(rectAfter));
	}
}
