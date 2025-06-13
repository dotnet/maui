using Xunit;
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

		[Fact]
		[Category(UITestCategories.Button)]
		public void Issue18242Test()
		{
			App.WaitForElement("WaitForStubControl");

			VerifyScreenshot();
		}
	}
}
