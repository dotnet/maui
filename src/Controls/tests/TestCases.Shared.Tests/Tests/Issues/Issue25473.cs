﻿#if TEST_FAILS_ON_CATALYST
//In MacCatalyst platform, the Clear Button in the Entry control is not visible during screenshots, but this issue is fixed after enabling the test for verifyScreenshot (https://github.com/dotnet/maui/pull/27531).
using Microsoft.Maui.Platform;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue25473 : _IssuesUITest
	{
		public Issue25473(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "MAUI Entry in Windows always shows ClearButton despite ClearButtonVisibility set to Never";
		[Test]
		[Category(UITestCategories.Entry)]
		public void VerifyEntryClearButtonVisibilitySetToWhileEditing()
		{
			App.WaitForElement("ToggleClearButtonVisibilityButton");
			App.Click("ToggleClearButtonVisibilityButton");
			App.ClearText("MainEntryField");
			App.EnterText("MainEntryField", "ClearButton is set to WhileEditing");
			App.Tap("MainEntryField");
#if ANDROID
			if (App.IsKeyboardShown())
			{
				App.DismissKeyboard();
			}
#endif
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.Entry)]
		public void VerifyEntryClearButtonVisibilitySetToNever()
		{
			App.WaitForElement("ToggleClearButtonVisibilityButton");
			App.Click("ToggleClearButtonVisibilityButton");
			App.ClearText("MainEntryField");
			App.EnterText("MainEntryField", "ClearButton is set to Never");
			App.Tap("MainEntryField");
#if ANDROID
			if (App.IsKeyboardShown())
			{
				App.DismissKeyboard();
			}
#endif
			VerifyScreenshot();
		}
	}
}
#endif