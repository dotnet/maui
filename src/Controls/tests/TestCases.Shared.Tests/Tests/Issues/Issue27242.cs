#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue27242 : _IssuesUITest
	{
		const string Button1 = "Button1";
		const string Page1Entry = "Page1Entry";
		const string Button2 = "Button2";
		const string Page2Entry = "Page2Entry";

		public Issue27242(TestDevice device) : base(device) { }

		public override string Issue => "[Android] WindowSoftInputModeAdjust is not working for modal pages";

		[Test, Order(1)]
		[Category(UITestCategories.Page)]
		public void WindowSoftInputModeAdjustSetToResizeForModalPage()
		{
			App.WaitForElement(Button1);
			App.Tap(Button1);
			App.WaitForElement(Page1Entry);
			var beforeEntryRectY = App.WaitForElement(Page1Entry).GetRect().Y;
			App.Tap(Page1Entry);
			var afterEntryRectY = App.WaitForElement(Page1Entry).GetRect().Y;
			Assert.That(beforeEntryRectY, Is.Not.EqualTo(afterEntryRectY));
			App.Tap("BackButton");
		}

		[Test, Order(2)]
		[Category(UITestCategories.Page)]
		public void WindowSoftInputModeAdjustSetToPanForModalpage()
		{
			App.WaitForElement(Button2);
			App.Tap(Button2);
			App.WaitForElement(Page2Entry);
			var beforeEntryRectY = App.WaitForElement(Page2Entry).GetRect().Y;
			App.Tap(Page2Entry);
			var afterEntryRectY = App.WaitForElement(Page2Entry).GetRect().Y;
			Assert.That(beforeEntryRectY, Is.EqualTo(afterEntryRectY));
		}
	}
}
#endif