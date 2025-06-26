﻿using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue27614 : _IssuesUITest
	{
		public Issue27614(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Label not sized correctly on Android";

		[Test, Order(1)]
		[Trait("Category", UITestCategories.Label)]
		public void LabelShouldSizeCorrectlyOnHorizontalStartLayoutOptions()
		{
			App.WaitForElement("Label");
			VerifyScreenshot();
		}

		[Test, Order(2)]
		[Trait("Category", UITestCategories.Label)]
		public void LabelShouldSizeCorrectlyOnHorizontalCenterLayoutOptions()
		{
			App.WaitForElement("CenterButton");
			App.Tap("CenterButton");
			VerifyScreenshot();
		}

		[Test, Order(3)]
		[Trait("Category", UITestCategories.Label)]
		public void LabelShouldSizeCorrectlyOnHorizontalEndLayoutOptions()
		{
			App.WaitForElement("EndButton");
			App.Tap("EndButton");
			VerifyScreenshot();
		}
	}
}