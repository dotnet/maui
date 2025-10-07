using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue31889 : _IssuesUITest
{
	public Issue31889(TestDevice testDevice) : base(testDevice) { }

	public override string Issue => "Entry and Editor AppThemeBinding colors for text and placeholder reset to default on theme change";

	[Test, Order(1)]
	[Category(UITestCategories.Entry)]
	public void EntryAndEditorTextColorAppThemeBindingUpdatesOnThemeChange()
	{
		App.WaitForElement("TestEntry");
		App.WaitForElement("DarkThemeButton");
		App.Tap("DarkThemeButton");
		VerifyScreenshot("EntryAndEditorTextColorAppThemeBindingUpdatesOnDarkTheme");
		App.WaitForElement("LightThemeButton");
		App.Tap("LightThemeButton");
		VerifyScreenshot("EntryAndEditorTextColorAppThemeBindingUpdatesOnLightTheme");
	}

	[Test, Order(2)]
	[Category(UITestCategories.Entry)]
	public void EntryAndEditorPlaceholderTextColorAppThemeBindingUpdatesOnThemeChange()
	{
		App.WaitForElement("TestEntry");
		App.ClearText("TestEntry");

		App.WaitForElement("TestEditor");
		App.ClearText("TestEditor");
#if IOS
		if(App.IsKeyboardShown())
		{
			App.WaitForElement("Done");
			App.Tap("Done");
		}
#endif
		App.WaitForElement("DarkThemeButton");
		App.Tap("DarkThemeButton");
		VerifyScreenshot("EntryAndEditorPlaceholderTextColorAppThemeBindingUpdatesOnDarkTheme");
		App.WaitForElement("LightThemeButton");
		App.Tap("LightThemeButton");
		VerifyScreenshot("EntryAndEditorPlaceholderTextColorAppThemeBindingUpdatesOnLightTheme");
	}
}