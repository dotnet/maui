﻿#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST	  //When clicking Button0 (the first button) incorrectly centers it in the ScrollView instead of maintaining its left alignment.
//Issue Link: https://github.com/dotnet/maui/issues/26760
using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	[Category(UITestCategories.ScrollView)]
	public class Bugzilla44461UITests : _IssuesUITest
	{
		public Bugzilla44461UITests(TestDevice device)
			: base(device)
		{
		}

		public override string Issue => "ScrollToPosition.Center works differently on Android and iOS";

		// Bugzilla44461 (src\Compatibility\ControlGallery\src\Issues.Shared\Bugzilla44461.cs)
		[Test]
		public void Bugzilla44461Test()
		{
			var positions = TapButton(0);
			ClassicAssert.AreEqual(positions.initialPosition.X, positions.finalPosition.X);
			ClassicAssert.LessOrEqual(positions.finalPosition.X, 1);
			VerifyScreenshot();
		}

		(System.Drawing.Rectangle initialPosition, System.Drawing.Rectangle finalPosition) TapButton(int position)
		{
			var buttonId = $"{position}";
			App.WaitForElement(buttonId);
			var initialPosition = App.FindElement(buttonId).GetRect();
			App.Tap(buttonId);
			var finalPosition = App.FindElement(buttonId).GetRect();
			return (initialPosition, finalPosition);
		}
	}
}
#endif