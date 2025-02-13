﻿using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla39636 : _IssuesUITest
	{
		public Bugzilla39636(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Cannot use XamlC with OnPlatform in resources, it throws System.InvalidCastException";

		[Test, Retry(2)]
		[Category(UITestCategories.LifeCycle)]
		[Category(UITestCategories.Compatibility)]
		public void DoesNotCrash()
		{
			App.WaitForElement("Success");
		}
	}
}