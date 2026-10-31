using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

public class ContentButtonFeatureTests : _GalleryUITest
{
	public override string GalleryPageName => "ContentButton Feature Matrix";
	public ContentButtonFeatureTests(TestDevice device) : base(device) { }

	void ResetButton()
	{
		App.WaitForElement("Reset");
		App.Tap("Reset");
		App.WaitForElement("ContentButtonControl");
	}

	string Status => App.FindElement("ContentButtonStatus").GetText()!;

	void TapContentButton(double x = 0.5, double y = 0.5)
	{
		var rect = App.WaitForElement("ContentButtonControl").GetRect();
		App.TapCoordinates((float)(rect.X + rect.Width * x), (float)(rect.Y + rect.Height * y));
	}

	[TestCase(0.25, 0.5)]
	[TestCase(0.75, 0.5)]
	[TestCase(0.02, 0.5)]
	[Category(UITestCategories.Button)]
	public void PresentationContentAndEmptySpaceActivateExactlyOnce(double x, double y)
	{
		ResetButton();
		TapContentButton(x, y);
		Assert.That(Status, Does.Contain("Commands=1; Clicks=1"));
		Assert.That(Status, Does.Contain("Pressed=1; Released=1"));
		Assert.That(Status, Does.Contain("IsPressed=False"));
		Assert.That(Status, Does.Contain("Order=Command,Clicked"));
	}

	[TestCase("ReplaceContent")]
	[TestCase("ClearContent")]
	[TestCase("Reconnect")]
	[TestCase("ChangeStyle")]
	[Category(UITestCategories.Button)]
	public void UpdatedOrReconnectedButtonActivatesExactlyOnce(string action)
	{
		ResetButton();
		App.Tap(action);
		TapContentButton();
		Assert.That(Status, Does.Contain("Commands=1; Clicks=1"));
		Assert.That(Status, Does.Contain("Pressed=1; Released=1"));
		Assert.That(Status, Does.Contain("IsPressed=False"));
	}

	[Test]
	[Category(UITestCategories.Button)]
	public void DisablingDuringPressDoesNotActivateOrLeavePressedState()
	{
		ResetButton();
		App.Tap("DisableOnPress");
		TapContentButton();
		Assert.That(Status, Does.Contain("Commands=0; Clicks=0"));
		Assert.That(Status, Does.Contain("Pressed=1; Released=1"));
		Assert.That(Status, Does.Contain("IsPressed=False"));
	}

	[Test]
	[Category(UITestCategories.Button)]
	public void RemovingDuringPressDoesNotActivateOrLeavePressedState()
	{
		ResetButton();
		App.Tap("RemoveOnPress");
		TapContentButton();
		Assert.That(Status, Does.Contain("Commands=0; Clicks=0"));
		Assert.That(Status, Does.Contain("Pressed=1; Released=1"));
		Assert.That(Status, Does.Contain("IsPressed=False"));
	}

	[Test]
	[Category(UITestCategories.Button)]
	public void DraggingIntoScrollAncestorCancelsActivation()
	{
		ResetButton();
		var rect = App.WaitForElement("ContentButtonControl").GetRect();
		App.DragCoordinates(rect.X + rect.Width / 2, rect.Y + rect.Height / 2,
			rect.X + rect.Width / 2, rect.Y + rect.Height + 80);
		Assert.That(Status, Does.Contain("Commands=0; Clicks=0"));
		Assert.That(Status, Does.Contain("IsPressed=False"));
	}

	[Test]
	[Category(UITestCategories.Button)]
	public void DraggingOutsideCancelsActivation()
	{
		ResetButton();
		var rect = App.WaitForElement("ContentButtonControl").GetRect();
		App.DragCoordinates(rect.X + rect.Width / 2, rect.Y + rect.Height / 2,
			rect.X + rect.Width * 1.1f, rect.Y + rect.Height / 2);
		Assert.That(Status, Does.Contain("Commands=0; Clicks=0"));
		Assert.That(Status, Does.Contain("IsPressed=False"));
	}

#if WINDOWS
	[TestCase(0x20)]
	[TestCase(0x0D)]
	[Category(UITestCategories.Button)]
	public void KeyboardActivationIsExactlyOnce(int virtualKey)
	{
		ResetButton();
		App.Tap("FocusButton");
		((AppiumApp)App).Driver.ExecuteScript("windows: keys", new Dictionary<string, object>
		{
			["actions"] = new[]
			{
				new Dictionary<string, object> { ["virtualKeyCode"] = virtualKey, ["down"] = true },
				new Dictionary<string, object> { ["virtualKeyCode"] = virtualKey, ["down"] = false },
			}
		});
		Assert.That(Status, Does.Contain("Commands=1; Clicks=1"));
		Assert.That(Status, Does.Contain("IsPressed=False"));
	}
#endif
}
