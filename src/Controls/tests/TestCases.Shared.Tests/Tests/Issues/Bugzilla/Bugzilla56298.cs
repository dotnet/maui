﻿#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_CATALYST
// ListView.HasUnevenRows Property Changes Not Reflected in UI on Android and Windows Platforms. Issue Link: https://github.com/dotnet/maui/issues/26780
// The CI-generated image on Catalyst doesn't match expectations, seems to be uneven rows aren't working correctly.
// However, the locally generated image appears to function as expected.

using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla56298 : _IssuesUITest
	{
		public Bugzilla56298(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Changing ListViews HasUnevenRows at runtime on iOS has no effect";

		[Test]
		[Category(UITestCategories.ListView)]
		[FailsOnAndroidWhenRunningOnXamarinUITest("This test is failing, likely due to product issue")]
		[FailsOnWindowsWhenRunningOnXamarinUITest("This test is failing, likely due to product issue")]
		public void Bugzilla56298Test()
		{
			App.WaitForElement("btnAdd");
			App.Tap("btnAdd");
			App.WaitForElement("btnToggle");
			App.Tap("btnToggle");
			VerifyScreenshot();
		}
	}
}
#endif