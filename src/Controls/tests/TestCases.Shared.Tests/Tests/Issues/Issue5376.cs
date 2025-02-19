using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue5376 : _IssuesUITest
	{
		public Issue5376(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Call unfocus entry crashes app";

		[Test]
		[Category(UITestCategories.Entry)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnIOSWhenRunningOnXamarinUITest]
		[FailsOnMacWhenRunningOnXamarinUITest]
		[FailsOnWindowsWhenRunningOnXamarinUITest]
		public void Issue5376Test()
		{
			App.WaitForNoElement("Success");
		}
	}
}
