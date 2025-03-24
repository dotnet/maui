using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue21202 : _IssuesUITest
	{
		public Issue21202(TestDevice device) : base(device)
		{
		}

		public override string Issue => "FontImageSource incorrectly sized";

		[Test]
		[Category(UITestCategories.Button)]
		public void Issue21202Test()
		{
			App.WaitForElement("WaitForStubControl");

			VerifyScreenshot();
		}
	}
}