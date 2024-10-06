using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla41842 : _IssuesUITest
	{
		public Bugzilla41842(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Set FlyoutPage.Detail = New Page() twice will crash the application when set FlyoutLayoutBehavior = FlyoutLayoutBehavior.Split";

		// Crash after navigation
		/*
		[Test]
		[Ignore("The sample is crashing.")]
		[Category(UITestCategories.FlyoutPage)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnAllPlatforms("The sample is crashing. More information: https://github.com/dotnet/maui/issues/21205")]
		public void Bugzilla41842Test()
		{
			App.WaitForElement("Success");
		}
		*/
	}
}