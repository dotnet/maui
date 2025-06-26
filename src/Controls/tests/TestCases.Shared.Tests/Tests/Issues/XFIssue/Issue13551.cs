﻿using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue13551 : _IssuesUITest
	{
		const string Success1 = "Success1";
		const string Success2 = "Success2";

		public Issue13551(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] [iOS] CollectionView does not display items if `IsVisible` modified via a binding/trigger";

		[Fact]
		[Trait("Category", UITestCategories.CollectionView)]
		[Trait("Category", UITestCategories.Compatibility)]
		public void CollectionViewWithFooterShouldNotCrashOnDisplay()
		{
			App.WaitForElement(Success1);
			App.WaitForElement(Success2);
		}
	}
}