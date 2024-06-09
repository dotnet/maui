using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue22450 : _IssuesUITest
	{
		public override string Issue => "[Android] Custom shell back button is always white in color";

		public Issue22450(TestDevice device) : base(device) { }

		[Test]
		[Category(UITestCategories.Shell)]
		[FailsOnWindowsWhenRunningOnXamarinUITest("The golden tests cut the title bar where the back button appears.")]
		public void BackButtonIconShouldBeBlue()
		{
			App.WaitForElement("label");

			// The test passes if the back button ison is blue
			VerifyScreenshot();
		}
	}
}
