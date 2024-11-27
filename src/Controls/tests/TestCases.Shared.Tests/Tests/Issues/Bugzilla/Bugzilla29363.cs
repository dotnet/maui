﻿using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla29363 : _IssuesUITest
	{
		public Bugzilla29363(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "PushModal followed immediate by PopModal crashes";

		[Test]
		[Category(UITestCategories.Navigation)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnIOSWhenRunningOnXamarinUITest]
		[FailsOnMacWhenRunningOnXamarinUITest]
		public void PushButton()
		{
			App.WaitForElement("ModalPushPopTest");
			App.Tap("ModalPushPopTest");
			Thread.Sleep(2000);

			// if it didn't crash, yay
			App.WaitForElement("ModalPushPopTest");
		}
	}
}