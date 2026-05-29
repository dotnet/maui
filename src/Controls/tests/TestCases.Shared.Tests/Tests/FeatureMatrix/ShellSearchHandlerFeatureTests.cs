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

	[Test, Order(1)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_Placeholder()
	{
		App.WaitForElement("ShellSearchButton");
		App.Tap("ShellSearchButton");
		OpenOptions();
		App.WaitForElement("PlaceholderEntry");
		App.ClearText("PlaceholderEntry");
		App.EnterText("PlaceholderEntry", "Custom placeholder");
		ApplyAndReturn();
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_ANDROID// Issue Link: https://github.com/dotnet/maui/issues/14497
	[Test, Order(2)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_SetQueryProgrammatically()
	{
		App.WaitForElement("SetQueryButton");
		App.Tap("SetQueryButton");
		var log = App.WaitForElement("QueryChangedLog").GetText();
		Assert.That(log, Does.Contain("Robin"));
	}
#endif

#if TEST_FAILS_ON_WINDOWS // Issue Link: https://github.com/dotnet/maui/issues/29493

	[Test, Order(3)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_PlaceholderColor()
	{
		OpenOptions();
		App.WaitForElement("PlaceholderColorPinkRadio");
		App.Tap("PlaceholderColorPinkRadio");
		ApplyAndReturn();
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(4)]
	[Ignore("Fails on all platforms, IssueLink = https://github.com/dotnet/maui/issues/35088")]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_SelectedItem()
	{
		App.WaitForElement("ToggleShowsResultsButton");
		App.Tap("ToggleShowsResultsButton");
		var searchHandler = App.GetShellSearchHandler();
		searchHandler.Tap();
		searchHandler.Clear();
		searchHandler.SendKeys("Mango");
		App.WaitForElement("Mango");
		App.TapFirstSearchResult(this, searchHandler, "SearchResultName");
		App.WaitForElement("Mango");
		App.WaitForElement("BackToSearchButton");
		App.Tap("BackToSearchButton");
		var selectedItem = App.WaitForElement("SelectedItem").GetText();
		Assert.That(selectedItem, Is.EqualTo("Mango"));
	}


	[Test, Order(5)]
	[Ignore("Fails on all platforms, IssueLink = https://github.com/dotnet/maui/issues/35088")]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_BackgroundColor()
	{
		OpenOptions();
		App.WaitForElement("BackgroundColorYellowRadio");
		App.Tap("BackgroundColorYellowRadio");
		ApplyAndReturn();
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(6)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_FontAttributesBold()
	{
		OpenOptions();
		App.WaitForElement("FontAttributesBold");
		App.Tap("FontAttributesBold");
		ApplyAndReturn();
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(7)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_FontAttributesItalic()
	{
		OpenOptions();
		App.WaitForElement("FontAttributesItalic");
		App.Tap("FontAttributesItalic");
		ApplyAndReturn();
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(8)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_FontFamily()
	{
		OpenOptions();
		App.WaitForElement("FontFamilyDokdoRadio");
		App.Tap("FontFamilyDokdoRadio");
		ApplyAndReturn();
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(9)]
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

#if TEST_FAILS_ON_ANDROID // Issue Link: https://github.com/dotnet/maui/issues/29493
	[Test, Order(10)]
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
#endif

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST // Issue Link: https://github.com/dotnet/maui/issues/35085
	[Test, Order(11)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_HorizontalTextAlignmentCenter()
	{
		OpenOptions();
		App.WaitForElement("HTextAlignmentCenter");
		App.Tap("HTextAlignmentCenter");
		ApplyAndReturn();
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(12)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_HorizontalTextAlignmentEnd()
	{
		OpenOptions();
		App.WaitForElement("HTextAlignmentEnd");
		App.Tap("HTextAlignmentEnd");
		ApplyAndReturn();
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(13)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_VerticalTextAlignmentStart()
	{
		OpenOptions();
		App.WaitForElement("VTextAlignmentStart");
		App.Tap("VTextAlignmentStart");
		ApplyAndReturn();
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
#endif

	[Test, Order(14)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_VerticalTextAlignmentEnd()
	{
		OpenOptions();
		App.WaitForElement("VTextAlignmentEnd");
		App.Tap("VTextAlignmentEnd");
		ApplyAndReturn();
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

#if TEST_FAILS_ON_ANDROID  // Issue Link:
	[Test, Order(15)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_TextTransformUppercase()
	{
		OpenOptions();
		App.WaitForElement("TextTransformUppercase");
		App.Tap("TextTransformUppercase");
		ApplyAndReturn();
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
#endif

	[Test, Order(16)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_TextTransformLowercase()
	{
		OpenOptions();
		App.WaitForElement("TextTransformLowercase");
		App.Tap("TextTransformLowercase");
		ApplyAndReturn();
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS // Issue Link: https://github.com/dotnet/maui/issues/35085
	[Test, Order(17)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_HorizontalTextAlignmentStart()
	{
		OpenOptions();
		App.WaitForElement("HTextAlignmentStart");
		App.Tap("HTextAlignmentStart");
		ApplyAndReturn();
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
#endif

	[Test, Order(18)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_VerticalTextAlignmentCenter()
	{
		OpenOptions();
		App.WaitForElement("VTextAlignmentCenter");
		App.Tap("VTextAlignmentCenter");
		ApplyAndReturn();
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
#endif

	[Test, Order(19)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_IsSearchEnabledFalse()
	{
		OpenOptions();
		App.WaitForElement("IsSearchEnabledFalse");
		App.Tap("IsSearchEnabledFalse");
		ApplyAndReturn();
		var searchHandler = App.GetShellSearchHandler();
		searchHandler.Tap();
		var status = App.WaitForElement("FocusStatus").GetText();
		Assert.That(status, Is.Null.Or.Empty);

	}

	[Test, Order(20)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_SearchBoxVisibilityHidden()
	{
		OpenOptions();
		App.WaitForElement("SearchBoxVisibilityHidden");
		App.Tap("SearchBoxVisibilityHidden");
		ApplyAndReturn();
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(21)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_SearchBoxVisibilityCollapsible()
	{
		OpenOptions();
		App.WaitForElement("SearchBoxVisibilityCollapsible");
		App.Tap("SearchBoxVisibilityCollapsible");
		ApplyAndReturn();
#if MACCATALYST
		App.ScrollUp("SearchContentPage", ScrollStrategy.Gesture, 0.9, 1000);
#elif IOS
		var rect = App.WaitForElement("SearchContentPage").GetRect();
		float startX = rect.X + rect.Width * 0.05f;
		float startY = rect.Y + rect.Height * 0.15f;
		float endY = rect.Y + rect.Height * 0.6f;
		App.DragCoordinates(startX, startY, startX, endY);
#endif
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}


	[Test, Order(22)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_SearchBoxVisibilityExpanded()
	{
		OpenOptions();
		App.WaitForElement("SearchBoxVisibilityExpanded");
		App.Tap("SearchBoxVisibilityExpanded");
		ApplyAndReturn();
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_WINDOWS // Issue Link: https://github.com/dotnet/maui/issues/28619 due to clearPlaceHolderIcon not showing couldn't able to find this property is working or not
	[Test, Order(23)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_ClearPlaceholderEnabledTrue()
	{
		OpenOptions();
		App.WaitForElement("ClearPlaceholderEnabledTrue");
		App.Tap("ClearPlaceholderEnabledTrue");
		ApplyAndReturn();
		App.WaitForElement("ToggleClearPlaceholderIconButton");
		App.Tap("ToggleClearPlaceholderIconButton");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
#endif

	[Test, Order(24)]
	[Ignore("Fails on all platforms, see https://github.com/dotnet/maui/issues/28619")]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_QueryIcon()
	{
		App.WaitForElement("ToggleQueryIconButton");
		App.Tap("ToggleQueryIconButton");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(25)]
	[Ignore("Fails on all platforms, see https://github.com/dotnet/maui/issues/28619")]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_ClearIcon()
	{
		App.WaitForElement("ToggleClearIconButton");
		App.Tap("ToggleClearIconButton");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(26)]
	[Ignore("Fails on all platforms, see https://github.com/dotnet/maui/issues/28619")]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_ClearPlaceholderIcon()
	{
		OpenOptions();
		App.WaitForElement("ClearPlaceholderEnabledTrue");
		App.Tap("ClearPlaceholderEnabledTrue");
		ApplyAndReturn();
		App.WaitForElement("ToggleClearPlaceholderIconButton");
		App.Tap("ToggleClearPlaceholderIconButton");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(27)]
	[Ignore("Fails on all platforms")]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_ShowsResultsTrue()
	{
		App.WaitForElement("ToggleShowsResultsButton");
		App.Tap("ToggleShowsResultsButton");
		var searchHandler = App.GetShellSearchHandler();
		searchHandler.Tap();
		searchHandler.Clear();
		searchHandler.SendKeys("Apple");
		App.WaitForElement("Apple");
		App.TapFirstSearchResult(this, searchHandler, "SearchResultName");
		App.WaitForElement("Apple");
		App.WaitForElement("BackToSearchButton");
		App.Tap("BackToSearchButton");
	}

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST // Issue Link: https://github.com/dotnet/maui/issues/30771
	[Test, Order(28)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_FocusAndUnfocusEvent()
	{
		App.WaitForElement("ToggleShowsResultsButton");
		App.Tap("ToggleShowsResultsButton");
		var searchHandler = App.GetShellSearchHandler();
		searchHandler.Tap();
		var status = App.WaitForElement("FocusStatus").GetText();
		Assert.That(status, Is.EqualTo("Focused"));
		searchHandler.Clear();
		searchHandler.SendKeys("Mango");
		App.WaitForElement("Mango");
		App.TapFirstSearchResult(this, searchHandler, "SearchResultName");
		App.WaitForElement("Mango");
		App.WaitForElement("BackToSearchButton");
		App.Tap("BackToSearchButton");
		var status2 = App.WaitForElement("FocusStatus").GetText();
		Assert.That(status2, Is.EqualTo("Unfocused"));
	}
#endif

	[Test, Order(29)]
	[Ignore("Fails on all platforms")]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_ItemsSourceFruits()
	{
		OpenOptions();
		App.WaitForElement("ItemsSourceFruitsRadio");
		App.Tap("ItemsSourceFruitsRadio");
		ApplyAndReturn();
		App.WaitForElement("ToggleShowsResultsButton");
		App.Tap("ToggleShowsResultsButton");
		var searchHandler = App.GetShellSearchHandler();
		searchHandler.Tap();
		searchHandler.Clear();
		searchHandler.SendKeys("Apple");
		App.WaitForElement("Apple");
		App.TapFirstSearchResult(this, searchHandler, "SearchResultName");
		App.WaitForElement("Fruit");
		App.WaitForElement("BackToSearchButton");
		App.Tap("BackToSearchButton");
	}

	[Test, Order(30)]
	[Ignore("Fails on all platforms")]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_ItemsSourceBirds()
	{
		OpenOptions();
		App.WaitForElement("ItemsSourceBirdsRadio");
		App.Tap("ItemsSourceBirdsRadio");
		ApplyAndReturn();
		App.WaitForElement("ToggleShowsResultsButton");
		App.Tap("ToggleShowsResultsButton");
		var searchHandler = App.GetShellSearchHandler();
		searchHandler.Tap();
		searchHandler.Clear();
		searchHandler.SendKeys("Peacock");
		App.WaitForElement("Peacock");
		App.TapFirstSearchResult(this, searchHandler, "SearchResultName");
		App.WaitForElement("Bird");
		App.WaitForElement("BackToSearchButton");
		App.Tap("BackToSearchButton");
	}

	[Test, Order(31)]
	[Ignore("Fails on all platforms")]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_ItemsSourceQuery()
	{
		OpenOptions();
		App.WaitForElement("ItemsSourceQueryRadio");
		App.Tap("ItemsSourceQueryRadio");
		ApplyAndReturn();
		App.WaitForElement("ToggleShowsResultsButton");
		App.Tap("ToggleShowsResultsButton");
		var searchHandler = App.GetShellSearchHandler();
		searchHandler.Tap();
		searchHandler.Clear();
		searchHandler.SendKeys("Peacock");
		App.WaitForElement("Peacock");
		App.TapFirstSearchResult(this, searchHandler, "SearchResultName");
		App.WaitForElement("Bird");
		App.WaitForElement("BackToSearchButton");
		App.Tap("BackToSearchButton");
		var searchHandler2 = App.GetShellSearchHandler();
		searchHandler2.Tap();
		searchHandler.Clear();
		searchHandler2.SendKeys("Apple");
		App.WaitForElement("Apple");
		App.TapFirstSearchResult(this, searchHandler2, "SearchResultName");
		App.WaitForElement("Fruit");
		App.WaitForElement("BackToSearchButton");
		App.Tap("BackToSearchButton");
	}

	[Test, Order(32)]
	[Ignore("Fails on all platforms")]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_ItemsSourceNull()
	{
		OpenOptions();
		App.WaitForElement("ItemsSourceNullRadio");
		App.Tap("ItemsSourceNullRadio");
		ApplyAndReturn();
		App.WaitForElement("ToggleShowsResultsButton");
		App.Tap("ToggleShowsResultsButton");
		var searchHandler = App.GetShellSearchHandler();
		searchHandler.Tap();
		searchHandler.Clear();
		searchHandler.SendKeys("Apple");
		App.WaitForNoElement("Apple");
	}

	[Test, Order(33)]
	[Ignore("Fails on all platforms, see https://github.com/dotnet/maui/issues/34874")]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_ItemTemplateCustom()
	{
		OpenOptions();
		App.WaitForElement("ItemTemplateCustomRadio");
		App.Tap("ItemTemplateCustomRadio");
		ApplyAndReturn();
		App.WaitForElement("ToggleShowsResultsButton");
		App.Tap("ToggleShowsResultsButton");
		var searchHandler = App.GetShellSearchHandler();
		searchHandler.Tap();
		searchHandler.Clear();
		searchHandler.SendKeys("Apple");
		App.WaitForElement("Apple");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(34)]
	[Ignore("Fails on all platforms, see https://github.com/dotnet/maui/issues/34874")]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_ItemTemplateNull()
	{
		OpenOptions();
		App.WaitForElement("ItemTemplateNullRadio");
		App.Tap("ItemTemplateNullRadio");
		ApplyAndReturn();
		App.WaitForElement("ToggleShowsResultsButton");
		App.Tap("ToggleShowsResultsButton");
		var searchHandler = App.GetShellSearchHandler();
		searchHandler.Tap();
		searchHandler.Clear();
		searchHandler.SendKeys("Apple");
		App.WaitForElement("Apple");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(35)]
	[Ignore("Fails on all platforms")]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_CommandParameter()
	{
		OpenOptions();
		App.WaitForElement("CommandParameterEntry");
		App.ClearText("CommandParameterEntry");
		App.EnterText("CommandParameterEntry", "TestParam");
		ApplyAndReturn();
		App.WaitForElement("ToggleShowsResultsButton");
		App.Tap("ToggleShowsResultsButton");
		// Type a query and press Enter to trigger QueryConfirmed → SearchCommand
		var searchHandler = App.GetShellSearchHandler();
		searchHandler.Tap();
		searchHandler.Clear();
		searchHandler.SendKeys("Apple");
		App.WaitForElement("Apple");
		App.TapFirstSearchResult(this, searchHandler, "SearchResultName");
		App.WaitForElement("BackToSearchButton");
		App.Tap("BackToSearchButton");
		var fired = App.WaitForElement("CommandFired").GetText();
		Assert.That(fired, Does.Contain("QueryConfirmed"));
		Assert.That(fired, Does.Contain("Param:TestParam"));
	}

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_WINDOWS
	[Test, Order(36)]
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
		Assert.That(fired, Is.EqualTo("ClearPlaceholder:ClearParam"));
	}
#endif

	[Test, Order(37)]
	[Ignore("Fails on all platforms, see https://github.com/dotnet/maui/issues/26968, https://github.com/dotnet/maui/issues/29493, https://github.com/dotnet/maui/issues/30771")]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_TextColor()
	{
		OpenOptions();
		App.WaitForElement("TextColorRedRadio");
		App.Tap("TextColorRedRadio");
		ApplyAndReturn();
		var searchHandler = App.GetShellSearchHandler();
		searchHandler.Tap();
		searchHandler.Clear();
		searchHandler.SendKeys("Testing");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(38)]
	[Ignore("Fails on all platforms, see https://github.com/dotnet/maui/issues/26968, https://github.com/dotnet/maui/issues/29493, https://github.com/dotnet/maui/issues/30771")]
	[Category(UITestCategories.Shell)]
	public void VerifyShellSearch_CancelButtonColor()
	{
		OpenOptions();
		App.WaitForElement("CancelButtonColorOrangeRadio");
		App.Tap("CancelButtonColorOrangeRadio");
		ApplyAndReturn();
		var searchHandler = App.GetShellSearchHandler();
		searchHandler.Tap();
		searchHandler.Clear();
		searchHandler.SendKeys("Testing");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
}
