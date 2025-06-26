﻿using Microsoft.Maui.Platform;
using Xunit;
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
		[Fact]
		[Trait("Category", UITestCategories.Entry)]
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

#if IOS //Inconsistent keyboard visibility issue in iOS CI environments can cause test flakiness. As this test validate the clear button visibility only, so the keyboard is not mandatory.
			VerifyScreenshot(cropBottom: 1200);
#else
			VerifyScreenshot();
#endif
		}

		[Fact]
		[Trait("Category", UITestCategories.Entry)]
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

#if IOS //Inconsistent keyboard visibility issue in iOS CI environments can cause test flakiness. As this test validate the clear button visibility only, so the keyboard is not mandatory.
			VerifyScreenshot(cropBottom: 1200);
#else
			VerifyScreenshot();
#endif
		}
	}
}