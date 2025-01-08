﻿using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue5535 : _IssuesUITest
	{
		public Issue5535(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "CollectionView: Swapping EmptyViews has no effect";

		[Test]
		[Category(UITestCategories.CollectionView)]
		[FailsOnWindowsWhenRunningOnXamarinUITest]
		public void SwappingEmptyViews()
		{
			App.WaitForElement("FilterItems");
			App.Tap("FilterItems");
			App.EnterText("FilterItems", "abcdef");

			// Default empty view
			App.WaitForElement("Nothing to see here.");

			App.Tap("ToggleEmptyView");

			// Other empty view
			App.WaitForElement("No results matched your filter.");
		}
	}
}