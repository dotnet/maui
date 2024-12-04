﻿#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue4600 : _IssuesUITest
	{
		public Issue4600(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[iOS] CollectionView crash with empty ObservableCollection";

		[Test]
		[Category(UITestCategories.CollectionView)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnIOSWhenRunningOnXamarinUITest]
		public void InitiallyEmptySourceDisplaysAddedItem()
		{
			App.WaitForNoElement("Insert");
			App.Tap("btnInsert");
			App.WaitForNoElement("Inserted");
		}
	}
}
#endif