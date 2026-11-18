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
