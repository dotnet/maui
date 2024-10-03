#if !MACCATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
    public class Issue13807 : _IssuesUITest
	{
		public Issue13807(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "NavigationPage TitleBar Persists if child pages have a title";

		[Test]
		[Category(UITestCategories.Navigation)]
		public void NavigationBarVisibility()
		{
			App.WaitForElement("label");
			VerifyScreenshot();
		}
	}
}
#endif
