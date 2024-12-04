﻿using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue19283 : _IssuesUITest
	{
		public Issue19283(TestDevice device) : base(device) { }

		public override string Issue => "PointerOver VSM Breaks Button";

		[Test]
		[Category(UITestCategories.Button)]
		public void ButtonStillWorksWhenItHasPointerOverVSMSet()
		{
			App.WaitForElement("btn");
			App.Tap("btn");
			App.WaitForElement("Success");
		}
	}
}