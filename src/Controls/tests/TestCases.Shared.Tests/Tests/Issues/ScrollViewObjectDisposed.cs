﻿#if !ANDROID && !WINDOWS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	[Category(UITestCategories.ScrollView)]
	public class ScrollViewObjectDisposedUITests : _IssuesUITest
	{
		public ScrollViewObjectDisposedUITests(TestDevice device)
		: base(device)
		{ }

		public override string Issue => "Object Disposed Exception in ScrollView";

		// ScrollViewObjectDisposedTest (src\Compatibility\ControlGallery\src\Issues.Shared\ScrollViewObjectDisposed.cs)
		[Test]
		[Description("Tapping a button inside the ScrollView does not cause an exception.")]
		[FailsOnAndroid("This test is failing, likely due to product issue")]
		[FailsOnWindows("This test is failing, likely due to product issue")]
		public void ScrollViewObjectDisposedTest()
		{
			// 1. Tap the button.
			App.Tap("TestButtonId");

			// 2. Verify does not cause an exception.
			App.WaitForElement("Success");
		}
	}
}
#endif