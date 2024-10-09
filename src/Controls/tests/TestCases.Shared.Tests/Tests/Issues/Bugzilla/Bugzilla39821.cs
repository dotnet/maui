﻿using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla39821 : _IssuesUITest
	{
		public Bugzilla39821(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "ViewExtension.TranslateTo cannot be invoked on Main thread";

		[Test]
		[Category(UITestCategories.Animation)]
		[Category(UITestCategories.Compatibility)]
		[Ignore("Fails intermittently")]
		public void DoesNotCrash()
		{
			App.Tap("Animate");
			App.WaitForNoElement("Success", timeout: TimeSpan.FromSeconds(25));
		}
	}
}