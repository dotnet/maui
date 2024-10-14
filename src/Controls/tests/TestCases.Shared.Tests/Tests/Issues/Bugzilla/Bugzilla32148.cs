﻿using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla32148 : _IssuesUITest
{
	public Bugzilla32148(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => " Pull to refresh hides the first item on a list view";

	// [Test]
	// [Category(UITestCategories.ListView)]
	// [FailsOnIOS]
	// public void Bugzilla32148Test()
	// {
	// 	App.WaitForElement("Contact0 LastName");
	// 	App.Tap("Search");
	// 	App.WaitForElement("Contact0 LastName");
	// 	App.Screenshot("For manual review, is the first cell visible?");
	// }
}
