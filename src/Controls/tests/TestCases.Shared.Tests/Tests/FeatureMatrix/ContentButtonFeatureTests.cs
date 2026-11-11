using System.Globalization;
using System.Text.RegularExpressions;
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

	double GetStatusValue(string name)
	{
		var match = Regex.Match(Status, $@"\b{name}=(?<value>-?\d+(?:\.\d+)?)");
		Assert.That(match.Success, Is.True, $"Status did not contain {name}: {Status}");
		return double.Parse(match.Groups["value"].Value, CultureInfo.InvariantCulture);
	}

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

#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_CATALYST // Mouse drags do not pan a ScrollView on desktop.
	[Test]
	[Category(UITestCategories.Button)]
	public void ScrollingAncestorCancelsActivation()
	{
		ResetButton();
		var rect = App.WaitForElement("ContentButtonControl").GetRect();

		// The button moves with the scrolled content, so the pointer remains inside it.
		// Only the ancestor's cancellation can prevent activation on release.
		App.DragCoordinates(rect.X + rect.Width / 2, rect.Y + rect.Height * 0.8f,
			rect.X + rect.Width / 2, rect.Y + rect.Height * 0.2f);

		Assert.That(GetStatusValue("ScrollY"), Is.GreaterThan(0));
		Assert.That(Status, Does.Contain("Commands=0; Clicks=0"));
		Assert.That(GetStatusValue("Pressed"), Is.EqualTo(GetStatusValue("Released")));
		Assert.That(Status, Does.Contain("IsPressed=False"));
	}
#endif

	[Test]
	[Category(UITestCategories.Button)]
	public void DraggingOutsideCancelsActivation()
	{
		ResetButton();
		var rect = App.WaitForElement("ContentButtonControl").GetRect();
		App.DragCoordinates(rect.X + rect.Width / 2, rect.Y + rect.Height / 2,
			rect.X + rect.Width * 1.1f, rect.Y + rect.Height / 2);
		Assert.That(Status, Does.Contain("Commands=0; Clicks=0"));
		Assert.That(GetStatusValue("Pressed"), Is.EqualTo(GetStatusValue("Released")));
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
