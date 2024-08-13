using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue14708 : _IssuesUITest
	{
		public override string Issue => "Search bar taken whole screen space when it aligns in landscape orientation";

		public Issue14708(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.SearchBar)]
		public void SearchBarScreenSpaceUpdatingAtLandscape()
		{
			App.WaitForElement("WaitForSearchBarControl");
			App.SetOrientationLandscape();
			Task.Delay(1000); // Wait to complete the device rotation animation.
			var rtlEntryRect = App.FindElement("WaitForSearchBarControl").GetRect();
			// Set focus
			App.Click(rtlEntryRect.X, rtlEntryRect.Y);

			// Tap Search Button at right side of search bar
			var margin = 30;
			App.Click(rtlEntryRect.Width + margin, rtlEntryRect.Y + margin);
			VerifyScreenshot();

		}
	}
}