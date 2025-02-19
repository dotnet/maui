#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue2842 : _IssuesUITest
	{
		public Issue2842(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "ViewCell in TableView not adapting to changed size on iOS";

		[Test]
		[Category(UITestCategories.TabbedPage)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnIOSWhenRunningOnXamarinUITest]
		public void Issue2842Test()
		{
			App.WaitForElement("btnClick");
			App.Tap("btnClick");
			App.Screenshot("Verify that the text is not on top of the image");
		}
	}
}
#endif