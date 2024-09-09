using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue18242 : _IssuesUITest
	{
		public Issue18242(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Button ImageSource not Scaling as expected";

		[Test]
		[Category(UITestCategories.Button)]
		[FailsOnIOS("iOS will be fixed in https://github.com/dotnet/maui/pull/20953")]
		[FailsOnMac("Catalyst will be fixed in https://github.com/dotnet/maui/pull/20953")]
		public void Issue18242Test()
		{
			App.WaitForElement("WaitForStubControl");

			VerifyScreenshot();
		}
	}
}
