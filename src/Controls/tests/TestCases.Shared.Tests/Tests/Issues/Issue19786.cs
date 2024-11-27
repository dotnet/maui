﻿using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue19786 : _IssuesUITest
	{
		public Issue19786(TestDevice device) : base(device)
		{
		}

		public override string Issue => "[Android] Crash removing item from CarouselView";

		[Test]
		[Category(UITestCategories.CarouselView)]
		[FailsOnWindowsWhenRunningOnXamarinUITest("This test is failing, likely due to product issue")]
		public void RemovingItemsShouldNotCauseCrash()
		{
			_ = App.WaitForElement("addItemButton");
			App.Tap("addItemButton");
			App.Tap("addItemButton");
			App.Tap("addItemButton");
			App.Tap("goToNextItemButton");
			App.Tap("goToNextItemButton");
			App.Tap("removeLastItemButton");
			App.Tap("removeLastItemButton");
			App.Tap("removeLastItemButton");
		}
	}
}
