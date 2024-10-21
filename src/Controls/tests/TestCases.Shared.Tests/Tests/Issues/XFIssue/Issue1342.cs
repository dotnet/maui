﻿using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue1342 : _IssuesUITest
{
	const string Success = "Success";

	public Issue1342(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[iOS] ListView throws Exception on ObservableCollection.Add/Remove for non visible list view";

	// [Test]
	// [Category(UITestCategories.ListView)]
	// [Ignore("Fails sometimes - needs a better test")]
	// public void AddingItemsToNonVisibleListViewDoesntCrash()
	// {
	// 	RunningApp.Tap(add2);
	// 	RunningApp.Tap(add3);
	// 	RunningApp.WaitForElement(success);
	// }
}
