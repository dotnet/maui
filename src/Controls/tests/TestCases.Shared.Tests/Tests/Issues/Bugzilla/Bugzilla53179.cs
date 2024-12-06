using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla53179 : _IssuesUITest
	{
		public Bugzilla53179(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "PopAsync crashing after RemovePage when support packages are updated to 25.1.1";

		[Test]
		[Category(UITestCategories.Navigation)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnAllPlatformsWhenRunningOnXamarinUITest]
		public void Bugzilla53179Test()
		{
			App.WaitForElement("Next Page");
			App.Tap("Next Page");

			App.WaitForElement("Next Page");
			App.Tap("Next Page");

			App.WaitForElement("Next Page");
			App.Tap("Next Page");

			App.WaitForElement("Remove previous pages");
			App.Tap("Remove previous pages");

			App.WaitForElement("Back");
			App.Tap("Back");
		}
	}
}