#if ANDROID || IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue14708 : _IssuesUITest
	{
		public override string Issue => "[Android] Ensure all controls in Layout with Search Bar are visible in Landscape Mode";

		public Issue14708(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.SearchBar)]
		public void AllControlsVisibleOnSearchBtnClickInSearchBarLandscapeOrientation()
		{
			App.WaitForElement("searchBar");
			App.SetOrientationLandscape();
			var searchBarRect = App.FindElement("searchBar").GetRect();
			// Set focus
            App.TapCoordinates(searchBarRect.X+10, searchBarRect.Y+10);
   			App.PressEnter();
            VerifyScreenshot();
		}
	}
}
#endif