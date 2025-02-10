#if TEST_FAILS_ON_WINDOWS // Maps Control not supported in Windows
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla39489 : _IssuesUITest
	{
		public Bugzilla39489(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Memory leak when using NavigationPage with Maps";

		[Test]
		[Category(UITestCategories.Maps)]
		[Category(UITestCategories.Performance)]
		[Category(UITestCategories.Compatibility)]
		public void Bugzilla39489Test()
		{
			// Original bug report (https://bugzilla.xamarin.com/show_bug.cgi?id=39489) had a crash (OOM) after 25-30
			// page loads. Obviously it's going to depend heavily on the device and amount of available memory, but
			// if this starts failing before 50 we'll know we've sprung another serious leak
			int iterations = 50;

			for (int n = 0; n < iterations; n++)
			{
				App.WaitForElement("NewPage");
				App.Tap("NewPage");
				App.WaitForElement("NewPage");
				App.TapBackArrow();
			}
		}
	}
}
#endif