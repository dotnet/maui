# if IOS		//More info : https://github.com/dotnet/maui/pull/34510
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32275 : _IssuesUITest
{
	const string FlyoutItem = "FlyoutItem";
	const string ResetButton = "Reset";
	public override string Issue => "Shell Flyout SafeArea Rendering";

	protected override bool ResetAfterEachTest => true;

	public Issue32275(TestDevice device) : base(device) { }

	// Test 1: Open flyout with default items and capture screenshot
	[Test, Order(1)]
	[Category(UITestCategories.SafeAreaEdges)]
	public void VerifyDefaultFlyoutItemsRendering()
	{
		App.WaitForElement("PageLoaded");
		App.ShowFlyout();
		VerifyScreenshot();
	}

	// Test 2: Open flyout with header/footer and capture screenshot
	[Test, Order(2)]
	[Category(UITestCategories.SafeAreaEdges)]
	public void VerifyFlyoutWithHeaderFooter()
	{
		App.WaitForElement("ToggleHeaderFooter");
		App.Tap("ToggleHeaderFooter");
		App.WaitForElement("PageLoaded");
		App.ShowFlyout();
		App.WaitForElement("Header View");
		App.WaitForElement("Footer View");
		VerifyScreenshot();
	}

	// Test 3: Tap ToggleFlyoutContentTemplate, then open flyout and capture screenshot
	[Test, Order(3)]
	[Category(UITestCategories.SafeAreaEdges)]
	public void VerifyCustomFlyoutContentTemplateRendering()
	{
		App.WaitForElement("ToggleFlyoutContentTemplate");
		App.Tap("ToggleFlyoutContentTemplate");
		App.WaitForElement("PageLoaded");
		App.ShowFlyout();
		VerifyScreenshot();
	}

	// Test 4: ToggleFlyoutContentTemplate + header/footer, open flyout and capture screenshot
	[Test, Order(4)]
	[Category(UITestCategories.SafeAreaEdges)]
	public void VerifyCustomFlyoutContentTemplateWithHeaderFooter()
	{
		App.WaitForElement("ToggleFlyoutContentTemplate");
		App.Tap("ToggleFlyoutContentTemplate");
		App.WaitForElement("ToggleHeaderFooter");
		App.Tap("ToggleHeaderFooter");
		App.WaitForElement("PageLoaded");
		App.ShowFlyout();
		App.WaitForElement("Header View");
		App.WaitForElement("Footer View");
		VerifyScreenshot();
	}

	// Test 5: Tap Toggle Flyout Content, open flyout, capture screenshot
	[Test, Order(5)]
	[Category(UITestCategories.SafeAreaEdges)]
	public void VerifyCustomFlyoutContentRendering()
	{
		App.WaitForElement("ToggleContent");
		App.Tap("ToggleContent");
		App.WaitForElement("PageLoaded");
		App.ShowFlyout();
		App.WaitForElement("ContentView");
		VerifyScreenshot();
	}

	// Test 6: Toggle Flyout Content + header/footer, open flyout, capture screenshot
	[Test, Order(6)]
	[Category(UITestCategories.SafeAreaEdges)]
	public void VerifyCustomFlyoutContentWithHeaderFooter()
	{
		App.WaitForElement("ToggleContent");
		App.Tap("ToggleContent");
		App.WaitForElement("ToggleHeaderFooter");
		App.Tap("ToggleHeaderFooter");
		App.WaitForElement("PageLoaded");
		App.ShowFlyout();
		App.WaitForElement("ContentView");
		App.WaitForElement("Header View");
		App.WaitForElement("Footer View");
		VerifyScreenshot();
	}
}
#endif