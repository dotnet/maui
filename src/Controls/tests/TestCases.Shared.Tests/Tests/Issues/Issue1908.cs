﻿using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue1908 : _IssuesUITest
	{
		public Issue1908(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Image reuse";

		[Fact]
		[Trait("Category", UITestCategories.Image)]
		[Trait("Category", UITestCategories.Compatibility)]
		public void Issue1908Test()
		{
			App.WaitForElement("OASIS1");
			App.WaitForElement("OASIS2");
			App.WaitForElement("OASIS1");
		}
	}
}
