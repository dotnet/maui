﻿using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue342NoSource : _IssuesUITest
	{
		public Issue342NoSource(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "NRE when Image is not assigned source";

		[Fact]
		[Trait("Category", UITestCategories.Page)]
		[Trait("Category", UITestCategories.Compatibility)]
		public void Issue342NoSourceTestsLablePresentNoImage()
		{
			App.WaitForElement("Uninitialized image");
		}
	}

	public class Issue342DelayedSource : _IssuesUITest
	{
		public Issue342DelayedSource(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "NRE when Image is delayed source";

		[Fact]
		[Trait("Category", UITestCategories.Page)]
		public void Issue342DelayedLoadTestsImageLoads()
		{
			App.WaitForElement("Delayed image");
		}
	}
}