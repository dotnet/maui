using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue17366 : _IssuesUITest
	{
		public Issue17366(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Wrong gray color using transparent in iOS gradients";

		[Test]
		[Category(UITestCategories.Brush)]
		public void Issue17366Test()
		{
			// The bug only happens on iOS; see https://github.com/dotnet/maui/pull/17789
			App.WaitForElement("WaitForStubControl");
			VerifyScreenshot();
		}
	}
}
