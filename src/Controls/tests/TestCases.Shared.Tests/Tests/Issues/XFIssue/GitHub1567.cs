﻿#if IOS
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class GitHub1567 : _IssuesUITest
	{
		public GitHub1567(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "NRE using TapGestureRecognizer on cell with HasUnevenRows";

		[Fact]
		[Trait("Category", UITestCategories.Gestures)]
		[Trait("Category", UITestCategories.Compatibility)]
		public void GitHub1567Test()
		{
			App.WaitForElement("btnFillData");
			App.Tap("btnFillData");
		}
	}
}
#endif