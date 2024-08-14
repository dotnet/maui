#if ANDROID 
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue14708 : _IssuesUITest
	{
		public override string Issue => "[Android] SearchBar taken whole screen space when it aligns in landscape orientation";

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
			var searchBarRect = App.FindElement("WaitForSearchBarControl").GetRect();
			
			// Set focus
            App.TapCoordinates(searchBarRect.X+10, searchBarRect.Y+10);

            // Tap Search Button at right side of search bar
            var margin = 20;
            App.TapCoordinates(searchBarRect.X + searchBarRect.Width + margin, (searchBarRect.Y /2) + (searchBarRect.Height / 2));
            
			Task.Delay(1000); // Wait to complete the device rotation animation.
            VerifyScreenshot();
		}
	}
}
#endif