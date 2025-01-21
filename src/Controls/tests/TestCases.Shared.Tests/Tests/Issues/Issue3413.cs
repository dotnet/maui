﻿using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue3413 : _IssuesUITest
	{
		public Issue3413(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[iOS] Searchbar in Horizontal Stacklayout doesn't render";

		[Test]
		[Category(UITestCategories.SearchBar)]
		[Category(UITestCategories.Compatibility)]
		public void Issue3413Test()
		{
			App.WaitForElement("srb_vertical");
			VerifyScreenshot();
		}
	}
}