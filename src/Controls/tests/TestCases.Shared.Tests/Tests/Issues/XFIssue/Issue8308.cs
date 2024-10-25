﻿using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue8308 : _IssuesUITest
{
	public Issue8308(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Bug] [iOS] Cannot access a disposed object. Object name: 'GroupableItemsViewController`1";

	//[Test]
	//[Category(UITestCategories.CollectionView)]
	//public void NavigatingBackToCollectionViewShouldNotCrash()
	//{
	//	RunningApp.WaitForElement("Instructions");

	//	TapInFlyout("Page 2");
	//	RunningApp.WaitForElement("Instructions2");

	//	TapInFlyout("Page 1");
	//	RunningApp.WaitForElement("Instructions");
	//}
}