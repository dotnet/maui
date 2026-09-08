using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla53834 : _IssuesUITest
	{
		public Bugzilla53834(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "incorrect row heights on ios when using groupheadertemplate in Xamarin.Forms 2.3.4.214-pre5";

		[Test]
		[Category(UITestCategories.ListView)]
		public void Bugzilla53834Test()
		{
			App.WaitForElement("TestReady");
			VerifyScreenshot();
		}
	}
}