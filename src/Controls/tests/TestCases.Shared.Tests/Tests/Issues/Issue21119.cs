using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue21119 : _IssuesUITest
	{
		public Issue21119(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Search Handler visual and functional bug in subtabs";

		[Test]
		[Category(UITestCategories.Shell)]
		public void UpdateSearchHandlerMenuItemForTabNavigation()
		{
#if WINDOWS
			App.Tap("navViewItem");
			App.WaitForElement("DogsPage");
			App.Tap("DogsPage");
			App.Tap("navViewItem");
			App.WaitForElement("CatsPage");
			App.Tap("CatsPage");
#else
			App.WaitForElement("CatPageButton");
			App.TapTab("DogsPage");
			App.WaitForElement("DogPageButton");
			App.TapTab("CatsPage");
#endif
			VerifyScreenshot();
		}
	}
}