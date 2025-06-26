﻿using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla46458 : _IssuesUITest
	{
		public Bugzilla46458(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Grid.IsEnabled property is not working";

		[Fact]
		[Trait("Category", UITestCategories.Layout)]
		[Trait("Category", UITestCategories.Compatibility)]
		public void GridIsEnabled()
		{
			App.WaitForElement("entry");
			App.Tap("entry");
			App.WaitForElement("Success");

			App.WaitForElement("button");
			App.Tap("button");
			App.WaitForElement("Success");

			App.WaitForElement("button1");
			App.Tap("button1");
			App.WaitForElement("Clicked");

			App.WaitForElement("entry");
			App.Tap("entry");
			App.WaitForElement("Success");

			App.WaitForElement("button");
			App.Tap("button");
			App.WaitForElement("Success");
		}
	}
}