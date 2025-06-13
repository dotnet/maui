#if IOS //This sample is specific to the iOS platform and handles text input behavior by accessing the native UITextView.
using System.Drawing;
using System.Globalization;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using OpenQA.Selenium.Appium.Interactions;
using OpenQA.Selenium.Interactions;
using UITest.Appium;
using UITest.Core;
using System.Text;
using Microsoft.Maui.Platform;
using OpenQA.Selenium;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue29643 : _IssuesUITest
{
    public Issue29643(TestDevice device) : base(device) { }

    public override string Issue => "iOS soft keyboard causes CollectionView to jump";

    [Test]
    [Category(UITestCategories.Entry)]
    public async Task KeepEntryAboveKeyboardInCollectionView ()
    {
        var app = App as AppiumApp;
        if (app is null)
        {
            return;
        }
        
        app.ScrollDown("CV", swipePercentage: 0.9, swipeSpeed: 100);
        app.ScrollDown("CV", swipePercentage: 0.9, swipeSpeed: 100);
        
		await AssertVisibleAboveKeyboard(app, "Input72");
		await AssertVisibleAboveKeyboard(app, "Input69");
		await AssertVisibleAboveKeyboard(app, "Input68");
		await AssertVisibleAboveKeyboard(app, "Input67");
		await AssertVisibleAboveKeyboard(app, "Input66");
		await AssertVisibleAboveKeyboard(app, "Input65");
		await AssertVisibleAboveKeyboard(app, "Input62");
		await AssertVisibleAboveKeyboard(app, "Input61");
    }

    static async Task AssertVisibleAboveKeyboard(AppiumApp app, string entryId)
    {
	    app.ScrollDown("CV", swipePercentage: 0.9, swipeSpeed: 100);
	    var entry = await TapEntryAsync(app, entryId);
	    var entryRect = entry.GetRect();
	    var keyboardLocation = KeyboardScrolling.FindiOSKeyboardLocation(app.Driver)!.Value;
	    ClassicAssert.GreaterOrEqual(entryRect.Top, 44);
	    ClassicAssert.LessOrEqual(entryRect.Bottom, keyboardLocation.Y);
	    KeyboardScrolling.HideKeyboard(app, app.Driver, false);
    }

    static async Task<IUIElement> TapEntryAsync(AppiumApp app, string entryId)
    {
	    var entry = app.WaitForElement(entryId);
	    entry.Click();
	    await Task.Delay(100);
	    return entry;
    }
}
#endif
