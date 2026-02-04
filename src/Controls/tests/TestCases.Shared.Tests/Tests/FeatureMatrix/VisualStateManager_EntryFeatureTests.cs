using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

[Category(UITestCategories.ViewBaseTests)]
public class VisualStateManager_EntryFeatureTests : _GalleryUITest
{
	public const string VisualStateManagerEntryFeatureTests = "VisualStateManager Feature Matrix";
	public override string GalleryPageName => VisualStateManagerEntryFeatureTests;

	public VisualStateManager_EntryFeatureTests(TestDevice device)
		: base(device)
	{
	}

	[Test, Order(1)]
	public void Entry_Focus_UpdatesStateLabel()
	{
		try { App.WaitForElement("VSMEntryButton", timeout: System.TimeSpan.FromSeconds(1)); App.Tap("VSMEntryButton"); } catch { }
		App.WaitForElement("EntryFocus");
		App.Tap("EntryFocus");
		App.WaitForElement("EntryState");
		var stateText = App.FindElement("EntryState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Focused"));
		VerifyScreenshot(retryTimeout: System.TimeSpan.FromSeconds(2));
	}

	[Test, Order(2)]
	public void Entry_Disable_UpdatesStateLabel()
	{
		try { App.WaitForElement("VSMEntryButton", timeout: System.TimeSpan.FromSeconds(1)); App.Tap("VSMEntryButton"); } catch { }
		App.WaitForElement("EntryDisable");
		App.Tap("EntryDisable");
		App.WaitForElement("EntryState");
		var stateText = App.FindElement("EntryState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Disabled"));
		VerifyScreenshot(retryTimeout: System.TimeSpan.FromSeconds(2));
	}

	[Test, Order(3)]
	public void Entry_Enable_ShowsUnfocused()
	{
		try { App.WaitForElement("VSMEntryButton", timeout: System.TimeSpan.FromSeconds(1)); App.Tap("VSMEntryButton"); } catch { }
		App.WaitForElement("EntryDisable");
		App.Tap("EntryDisable"); // toggle enable
		App.WaitForElement("EntryState");
		var stateText = App.FindElement("EntryState").GetText();
		Assert.That(stateText, Is.EqualTo("State: Unfocused"));
		VerifyScreenshot(retryTimeout: System.TimeSpan.FromSeconds(2));
	}
}