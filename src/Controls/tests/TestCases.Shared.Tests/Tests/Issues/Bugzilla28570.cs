#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla28570 : _IssuesUITest
	{
		public Bugzilla28570(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "https://bugzilla.xamarin.com/show_bug.cgi?id=28570";

		[Test]
		[Category(UITestCategories.Navigation)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnAndroid("Scroll to end not working.")]
		public void Bugzilla28570Test()
		{
			App.WaitForElement("Tap");
			App.Screenshot("At test page");
			App.Tap("Tap");

			App.WaitForElement("28570Target");
		}
	}
}
#endif