using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue31889 : _IssuesUITest
{
	public Issue31889(TestDevice testDevice) : base(testDevice) { }

	private const string TestEntryId = "TestEntry";
	private const string TestEditorId = "TestEditor";
	private const string DarkThemeButtonId = "DarkThemeButton";
	private const string LightThemeButtonId = "LightThemeButton";
	private const string DoneButtonId = "Done";
	private const string TextColorDarkTheme = "EntryAndEditorTextColorAppThemeBindingUpdatesOnDarkTheme";
	private const string TextColorLightTheme = "EntryAndEditorTextColorAppThemeBindingUpdatesOnLightTheme";
	private const string PlaceholderColorDarkTheme = "EntryAndEditorPlaceholderTextColorAppThemeBindingUpdatesOnDarkTheme";
	private const string PlaceholderColorLightTheme = "EntryAndEditorPlaceholderTextColorAppThemeBindingUpdatesOnLightTheme";
	public override string Issue => "Entry and Editor AppThemeBinding colors for text and placeholder reset to default on theme change";

	[Test, Order(1)]
	[Category(UITestCategories.Entry)]
	public void EntryAndEditorTextColorAppThemeBindingUpdatesOnThemeChange()
	{
		Exception? exception = null;

		App.WaitForElement(TestEntryId);
		App.WaitForElement(DarkThemeButtonId);
		App.Tap(DarkThemeButtonId);
		VerifyScreenshotOrSetException(ref exception, TextColorDarkTheme);
		App.WaitForElement(LightThemeButtonId);
		App.Tap(LightThemeButtonId);
		VerifyScreenshotOrSetException(ref exception, TextColorLightTheme);

		if (exception is not null)
		{
			throw exception;
		}
	}

	[Test, Order(2)]
	[Category(UITestCategories.Entry)]
	public void EntryAndEditorPlaceholderTextColorAppThemeBindingUpdatesOnThemeChange()
	{
		Exception? exception = null;

		App.WaitForElement(TestEntryId);
		App.ClearText(TestEntryId);

		App.WaitForElement(TestEditorId);
		App.ClearText(TestEditorId);
#if MACCATALYST // On macOS, clearing the text in the Editor does not remove the full content.
		App.ClearText(TestEditorId);
#endif
#if IOS
		if (App.IsKeyboardShown())
		{
			App.WaitForElement(DoneButtonId);
			App.Tap(DoneButtonId);
		}
#endif
		App.WaitForElement(DarkThemeButtonId);
		App.Tap(DarkThemeButtonId);
		VerifyScreenshotOrSetException(ref exception, PlaceholderColorDarkTheme);
		App.WaitForElement(LightThemeButtonId);
		App.Tap(LightThemeButtonId);
		VerifyScreenshotOrSetException(ref exception, PlaceholderColorLightTheme);

		if (exception is not null)
		{
			throw exception;
		}
	}
}