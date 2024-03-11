using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace UITests
{
    internal class Bugzilla53834 : IssuesUITest
	{
		public Issue10182(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "incorrect row heights on ios when using groupheadertemplate in Xamarin.Forms 2.3.4.214-pre5";

		[Test]
		[Category(UITestCategories.ListView)]
		public void Bugzilla53834Test()
		{
			RunningApp.WaitForElement("TestReady");
			RunningApp.Screenshot("Incorrect row heights test");
		}
	}
}