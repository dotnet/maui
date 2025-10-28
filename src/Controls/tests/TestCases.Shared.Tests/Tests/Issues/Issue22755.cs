#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue22755 : _IssuesUITest
	{
		public Issue22755(TestDevice device) : base(device) { }

		public override string Issue => "[Android] TabbedPage in FlyoutPage in navigationPage does not fit all screen";

		[Test]
		public void Issue22755Test()
		{
			App.WaitForElement("label");

			// The test passes if navigation bar is correctly placed
			VerifyScreenshot();
		}
	}
}
#endif