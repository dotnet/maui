#if ANDROID //This test case verifies "that the KeyListener is being set to handle numeric keyboard" exclusively on Android platform
using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue21109 : _IssuesUITest
	{
		public Issue21109(TestDevice device) : base(device) { }

		public override string Issue => "[Android] MAUI 8.0.3 -> 8.0.6 regression: custom handler with key listener no longer works";

		[Test, Order(1)]
		[Category(UITestCategories.Entry)]
		public void VerifyInitialEntryReturnTypeChange()
		{
			App.WaitForElement("WaitForStubControl");
			//On InitialLoading returnType should work.
			App.Tap("SearchEntry");
			Thread.Sleep(500); // Add some wait for poping up the keyboard to resolve flakiness in CI.

			// CI tests sometimes fail due to color inconsistency between the keyboard and bottom navigation bar.
#if ANDROID
			VerifyScreenshot(cropTop: 63, cropBottom: 126);
#else
			VerifyScreenshot();
#endif
		}

		[Test, Order(2)]
		[Category(UITestCategories.Entry)]
		public void VerifyDynamicEntryReturnTypeChange()
		{
			App.WaitForElement("WaitForStubControl");

			// Verify that ReturnType works as expected.
			if (App.IsKeyboardShown())
				App.DismissKeyboard();

			var returnType1 = App.FindElement("ReturnTypeResult").GetText();
			App.Tap("ReturnTypeEntry");
			App.EnterText("ReturnTypeEntry", "a");
			var returnType2 = App.FindElement("ReturnTypeResult").GetText();
			ClassicAssert.AreNotEqual(returnType1, returnType2);
		}
	}
}
#endif