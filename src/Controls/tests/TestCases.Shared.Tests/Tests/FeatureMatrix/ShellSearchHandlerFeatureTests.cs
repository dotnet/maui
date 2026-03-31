using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

/// <summary>
/// Shell SearchHandler feature matrix tests.
/// WARNING: Tests use [Order] and maintain state across methods.
/// They are NOT independent — running out of order may cause failures.
/// The first test navigates to the Shell Search page; subsequent tests
/// continue from the same page. Each Options round-trip calls Reset()
/// on the ViewModel so all properties return to defaults before the
/// next option is applied.
/// </summary>
public class ShellSearchHandlerFeatureTests : _GalleryUITest
{
	public const string ShellSearchFeatureMatrix = "Shell Feature Matrix";
	public override string GalleryPageName => ShellSearchFeatureMatrix;
	public const string Options = "Options";
	public const string Apply = "Apply";

	public ShellSearchHandlerFeatureTests(TestDevice device)
		: base(device)
	{
	}

	void OpenOptions()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
	}

	void ApplyAndReturn()
	{
		App.WaitForElement(Apply);
		App.Tap(Apply);
	}

	// ── Focus / Unfocus Events ────────────────────────────────────────────────────

	[Test, Order(1)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_FocusEvent()
	{
		App.WaitForElement("ShellSearchButton");
		App.Tap("ShellSearchButton");
		App.WaitForElement("FocusButton");
		App.Tap("FocusButton");
		var status = App.WaitForElement("FocusStatus").GetText();
		Assert.That(status, Is.EqualTo("Focused"));
	}

	[Test, Order(2)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_UnfocusEvent()
	{
		App.Tap("FocusButton");
		App.WaitForElement("FocusStatus");
		App.Tap("UnfocusButton");
		var status = App.WaitForElement("FocusStatus").GetText();
		Assert.That(status, Is.EqualTo("Unfocused"));
	}

	// ── Programmatic Query ────────────────────────────────────────────────────────

	[Test, Order(3)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_SetQueryProgrammatically()
	{
		App.Tap("SetQueryButton");
		var log = App.WaitForElement("QueryChangedLog").GetText();
		Assert.That(log, Does.Contain("Robin"));
	}

	// ── Search Results & Selection ────────────────────────────────────────────────

	[Test, Order(4)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_SelectedItem()
	{
		var searchHandler = App.GetShellSearchHandler();
		searchHandler.Tap();
		searchHandler.SendKeys("Mango");
		App.TapFirstSearchResult(this, searchHandler, "SearchResultName");
		App.WaitForElement("DetailName");
		App.WaitForElement("BackToSearchButton");
		App.Tap("BackToSearchButton");
		var selectedItem = App.WaitForElement("SelectedItem").GetText();
		Assert.That(selectedItem, Is.EqualTo("Mango"));
	}

	// ── SearchCommand & CommandParameter ──────────────────────────────────────────

	[Test, Order(5)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_CommandParameter()
	{
		OpenOptions();
		App.WaitForElement("CommandParameterEntry");
		App.ClearText("CommandParameterEntry");
		App.EnterText("CommandParameterEntry", "TestParam");
		ApplyAndReturn();
		var paramText = App.WaitForElement("CommandParameter").GetText();
		Assert.That(paramText, Is.EqualTo("TestParam"));
	}

	[Test, Order(6)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_ClearPlaceholderCommand()
	{
		OpenOptions();
		App.WaitForElement("ClearPlaceholderEnabledTrue");
		App.Tap("ClearPlaceholderEnabledTrue");
		App.WaitForElement("ClearPlaceholderCommandParameterEntry");
		App.ClearText("ClearPlaceholderCommandParameterEntry");
		App.EnterText("ClearPlaceholderCommandParameterEntry", "ClearParam");
		ApplyAndReturn();
		App.WaitForElement("TriggerClearPlaceholderButton");
		App.Tap("TriggerClearPlaceholderButton");
		var fired = App.WaitForElement("ClearPlaceholderCommandFired").GetText();
		Assert.That(fired, Does.Contain("ClearPlaceholder"));
		var param = App.WaitForElement("ClearPlaceholderCommandParameter").GetText();
		Assert.That(param, Is.EqualTo("ClearParam"));
	}

	// ── ShowsResults = false ──────────────────────────────────────────────────────

	[Test, Order(7)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_ShowsResultsFalse()
	{
		OpenOptions();
		App.WaitForElement("ShowsResultsFalse");
		App.Tap("ShowsResultsFalse");
		ApplyAndReturn();
		var searchHandler = App.GetShellSearchHandler();
		searchHandler.Tap();
		searchHandler.SendKeys("Apple");
		App.WaitForNoElement("SearchResultName");
	}

	// ── Visual Property Tests (Screenshot-based) ─────────────────────────────────

	[Test, Order(8)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_PlaceholderColor()
	{
		OpenOptions();
		App.WaitForElement("PlaceholderColorPinkRadio");
		App.Tap("PlaceholderColorPinkRadio");
		ApplyAndReturn();
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(9)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_TextColor()
	{
		OpenOptions();
		App.WaitForElement("TextColorRedRadio");
		App.Tap("TextColorRedRadio");
		ApplyAndReturn();
		var searchHandler = App.GetShellSearchHandler();
		searchHandler.Tap();
		searchHandler.SendKeys("Testing");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(10)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_CancelButtonColor()
	{
		OpenOptions();
		App.WaitForElement("CancelButtonColorOrangeRadio");
		App.Tap("CancelButtonColorOrangeRadio");
		ApplyAndReturn();
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(11)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_BackgroundColor()
	{
		OpenOptions();
		App.WaitForElement("BackgroundColorYellowRadio");
		App.Tap("BackgroundColorYellowRadio");
		ApplyAndReturn();
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(12)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_FontAttributesBold()
	{
		OpenOptions();
		App.WaitForElement("FontAttributesBold");
		App.Tap("FontAttributesBold");
		ApplyAndReturn();
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(13)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_FontAttributesItalic()
	{
		OpenOptions();
		App.WaitForElement("FontAttributesItalic");
		App.Tap("FontAttributesItalic");
		ApplyAndReturn();
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(14)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_FontFamily()
	{
		OpenOptions();
		App.WaitForElement("FontFamilyDokdoRadio");
		App.Tap("FontFamilyDokdoRadio");
		ApplyAndReturn();
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(15)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_FontSize()
	{
		OpenOptions();
		App.WaitForElement("FontSizeEntry");
		App.ClearText("FontSizeEntry");
		App.EnterText("FontSizeEntry", "24");
		ApplyAndReturn();
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(16)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_CharacterSpacing()
	{
		OpenOptions();
		App.WaitForElement("CharacterSpacingEntry");
		App.ClearText("CharacterSpacingEntry");
		App.EnterText("CharacterSpacingEntry", "5");
		ApplyAndReturn();
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(17)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_HorizontalTextAlignmentCenter()
	{
		OpenOptions();
		App.WaitForElement("HTextAlignmentCenter");
		App.Tap("HTextAlignmentCenter");
		ApplyAndReturn();
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(18)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_HorizontalTextAlignmentEnd()
	{
		OpenOptions();
		App.WaitForElement("HTextAlignmentEnd");
		App.Tap("HTextAlignmentEnd");
		ApplyAndReturn();
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(19)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_VerticalTextAlignmentStart()
	{
		OpenOptions();
		App.WaitForElement("VTextAlignmentStart");
		App.Tap("VTextAlignmentStart");
		ApplyAndReturn();
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(20)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_VerticalTextAlignmentEnd()
	{
		OpenOptions();
		App.WaitForElement("VTextAlignmentEnd");
		App.Tap("VTextAlignmentEnd");
		ApplyAndReturn();
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(21)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_TextTransformUppercase()
	{
		OpenOptions();
		App.WaitForElement("TextTransformUppercase");
		App.Tap("TextTransformUppercase");
		ApplyAndReturn();
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(22)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_TextTransformLowercase()
	{
		OpenOptions();
		App.WaitForElement("TextTransformLowercase");
		App.Tap("TextTransformLowercase");
		ApplyAndReturn();
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	// ── Placeholder ──────────────────────────────────────────────────────────────

	[Test, Order(23)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_Placeholder()
	{
		OpenOptions();
		App.WaitForElement("PlaceholderEntry");
		App.ClearText("PlaceholderEntry");
		App.EnterText("PlaceholderEntry", "Custom placeholder");
		ApplyAndReturn();
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	// ── IsSearchEnabled ──────────────────────────────────────────────────────────

	[Test, Order(24)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_IsSearchEnabledFalse()
	{
		OpenOptions();
		App.WaitForElement("IsSearchEnabledFalse");
		App.Tap("IsSearchEnabledFalse");
		ApplyAndReturn();
		var searchHandler = App.GetShellSearchHandler();
		searchHandler.Tap();
		searchHandler.SendKeys("Apple");
		var queryLog = App.WaitForElement("QueryChangedLog").GetText();
		Assert.That(queryLog, Is.Null.Or.Empty, "QueryChangedLog should be empty when IsSearchEnabled is false");
		App.WaitForNoElement("SearchResultName");
	}

	// ── SearchBoxVisibility ──────────────────────────────────────────────────────

	[Test, Order(25)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_SearchBoxVisibilityHidden()
	{
		OpenOptions();
		App.WaitForElement("SearchBoxVisibilityHidden");
		App.Tap("SearchBoxVisibilityHidden");
		ApplyAndReturn();
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(26)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_SearchBoxVisibilityCollapsible()
	{
		OpenOptions();
		App.WaitForElement("SearchBoxVisibilityCollapsible");
		App.Tap("SearchBoxVisibilityCollapsible");
		ApplyAndReturn();
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(27)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_SearchBoxVisibilityExpanded()
	{
		OpenOptions();
		App.WaitForElement("SearchBoxVisibilityExpanded");
		App.Tap("SearchBoxVisibilityExpanded");
		ApplyAndReturn();
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	// ── FontAutoScalingEnabled ────────────────────────────────────────────────────

	[Test, Order(28)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_FontAutoScalingDisabled()
	{
		OpenOptions();
		App.WaitForElement("FontAutoScalingEnabledFalse");
		App.Tap("FontAutoScalingEnabledFalse");
		ApplyAndReturn();
		App.WaitForElement("SetQueryButton");
		App.Tap("SetQueryButton");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	// ── ItemsSource ──────────────────────────────────────────────────────────────

	[Test, Order(29)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_ItemsSourceFruits()
	{
		OpenOptions();
		App.WaitForElement("ItemsSourceFruitsRadio");
		App.Tap("ItemsSourceFruitsRadio");
		ApplyAndReturn();
		var searchHandler = App.GetShellSearchHandler();
		searchHandler.Tap();
		searchHandler.SendKeys("App");
		var result = App.WaitForElement("SearchResultName").GetText();
		Assert.That(result, Is.EqualTo("Apple"));
		searchHandler.Clear();
		searchHandler.SendKeys("Peacock");
		App.WaitForNoElement("SearchResultName");
	}

	[Test, Order(30)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_ItemsSourceBirds()
	{
		OpenOptions();
		App.WaitForElement("ItemsSourceBirdsRadio");
		App.Tap("ItemsSourceBirdsRadio");
		ApplyAndReturn();
		var searchHandler = App.GetShellSearchHandler();
		searchHandler.Tap();
		searchHandler.SendKeys("Pea");
		var result = App.WaitForElement("SearchResultName").GetText();
		Assert.That(result, Is.EqualTo("Peacock"));
		searchHandler.Clear();
		searchHandler.SendKeys("Apple");
		App.WaitForNoElement("SearchResultName");
	}

	[Test, Order(31)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_ItemsSourceQuery()
	{
		OpenOptions();
		App.WaitForElement("ItemsSourceQueryRadio");
		App.Tap("ItemsSourceQueryRadio");
		ApplyAndReturn();
		var searchHandler = App.GetShellSearchHandler();
		searchHandler.Tap();
		searchHandler.SendKeys("Pea");
		var result = App.WaitForElement("SearchResultName").GetText();
		Assert.That(result, Is.EqualTo("Peacock"));
		searchHandler.Clear();
		searchHandler.SendKeys("Apple");
		var result2 = App.WaitForElement("SearchResultName").GetText();
		Assert.That(result2, Is.EqualTo("Apple"));
	}

	[Test, Order(32)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_ItemsSourceNull()
	{
		OpenOptions();
		App.WaitForElement("ItemsSourceNullRadio");
		App.Tap("ItemsSourceNullRadio");
		ApplyAndReturn();
		var searchHandler = App.GetShellSearchHandler();
		searchHandler.Tap();
		searchHandler.SendKeys("Apple");
		App.WaitForNoElement("SearchResultName");
	}

	// ── ItemTemplate ──────────────────────────────────────────────────────────────

	[Test, Order(33)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_ItemTemplateCustom()
	{
		OpenOptions();
		App.WaitForElement("ItemTemplateCustomRadio");
		App.Tap("ItemTemplateCustomRadio");
		ApplyAndReturn();
		var searchHandler = App.GetShellSearchHandler();
		searchHandler.Tap();
		searchHandler.SendKeys("App");
		App.WaitForElement("SearchResultName");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	// ── Icons ─────────────────────────────────────────────────────────────────────

	[Test, Order(34)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_QueryIcon()
	{
		OpenOptions();
		App.WaitForElement("QueryIconCustomRadio");
		App.Tap("QueryIconCustomRadio");
		ApplyAndReturn();
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(35)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_ClearIcon()
	{
		OpenOptions();
		App.WaitForElement("ClearIconCustomRadio");
		App.Tap("ClearIconCustomRadio");
		ApplyAndReturn();
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(36)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_ClearPlaceholderIcon()
	{
		OpenOptions();
		App.WaitForElement("ClearPlaceholderIconCustomRadio");
		App.Tap("ClearPlaceholderIconCustomRadio");
		ApplyAndReturn();
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	// ── Keyboard ──────────────────────────────────────────────────────────────────

	[Test, Order(37)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_KeyboardNumeric()
	{
		OpenOptions();
		App.WaitForElement("KeyboardNumeric");
		App.Tap("KeyboardNumeric");
		ApplyAndReturn();
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(38)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_KeyboardEmail()
	{
		OpenOptions();
		App.WaitForElement("KeyboardEmail");
		App.Tap("KeyboardEmail");
		ApplyAndReturn();
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(39)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_KeyboardTelephone()
	{
		OpenOptions();
		App.WaitForElement("KeyboardTelephone");
		App.Tap("KeyboardTelephone");
		ApplyAndReturn();
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(40)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_KeyboardUrl()
	{
		OpenOptions();
		App.WaitForElement("KeyboardUrl");
		App.Tap("KeyboardUrl");
		ApplyAndReturn();
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	// ── Missing Coverage Tests ───────────────────────────────────────────────────

	[Test, Order(41)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_ItemTemplateNull()
	{
		OpenOptions();
		App.WaitForElement("ItemTemplateNullRadio");
		App.Tap("ItemTemplateNullRadio");
		ApplyAndReturn();
		var searchHandler = App.GetShellSearchHandler();
		searchHandler.Tap();
		searchHandler.SendKeys("App");
		App.WaitForElement("SearchResultName");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(42)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_ClearPlaceholderEnabledTrue()
	{
		OpenOptions();
		App.WaitForElement("ClearPlaceholderEnabledTrue");
		App.Tap("ClearPlaceholderEnabledTrue");
		App.WaitForElement("ClearPlaceholderIconCustomRadio");
		App.Tap("ClearPlaceholderIconCustomRadio");
		ApplyAndReturn();
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(43)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_HorizontalTextAlignmentStart()
	{
		OpenOptions();
		App.WaitForElement("HTextAlignmentStart");
		App.Tap("HTextAlignmentStart");
		ApplyAndReturn();
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(44)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_VerticalTextAlignmentCenter()
	{
		OpenOptions();
		App.WaitForElement("VTextAlignmentCenter");
		App.Tap("VTextAlignmentCenter");
		ApplyAndReturn();
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
}