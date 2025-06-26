﻿using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue2775 : _IssuesUITest
	{
		public Issue2775(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "ViewCell background conflicts with ListView Semi-Transparent and Transparent backgrounds";

		[Fact]
		[Trait("Category", UITestCategories.ListView)]
		[Trait("Category", UITestCategories.Compatibility)]
		public void Issue2775Test()
		{
			App.WaitForElement("TestReady");
			VerifyScreenshot();
		}
	}
}
