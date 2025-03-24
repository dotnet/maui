﻿using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue19152 : _IssuesUITest
	{
		public Issue19152(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Windows | Entry ClearButton not taking color of text";

		[Test]
		[Category(UITestCategories.Entry)]
		public void EntryClearButtonColorShouldMatchTextColor()
		{
			App.WaitForElement("entry");
			App.Tap("button");
#if ANDROID || IOS 
			// On Android and iOS, the virtual keyboard appears inconsistent can cause flaky test results. As this test verifying only the entry clear button color, crop the bottom portion of the screenshot to exclude the keyboard.
			// Using DismissKeyboard(), would unfocus the control in iOS, so we're using cropping instead to maintain focus during testing.
			VerifyScreenshot(cropBottom: 1200);  
#else
			VerifyScreenshot();
#endif
		}
	}
}