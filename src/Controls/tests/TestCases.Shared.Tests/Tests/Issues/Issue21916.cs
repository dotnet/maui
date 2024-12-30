#if TEST_FAILS_ON_ANDROID //more information: https://github.com/dotnet/maui/issues/26050
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue21916 : _IssuesUITest
	{
		public Issue21916(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Page and control Unloaded events firing on iOS when navigating to another page"; 
		
		[Test]
		[Category(UITestCategories.Shell)]
		public void Shell_Issue21916()
		{
			App.WaitForElement("Button");
			App.Click("Button");
			App.WaitForElement("Button");
			App.Click("Button");
			VerifyScreenshot();
		}
	}
}
#endif