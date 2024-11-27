﻿using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue7102 : _IssuesUITest
	{
		public Issue7102(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] CollectionView Header cause delay to adding items.";

		[Test]
		[Category(UITestCategories.CollectionView)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnIOSWhenRunningOnXamarinUITest]
		[FailsOnMacWhenRunningOnXamarinUITest]
		public void HeaderDoesNotBreakIndexes()
		{
			App.WaitForElement("entryInsert");
			App.Tap("entryInsert");
			App.ClearText("entryInsert");
			App.EnterText("entryInsert", "1");
			App.Tap("btnInsert");

			// If the bug is still present, then there will be 
			// two "Item: 0" items instead of the newly inserted item
			// Or the header will have disappeared
			App.WaitForNoElement("Inserted");
			App.WaitForNoElement("This is the header");
		}
	}
}
