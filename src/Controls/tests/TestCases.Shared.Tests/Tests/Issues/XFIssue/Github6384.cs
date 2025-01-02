#if TEST_FAILS_ON_ANDROID // Clicking the navigation button causes the app to crash. Also as per documentation, dont use the tabbed page into the shell and navigation page.  
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Github6384 : _IssuesUITest
	{
		public Github6384(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "content page in tabbed page not showing inside shell tab";

		[Test]
		[Category(UITestCategories.Navigation)]
		public void Github6384Test()
		{
			App.WaitForElement("NavigationButton");
			App.Tap("NavigationButton");
			App.WaitForElement("SubTabLabel1");
		}
	}
}
#endif