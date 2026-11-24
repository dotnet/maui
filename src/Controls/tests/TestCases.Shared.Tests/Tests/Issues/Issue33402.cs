using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33402 : _IssuesUITest
{
	public Issue33402(TestDevice device) : base(device) { }

	public override string Issue => "Show ContextFlyout on long press on mobile platforms";

	[Test, Order(1)]
	[Category(UITestCategories.ContextActions)]
	public void ContextFlyoutRunsBoundCommand()
	{
		OpenContextFlyout("MenuLabel");
		TapMenuItem("Copy item");

		Assert.That(App.WaitForElement("StatusLabel").GetText(), Is.EqualTo("copy"));
	}

	[Test, Order(2)]
	[Category(UITestCategories.ContextActions)]
	public void ContextFlyoutSubItemRunsBoundCommand()
	{
		OpenContextFlyout("MenuLabel");
		TapMenuItem("More");
		TapMenuItem("Archive");

		Assert.That(App.WaitForElement("StatusLabel").GetText(), Is.EqualTo("archive"));
	}

	[Test, Order(3)]
	[Category(UITestCategories.ContextActions)]
	public void ContextFlyoutOnChildDoesNotBlockParentTap()
	{
		App.Tap("InnerLabel");
		Assert.That(App.WaitForElement("StatusLabel").GetText(), Is.EqualTo("border-tapped"));

		OpenContextFlyout("InnerLabel");
		TapMenuItem("Inner action");
		Assert.That(App.WaitForElement("StatusLabel").GetText(), Is.EqualTo("inner"));
	}

	[Test, Order(4)]
	[Category(UITestCategories.ContextActions)]
	public void ContextFlyoutOnCollectionViewItemKeepsSelectionWorking()
	{
		App.Tap("Item1");
		Assert.That(App.WaitForElement("StatusLabel").GetText(), Is.EqualTo("selected:Item1"));

		OpenContextFlyout("Item2");
		TapMenuItem("Delete");
		Assert.That(App.WaitForElement("StatusLabel").GetText(), Is.EqualTo("deleted:Item2"));
		App.WaitForNoElement("Item2");
	}

#if ANDROID || IOS
	// A long press on a view with both a long-press gesture and a context menu does what the platform does:
	// Android runs the view's own long-press handler, iOS opens the context menu.
	[Test, Order(5)]
	[Category(UITestCategories.ContextActions)]
	public void LongPressOnViewWithGestureFollowsPlatform()
	{
		App.WaitForElement("LongPressLabel");
		App.LongPress("LongPressLabel");

#if ANDROID
		Assert.That(App.WaitForElement("StatusLabel").GetText(), Is.EqualTo("long-pressed"));
		App.WaitForNoElement(MenuItem("Gesture menu item"));
#else
		TapMenuItem("Gesture menu item");
		Assert.That(App.WaitForElement("StatusLabel").GetText(), Is.EqualTo("gesture-menu"));
#endif
	}
#endif

#if ANDROID || IOS
	// Text keeps its own selection, like in native apps; the flyout items join the text's menu
	[Test, Order(6)]
	[Category(UITestCategories.ContextActions)]
	public void LongPressOnEntryKeepsTextSelectionAndShowsFlyoutItems()
	{
		App.WaitForElement("FlyoutEntry");

#if ANDROID
		// The text toolbar is a window of its own, which the page source only includes with this setting
		var driver = (OpenQA.Selenium.Appium.Android.AndroidDriver)((AppiumApp)App).Driver;
		driver.SetSetting("enableMultiWindows", true);
		try
		{
#endif
			// A long press selects a word on Android; on iOS it moves the cursor and a double tap selects a word
#if ANDROID
			App.LongPress("FlyoutEntry");
#else
			App.DoubleTap("FlyoutEntry");
#endif

			// The system's text menu can put extra items on another page or in an overflow menu
#if ANDROID
			const string more = "//*[@content-desc='More options']";
			App.WaitForElement(AppiumQuery.ByXPath($"//*[@text='Entry item'] | {more}"));
#else
			const string more = "//*[@name='Next Page' or @name='Forward']";
			App.WaitForElement(AppiumQuery.ByXPath($"//*[@name='Entry item' or @label='Entry item'] | {more}"));
#endif
			if (App.FindElements(MenuItem("Entry item")).Count == 0)
				App.WaitForElement(AppiumQuery.ByXPath(more)).Tap();

			TapMenuItem("Entry item");
			Assert.That(App.WaitForElement("StatusLabel").GetText(), Is.EqualTo("entry-item"));
#if ANDROID
		}
		finally
		{
			driver.SetSetting("enableMultiWindows", false);
		}
#endif
	}
#endif

	void OpenContextFlyout(string automationId)
	{
		App.WaitForElement(automationId);
#if MACCATALYST || WINDOWS
		App.RightClick(automationId);
#else
		App.LongPress(automationId);
#endif
	}

	void TapMenuItem(string text) =>
		App.WaitForElement(MenuItem(text)).Tap();

	static IQuery MenuItem(string text) =>
#if ANDROID
		AppiumQuery.ByXPath($"//*[@text='{text}']");
#elif WINDOWS
		AppiumQuery.ByXPath($"//*[@Name='{text}']");
#elif MACCATALYST
		AppiumQuery.ByXPath($"//*[@title='{text}' or @label='{text}']");
#else
		AppiumQuery.ByXPath($"//*[@name='{text}' or @label='{text}']");
#endif
}
