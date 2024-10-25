using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla42956 : _IssuesUITest
	{
		const string Success = "Success";

		public Bugzilla42956(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] Exception Ancestor must be provided for all pushes except first";

		// Crash after navigating
		/*
		[Test]
		[Category(UITestCategories.ListView)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnAndroidWhenRunningOnXamarinUITest]
		[FailsOnIOSWhenRunningOnXamarinUITest]
		[FailsOnMacWhenRunningOnXamarinUITest]
		public void Bugzilla42956Test()
		{
			App.WaitForElement(Success);
		}
		*/
	}
}